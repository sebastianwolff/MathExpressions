using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using MathExpressinTests;
using System.Linq;
using System.Diagnostics;

/** 
 * EBNF-alike syntax:
 * ==================
 * 
 * Expression   ::= PrimaryExpr
 * PrimaryExpr  ::= LogicalExpr | ContainsExpr
 * ContainsExpr ::= Expression 'INNERHALB' 
 *                  ( Expression 'UND' Expression
 *                  | RangeExpr )
 * LogicalExpr  ::= RelExpr [LOGIC RelExpr]
 * LOGIC        ::= 'UND' | 'ODER' | 'XODER'
 * RelExpr      ::= RangeExpr [RELOP RangeExpr]...
 * RangeExpr    ::= AddExpr ['..' AddExpr]
 * RELOP        ::= '<' | '<=' | '=' | '<>' | '>=' | '>'
 * AddExpr      ::= MulExpr [ADDOP MulExpr]...
 * ADDOP        ::= '+' | '-'
 * MulExpr      ::= PowExpr [MULOP PowExpr]...
 * MULOP        ::= '*' | '/'
 * PowExpr      ::= QualExpr ['^' QualExpr]...
 * QualExpr     ::= ValueExpr ['.' ValueExpr]...
 * ValueExpr    ::= NUMBER
 *                | DATE
 *                | TEXT
 *                | VARIABLE
 *                | Conditional
 *                | '(' Expression ')'
 *                | '[' Expression ']'
 * NUMBER       ::= DIGIT... [('.' | ',') DIGIT...]
 * DIGIT        ::= '0'..'9'
 * DATE_TIME    ::= DATE [TIME]
 * DATE         ::= DIGIT [DIGIT] '.' DIGIT [DIGIT] '.' DIGIT DIGIT [DIGIT DIGIT] TIME
 * TIME         ::= DIGIT DIGIT ':' DIGIT DIGIT ':' DIGIT DIGIT
 * VARIABLE     ::= ALPHA_ [ALPHA_ | DIGIT]...
 * ALPHA_       ::= 'a'..'z' | 'A'..'Z' | '_'
 * TEXT          ::= (\' [^\']*? \') | (\" [^\"]*? \")
 * Conditional  ::= 'WENN' Expression 'DANN' Expression ['SONST' Expression]
 */
namespace Expressionator.Expressions.Builder
{
	/// <summary>
	/// This is the standard implementation of the IExpressionBuilder 
	/// being responsible for parsing input strings and 
	/// compiling equivalent expression trees from these inputs.
	/// </summary>
	public  class ExpressionBuilder : IExpressionBuilder
	{
		/// <summary>
		/// Use this method for convience
		/// </summary>
		/// <param name="pattern">the expression in form of a mathematical pattern</param>
		/// <param name="culture">Cultureinfo for Localization of Numbers and Dates</param>
		/// <returns>the compiled expression tree</returns>
		public static Node ParseExpression(string pattern, CultureInfo culture = null)
		{
			culture ??= CultureInfo.InvariantCulture;
			var builder = new ExpressionBuilder(culture);

			return builder.Parse(pattern);
		}

		#region IExpressionBuilder Members
		public Node Parse(string expressionPattern)
		{
			InitializeLexer(expressionPattern);

			Node result = Expression(false);

			if (currentToken.TokenType != Token.Type.None)
				throw new UnexpectedTokenException(currentToken.StringValue, currentToken.Line, currentToken.Column);
			
			return result;
		}
		#endregion

		#region syntactical analyzer
		private Node Expression(bool get)
		{
			return PrimaryExpr(get);
		}

		private Node PrimaryExpr(bool get)
		{
			Node expr = LogicalExpr(get);

			if (currentToken.TokenType == Token.Type.Contains)
			{
				Node low = RangeExpr(true);

				if (low.GetType() == typeof(RangeNode))
				{
					expr = new ContainsNode(expr, (RangeNode)low);
				}
				else
				{
					Consume(Token.Type.And);

					Node high = RangeExpr(false);

					expr = new ContainsNode(expr, new RangeNode(low, high));
				}
			}

			return expr;
		}

		// logicalExpr ::= relExpr [LOGIC relExpr]...
		// LOGIC ::= 'AND' | 'OR' | 'XOR'
		private Node LogicalExpr(bool get)
		{
			Node result = RelExpr(get);

			while (true)
			{
				switch (currentToken.TokenType)
				{
					case Token.Type.And:
					case Token.Type.Or:
					case Token.Type.XOr:
						result = new CompareNode(TokenType2Compare(currentToken.TokenType), result, RelExpr(true));
						break;
					default:
						return result;
				}
			}
		}

		// relExpr ::= addExpr RELOP addExpr
		// RELOP ::= '<' | '<=' | '=' | '<>' | '>=' | '>'
		private Node RelExpr(bool get)
		{
			Node result = RangeExpr(get);

			switch (currentToken.TokenType)
			{
				case Token.Type.Less:
				case Token.Type.LessEqual:
				case Token.Type.Equal:
				case Token.Type.UnEqual:
				case Token.Type.GreaterEqual:
				case Token.Type.Greater:
					result = new CompareNode(TokenType2Compare(currentToken.TokenType), result, RangeExpr(true));
					break;
				default:
					break;
			}
			return result;
		}

		private Node RangeExpr(bool get)
		{
			Node result = AddExpr(get);

			if (currentToken.TokenType == Token.Type.DblPeriod)
				result = new RangeNode(result, AddExpr(true));

			return result;
		}

		private static CompareNode.CompareTypes TokenType2Compare(Token.Type type)
		{
			switch (type)
			{
				case Token.Type.Less: return CompareNode.CompareTypes.Less;
				case Token.Type.LessEqual: return CompareNode.CompareTypes.LessEqual;
				case Token.Type.Equal: return CompareNode.CompareTypes.Equal;
				case Token.Type.UnEqual: return CompareNode.CompareTypes.UnEqual;
				case Token.Type.GreaterEqual: return CompareNode.CompareTypes.GreaterEqual;
				case Token.Type.Greater: return CompareNode.CompareTypes.Greater;
				case Token.Type.And: return CompareNode.CompareTypes.And;
				case Token.Type.Or: return CompareNode.CompareTypes.Or;
				case Token.Type.XOr: return CompareNode.CompareTypes.XOr;
				default:
					throw new Exception("Internal error: Invalid token-to-compare conversion.");
			}
		}

		private Node AddExpr(bool get)
		{
			Node result = MulExpr(get);

			while (true)
			{
				switch (currentToken.TokenType)
				{
					case Token.Type.Plus:
						result = new AddNode(result, MulExpr(true));
						break;
					case Token.Type.Neg:
						result = new AddNode(result, new NegNode(MulExpr(true)));
						break;
					default:
						return result;
				}
			}
		}

		/// <summary>
		/// term ::= prim { ('*' / '/') prim } ...
		/// </summary>
		/// <param name="get">consume next token before parsing or not</param>
		/// <returns>the equvalent expression tree fragment</returns>
		private Node MulExpr(bool get)
		{
			Node result = PowExpr(get);

			while (true)
			{
				switch (currentToken.TokenType)
				{
					case Token.Type.Mul:
						result = new MulNode(result, PowExpr(true));
						break;
					case Token.Type.Div:
						result = new DivNode(result, PowExpr(true));
						break;
					default:
						return result;
				}
			}
		}

		private Node PowExpr(bool get)
		{
			Node result = QualExpr(get);

			while (currentToken.TokenType == Token.Type.Pow)
				result = new PowNode(result, QualExpr(true));

			return result;
		}

		private Node QualExpr(bool get)
		{
			Node result = ValueExpr(get);

			while (currentToken.TokenType == Token.Type.Period)
			{
				NextToken();

				if (currentToken.TokenType == Token.Type.Symbol)
				{
					if (currentToken.StringValue == "tag")
						result = new DateQualExpr(DateQualExpr.Quals.Day, result);
					else if (currentToken.StringValue == "monat")
						result = new DateQualExpr(DateQualExpr.Quals.Month, result);
					else if (currentToken.StringValue == "jahr")
						result = new DateQualExpr(DateQualExpr.Quals.Year, result);
					else
						continue;

					NextToken(); // consume qualification symbol
				}
				else
					throw new UnexpectedTokenException("Diese Art von Qualifikation nicht unterstuetzt", line, column);
			}

			return result;
		}

		// ValueExpr   ::= NUMBER 
		//               | DATE
		//               | '(' Expression ')'
		//               | '[' Expreession ']'
		//               | VARIABLE
		//               | Conditional
		// NUMBER      ::= DIGIT ['.' DIGIT...] 
		//               | [DIGIT] '.' DIGIT...
		// DIGIT       ::= '0'..'9'
		// VARIABLE    ::= SYMBOL
		// SYMBOL      ::= ALPHA_ [ALPHA_ | DIGIT]...
		// ALPHA_      ::= 'a'..'z' | 'A'..'Z' | '_'
		// Conditional ::= 'WENN' Expression 'DANN' Expression ['SONST' Expression]
		// TEXT          ::= (\' [^\']*? \') | (\" [^\"]*? \")
		private Node ValueExpr(bool get)
		{
			if (get)
				NextToken();

			switch (currentToken.TokenType)
			{
				case Token.Type.Number:
				{
					Node result = new NumberNode(currentToken.NumberValue);
					NextToken();
					return result;
				}
				case Token.Type.RndOpen:
				{
					Node result = Expression(true);
					Consume(Token.Type.RndClose);
					return result;
				}
				case Token.Type.BrOpen:
				{
					Node result = Expression(true);
					Consume(Token.Type.BrClose);
					return result;
				}
				case Token.Type.If:
					return Conditional(true);
				case Token.Type.Plus:
					return ValueExpr(true);
				case Token.Type.Neg:
					return new NegNode(ValueExpr(true));
				case Token.Type.Symbol:
				{
					string symbolName = currentToken.StringValue;
					NextToken();

					if (currentToken.TokenType == Token.Type.RndOpen)
					{
						Node arg = Expression(true);
						Consume(Token.Type.RndClose);
						var symbol = symbolName.ToLower().StartsWith("round") || symbolName.ToLower().StartsWith("runden") ? "round" : symbolName.ToLower();

						switch (symbol) {
							case "tage":
							case "days":
								return new TimeSpanCastExpr(TimeSpanCastExpr.Units.Day, arg);
							case "monate":
							case "months":
									return new TimeSpanCastExpr(TimeSpanCastExpr.Units.Month, arg);
							case "jahre":
							case "years":
									return new TimeSpanCastExpr(TimeSpanCastExpr.Units.Year, arg);
							case "hours":
							case "stunden":
								return new TimeSpanCastExpr(TimeSpanCastExpr.Units.Hour, arg);
							case "minutes":
							case "minuten":
								return new TimeSpanCastExpr(TimeSpanCastExpr.Units.Minute, arg);
							case "sekunden":
							case "seconds":
								return new TimeSpanCastExpr(TimeSpanCastExpr.Units.Second, arg);


							case "jahr":
							case "year":
								return new DateQualExpr(DateQualExpr.Quals.Year, arg);
							case "monat":
							case "month":
									return new DateQualExpr(DateQualExpr.Quals.Month, arg);
							case "tag":
							case "day":
									return new DateQualExpr(DateQualExpr.Quals.Day, arg);
							case "stunde":
							case "hour":
								return new DateQualExpr(DateQualExpr.Quals.Hour, arg);
							case "minute":
								return new DateQualExpr(DateQualExpr.Quals.Minute, arg);
							case "second":
							case "sekunde":
								return new DateQualExpr(DateQualExpr.Quals.Second, arg);
							
							case "round":
									var precisionString = symbolName.ToLower().Replace("round", "");

									precisionString = (string.IsNullOrEmpty(precisionString))
										? "0"
										: precisionString;

									if (!int.TryParse(precisionString, out int precision))
                                    	throw new UnknownPrecisionException($"{precisionString}");
									
									return new RoundCastExpr(precision, arg);
						
						}

						throw new Exception(String.Format("Unknown Function: '{0}'", symbolName));
					}
					else
						return new VariableNode(symbolName, true);
				}
				case Token.Type.Date:
				{
					Node result = new DateExpr(currentToken.DateValue);
					NextToken();
					return result;
				}
				case Token.Type.Time:
					{
						Node result = new TimeExpr(currentToken.DateTimeValue);
						NextToken();
						return result;
					}
				case Token.Type.Text: {
					Node result = new TextNode(currentToken.StringValue);
					NextToken();
					return result;
				}
				default:
					throw new UnexpectedTokenException(currentToken.StringValue, currentToken.Line, currentToken.Column);
			}
		}

		private Node Conditional(bool get)
		{
			if (get)
				Consume(Token.Type.If);

			Node condExpr = Expression(false);

			Consume(Token.Type.Then);

			Node thenExpr = Expression(false);

			Node elseExpr = (currentToken.TokenType == Token.Type.Else) 
				? Expression(true) 
				: null;

			return new CondExpr(condExpr, thenExpr, elseExpr);
		}

		/// <summary>
		/// consumes a given token-type as ensuring wether given token is available, 
		/// if so, switches over to the next token, and throws otherwise.
		/// </summary>
		/// <param name="tokenType">the token-type to consume</param>
		private void Consume(Token.Type tokenType)
		{
			Consume(tokenType, true);
		}

		private void Consume(Token.Type tokenType, bool get)
		{
			if (currentToken.TokenType != tokenType)
				throw new UnexpectedTokenException(currentToken.StringValue, currentToken.Line, currentToken.Column);

			if (get)
				NextToken();
		}
		#endregion

		#region lexical analizing
		private char[] input;
		private int line;
		private int column;
		private int currentChar;
		private long currentPos = 0;
		private Token currentToken;
	
		private int PeekedNextChar
		{
			get
			{
				return input.Peek(currentPos);
			}
		}

		

        private CultureInfo Culture { get; set; }

		private void InitializeLexer(string pattern)
		{
			input = pattern.ToCharArray();
			
			line = 1;
			column = 0;
			currentChar = 0;

			NextChar();
			NextToken();
		}

		private int NextChar()
		{
			if (currentChar == '\n')
			{
				++line;
				column = 1;
			}
			else
			{
				++column;
			}
			return currentChar = input.Read(ref currentPos);
		}

		/// <summary>
		/// Indicates if the Char is not Part of a Number or Date
		/// </summary>
		/// <param name="currentChar">The Char</param>
		/// <param name="couldBeDate">Could the current char series be a DateTime</param>
		/// <returns></returns>
		public bool IsIntDateStopToken(int currentChar, bool couldBeDate)
        {
			
			List<int> expect = new() { Culture.NumberFormat.NumberDecimalSeparator[0], Culture.NumberFormat.NumberGroupSeparator[0] };

			//Prevent Misinterpretation of "/" as DatePart instead of Division 
			if (couldBeDate)
			{
				expect.Add(Culture.DateTimeFormat.DateSeparator[0]);
				expect.Add(Culture.DateTimeFormat.TimeSeparator[0]);
			}

			if ( expect.Contains(currentChar))
			{
				return false;
			}

			switch (currentChar)
			{
				case -1: 
				case ' ':
				case '\n':
				case '\r':
				case '+':
				case '*':
				case '^':
				case '(':
				case ')':
				case '{':
				case '[':
				case ']':
				case '}':
				case '!':
				case '<':
				case '>':
				case '=':
				case ':':
				case '\'':
				case '"':
				case '-':
				case '/':
				case '.':
				case ',':
					return true;
				default:
					return false;
			}

		}

		private Token NextToken()
		{
			SkipWhiteSpaces();

			switch (currentChar)
			{
				case '+':
					NextChar();
					return currentToken = new Token(Token.Type.Plus, line, column);
				case '-':
					NextChar();
					return currentToken = new Token(Token.Type.Neg, line, column);
				case '*':
					NextChar();
					return currentToken = new Token(Token.Type.Mul, line, column);
				case '/':
					NextChar();
					return currentToken = new Token(Token.Type.Div, line, column);
				case '^':
					NextChar();
					return currentToken = new Token(Token.Type.Pow, line, column);
				case '(':
					NextChar();
					return currentToken = new Token(Token.Type.RndOpen, line, column);
				case ')':
					NextChar();
					return currentToken = new Token(Token.Type.RndClose, line, column);
				case '{':
					NextChar();
					return currentToken = new Token(Token.Type.SetOpen, line, column);
				case '[':
					NextChar();
					return currentToken = new Token(Token.Type.BrOpen, line, column);
				case ']':
					NextChar();
					return currentToken = new Token(Token.Type.BrClose, line, column);
				case '}':
					NextChar();
					return currentToken = new Token(Token.Type.SetClose, line, column);
				case '!':
					if (PeekedNextChar == '=') {
						NextChar();
						NextChar();
						return currentToken = new Token(Token.Type.UnEqual, line, column);
					}
					break;
				case '<':
					switch (NextChar())
					{
						case '=':
							NextChar();
							return currentToken = new Token(Token.Type.LessEqual, line, column);
						case '>':
							NextChar();
							return currentToken = new Token(Token.Type.UnEqual, line, column);
						default:
							return currentToken = new Token(Token.Type.Less, line, column);
					}
				case '>':
					switch (NextChar())
					{
						case '=':
							NextChar();
							return currentToken = new Token(Token.Type.GreaterEqual, line, column);
						default:
							return currentToken = new Token(Token.Type.Greater, line, column);
					}
				case '=':
					NextChar();
					if (currentChar == '=')
						NextChar();
					return currentToken = new Token(Token.Type.Equal, line, column);
				case '.':
					NextChar();

					if (currentChar == '.')
					{
						NextChar();

						return currentToken = new Token(Token.Type.DblPeriod, line, column);
					}
					return currentToken = new Token(Token.Type.Period, line, column);
				case ':':
					NextChar();
					return currentToken = new Token(Token.Type.Colon, line, column);
				case '\'':
				case '"':
					return currentToken = ParseText();
				default:
					break;
			}

			if (IsDigit(currentChar)) 
				return currentToken = ParseNumberOrDate();

			if (IsAlpha_(currentChar))
			{
				int _line = line;
				int _column = column;
				string symbol = ParseSymbol();

				for (Dictionary<string, Token.Type>.Enumerator e = keywords.GetEnumerator(); e.MoveNext(); )
				{
					if (e.Current.Key.ToUpper() == symbol.ToUpper())
					{
						currentToken = new Token(e.Current.Value, _line, _column);
						return currentToken;
					}
				}

				return currentToken = new Token(symbol, line, column);
			}

			// unknown character or EOF reached

			if (currentChar == -1)
				return currentToken = new Token(Token.Type.None, line, column);

			throw new UnexpectedTokenException(String.Format("{0}", (char)currentChar), line, column);
		}

		private static readonly Dictionary<string, Token.Type> keywords = CreateKeywordMappings();

        public ExpressionBuilder(CultureInfo culture)
        {
            Culture = culture;
        }

        private static Dictionary<string, Token.Type> CreateKeywordMappings()
		{
            Dictionary<string, Token.Type> keywords = new()
            {
                ["WENN"] = Token.Type.If,
                ["DANN"] = Token.Type.Then,
                ["SONST"] = Token.Type.Else,

                ["INNERHALB"] = Token.Type.Contains,

                ["UND"] = Token.Type.And,
                ["ODER"] = Token.Type.Or,
                ["XODER"] = Token.Type.XOr,


                ["IF"] = Token.Type.If,
                ["THEN"] = Token.Type.Then,
                ["ELSE"] = Token.Type.Else,

                ["BETWEEN"] = Token.Type.Contains,

                ["AND"] = Token.Type.And,
                ["OR"] = Token.Type.Or,
                ["XOR"] = Token.Type.XOr
            };

            return keywords;
		}

		private static bool IsDigit(int ch)
		{
			return ch >= '0' && ch <= '9';
		}

		private static bool IsAlpha(int ch)
		{
			return (ch >= 'a' && ch <= 'z')
				|| (ch >= 'A' && ch <= 'Z');
		}

		private static bool IsAlpha_(int ch)
		{
			return IsAlpha(ch) || ch == '_';
		}

		
		/*
		 * NUMBER       ::= DIGIT... [('.' | ',') DIGIT...]
		 * 
		 * DATE_TIME    ::= DATE [TIME]
		 * DATE         ::= DIGIT [DIGIT] '.' DIGIT [DIGIT] '.' DIGIT DIGIT [DIGIT DIGIT] TIME
		 * TIME         ::= DIGIT DIGIT ':' DIGIT DIGIT ':' DIGIT DIGIT
		 * 
		 * DIGIT        ::= '0'..'9'
		 */
		private Token ParseNumberOrDate()
        {
            int _line = line;
            int _column = column;

			BuildNumberDateSeries(out string buildedString, out bool isDateOrTimeFormat, out bool isTimePattern);

            ///Add Datepart to the TimePart
            buildedString = (isDateOrTimeFormat && isTimePattern) ?
                $"{DateTime.MinValue.ToString(Culture.DateTimeFormat.ShortDatePattern)} {TimeFormatWithSeconds(buildedString)}"
                : buildedString;

            ///Add TimePart to the TimePart
    //        buildedString = (isDateOrTimeFormat && !isTimePattern) ?
				//$"{buildedString} {DateTime.MinValue.ToString(Culture.DateTimeFormat.LongTimePattern)}"
				//: buildedString;


			if (isDateOrTimeFormat && DateTime.TryParse(buildedString,Culture,DateTimeStyles.None, out DateTime dateValue))
            {
                return new Token(dateValue, _line, _column, isTimePattern);
            }

			NumberStyles numberStyles = NumberStyles.AllowDecimalPoint | NumberStyles.Number;

            if (Double.TryParse(buildedString, numberStyles, Culture, out double doubleValue))
            {
                return new Token(doubleValue, _line, _column);
            }

            throw new Exception("Invalid date/number format in expression.");
        }

        private object TimeFormatWithSeconds(string buildedString)
        {
			return (buildedString.Length == 5) ? $"{buildedString}{Culture.DateTimeFormat.TimeSeparator}00" : buildedString;

		}

        private void BuildNumberDateSeries(out string buildedString, out bool isDateOrTimeFormat, out bool isTimePattern)
        {
            var sb = new StringBuilder(100);
			
			isTimePattern = IsTimePattern();
			isDateOrTimeFormat = IsDatePattern() || isTimePattern;

            while (IsValidForDateOrInt(currentChar, isDateOrTimeFormat))
            {
                sb.Append((char)currentChar);
                NextChar();
            }

			buildedString = sb.ToString();
        }

        private bool CouldBeADateOrTime()
        {
		
			return IsDatePattern() || IsTimePattern();
			
		}

		private bool IsDatePattern()
        {
			var dateSeperator = Culture.DateTimeFormat.DateSeparator[0];
			var datePattern = CleanDatePattern(Culture.DateTimeFormat.ShortDatePattern);
			
			var dateSeperatorFirstPosition = datePattern.IndexOf(dateSeperator);
			var dateSeperatorLastPosition = datePattern.LastIndexOf(dateSeperator);

			var invalidChar = false;
			var isDate = false;

			for (int i = 0; i < 11; i++)
			{
				var charAtPosition = input.PeekSkip(currentPos, i - 1);
				if (i == datePattern.Length) 
					isDate = !invalidChar && !IsValidForDateOrInt(charAtPosition, true);

				if (i == dateSeperatorFirstPosition || i == dateSeperatorLastPosition)
				{
					//Expect Time Seperator at this Position!
					invalidChar = (charAtPosition != dateSeperator) || invalidChar;
				}
				else
				{
					invalidChar = (!IsValidForDateOrInt(charAtPosition, false)) || invalidChar;
				}
				//if (invalidChar) Debugger.Break();

			}


			return isDate;


		}

		private bool IsTimePattern()
		{
			var timeSeperator = Culture.DateTimeFormat.TimeSeparator[0];
			var timePattern = Culture.DateTimeFormat.LongTimePattern;

			var timeSeperatorFirstPosition = timePattern.IndexOf(timeSeperator);
			var timeSeperatorLastPosition = timePattern.LastIndexOf(timeSeperator);

			var invalidChar = false;

			//Format 00:00
			var isTwoBlocksTime = false;
			//Format 00:00:00 
			var isThreeBlocksTime = false;

			for (int i = 0; i < 9; i++)
			{
				var charAtPosition = input.PeekSkip(currentPos, i-1);
                if (i == 5) isTwoBlocksTime = !invalidChar && !IsValidForDateOrInt(charAtPosition, true);
				if (i == 8) isThreeBlocksTime = !invalidChar && !IsValidForDateOrInt(charAtPosition, true);
		
				if (i == timeSeperatorFirstPosition || i == timeSeperatorLastPosition)
				{
					//Expect Time Seperator at this Position!
					invalidChar = (charAtPosition != timeSeperator) || invalidChar;
                }
                else
                {
					invalidChar = (!IsValidForDateOrInt( charAtPosition, false)) || invalidChar;
				}

			}


			return isTwoBlocksTime || isThreeBlocksTime;

		}


		/// <summary>
		/// DatePatterns can contain single d and M ... here we clean it up to get the correct potiion
		/// </summary>
		/// <param name="datePattern"></param>
		/// <returns></returns>
		private static string CleanDatePattern(string datePattern)
        {
			if (datePattern.Count(p => p == 'M') == 1)
				datePattern = datePattern.Replace("M", "MM");

			if (datePattern.Count(p => p == 'd') == 1)
				datePattern = datePattern.Replace("d", "dd");

			if (datePattern.Count(p => p == 'y') == 2)
				datePattern = datePattern.Replace("yy", "yyyy");
			
			return datePattern;
		}

        public bool IsValidForDateOrInt(int currentChar, bool couldBeDate)
        {
			var valid = false;
			valid = IsDigit(currentChar) || valid;
			valid = !IsIntDateStopToken(currentChar, couldBeDate) || valid;
			return valid;
        }

        // SYMBOL ::= ALPHA_ [ALPHA_ | DIGIT]...
        // ALPHA_ ::= 'a'..'z' | 'A'..'Z' | '_'
        private string ParseSymbol()
		{
			var sb = new StringBuilder(16);

			if (IsAlpha_(currentChar))
			{
				sb.Append((char)currentChar);

				for (NextChar(); IsAlpha_(currentChar) || IsDigit(currentChar); NextChar())
					sb.Append((char)currentChar);
			}

			return sb.ToString();
		}

		private Token ParseText() {
			int oldline = line;
			int oldcolumn = column;

			char delimiter = (char)currentChar;
			var sb = new StringBuilder();
			
			while (NextChar() != delimiter)
				sb.Append((char)currentChar);
			
			NextChar();

			return new Token(Token.Type.Text, sb.ToString(), oldline, oldcolumn);
		}

		// S ::= \x20 | \t | \r | \n
		private void SkipWhiteSpaces()
		{
			// TODO: skip comments as well
			while (IsWhiteSpace(currentChar))
				NextChar();
		}

		private static bool IsWhiteSpace(int ch)
		{
			switch (ch)
			{
				case ' ':
				case '\t':
				case '\n':
				case '\r':
					return true;
				default:
					return false;
			}
		}
		#endregion
	}
}
