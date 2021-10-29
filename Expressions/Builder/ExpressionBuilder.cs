using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using MathExpressinTests;
using System.Linq;
using System.Diagnostics;
using Expressionator.Translations;

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
                    switch (ReservedKeyWord.GetKeyWordType(currentToken.StringValue))
                    {
						case KeyWords.Day:
						result = new DateQualExpr(DateQualExpr.Quals.Day, result);
							break;
						case KeyWords.Month:
							result = new DateQualExpr(DateQualExpr.Quals.Month, result);
							break;
						case KeyWords.Year:
							result = new DateQualExpr(DateQualExpr.Quals.Year, result);
							break;
						default:
							continue;
                    }
                    
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

                        switch (ReservedKeyWord.GetKeyWordType(symbol))
                        {
                            case KeyWords.Days:
                                return new TimeSpanCastExpr(TimeSpanCastExpr.Units.Day, arg);
                            case KeyWords.Months:
                                return new TimeSpanCastExpr(TimeSpanCastExpr.Units.Month, arg);
                            case KeyWords.Years:
                                return new TimeSpanCastExpr(TimeSpanCastExpr.Units.Year, arg);
                            case KeyWords.Hours:
                                return new TimeSpanCastExpr(TimeSpanCastExpr.Units.Hour, arg);
                            case KeyWords.Minutes:
                                return new TimeSpanCastExpr(TimeSpanCastExpr.Units.Minute, arg);
                            case KeyWords.Seconds:
                                return new TimeSpanCastExpr(TimeSpanCastExpr.Units.Second, arg);

                            case KeyWords.Year:
                                return new DateQualExpr(DateQualExpr.Quals.Year, arg);
                            case KeyWords.Month:
                                return new DateQualExpr(DateQualExpr.Quals.Month, arg);
                            case KeyWords.Day:
                                return new DateQualExpr(DateQualExpr.Quals.Day, arg);
                            case KeyWords.Hour:
                                return new DateQualExpr(DateQualExpr.Quals.Hour, arg);
                            case KeyWords.Minute:
                                return new DateQualExpr(DateQualExpr.Quals.Minute, arg);
                            case KeyWords.Second:
                                return new DateQualExpr(DateQualExpr.Quals.Second, arg);

                            case KeyWords.Round:
                                return GetPrecision(symbolName, arg);
                            case KeyWords.Unknown:
								throw new Exception(String.Format("Unknown Function: '{0}'", symbolName));
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

        /// <summary>
        /// Gets RoundCastExpression with extracted Precision
        /// </summary>
        /// <param name="symbolName"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        private static Node GetPrecision(string symbolName, Node arg)
        {
            var precisionString = symbolName.ToLower().Replace("round", "");

            precisionString = (string.IsNullOrEmpty(precisionString))
                ? "0"
                : precisionString;

            if (!int.TryParse(precisionString, out int precision))
                throw new UnknownPrecisionException($"{precisionString}");

            return new RoundCastExpr(precision, arg);
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
		private char currentChar;
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
			currentChar = (char)0;

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

		private void MoveBack(int positions)
		{
            while (positions+1 != 0)
            {
				if (currentChar == '\n')
				{
					throw new Exception("Moveback over Multilines isn't supported");
				}
				else
				{
					--column;
				}
				currentPos--;
				positions--;
			}
			NextChar();
		}

		/// <summary>
		/// List of Token, that indicates, that the currentCahr ist not part of a Number or Date
		/// </summary>
		private static readonly char[] unwantedIntDateStopToken = new char[] {
				 (char)0,
				 ' ',
				 '\n',
				 '\r',
				 '+',
				 '*',
				 '^',
				 '(',
				 ')',
				 '{',
				 '[',
				 ']',
				 '}',
				 '!',
				 '<',
				 '>',
				 '=',
				 ',',
				 '\'',
				 '"',
				 '-',
				 '/',
				 '.',
				 ',',

			};


		/// <summary>
		/// Indicates if the Char is not Part of a Number or Date
		/// </summary>
		/// <param name="currentChar">The Char</param>
		/// <param name="couldBeDate">Could the current char series be a DateTime</param>
		/// <returns></returns>
		public bool IsIntDateStopToken(char currentChar, bool couldBeDate)
        {
			//Prevent Misinterpretation of "/" as DatePart instead of Division 
			if (couldBeDate)
			{
				if (excludeSeperatorsWithDate.Contains(currentChar))
					return false;
            }
            else
            {
				if (excludeSeperators.Contains(currentChar))
					return false;
			}

			return unwantedIntDateStopToken.Contains(currentChar);
           
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

                if (tokenTypeKeywords.TryGetValue(symbol.ToUpper(), out Token.Type type ))
                {
					currentToken = new Token(type, _line, _column);
					return currentToken;
				}

				

				return currentToken = new Token(symbol, line, column);
			}

			// unknown character or EOF reached

			if (currentChar == -1 || currentChar == (char)0)
				return currentToken = new Token(Token.Type.None, line, column);

			throw new UnexpectedTokenException(String.Format("{0}", (char)currentChar), line, column);
		}

		private static readonly Dictionary<string, Token.Type> tokenTypeKeywords = ReservedKeyWord.CreateTokenTypeMappings();
		private readonly string dateTimeFormat;
        private readonly string cleanedDatePattern;
        private readonly char[] excludeSeperators;
		private readonly char[] excludeSeperatorsWithDate;
        private readonly char dateSeperator;
        private readonly int dateSeperatorFirstPosition;
        private readonly int dateSeperatorLastPosition;
        private readonly char timeSeperator;
        private readonly string timePattern;
        private readonly int timeSeperatorFirstPosition;
        private readonly int timeSeperatorLastPosition;

		public ExpressionBuilder(CultureInfo culture)
        {
            Culture = culture;

            #region InitDateTime Patterns
            cleanedDatePattern = CleanDatePattern(Culture.DateTimeFormat.ShortDatePattern);
			dateTimeFormat = $"{cleanedDatePattern} {CleanTimePattern(Culture.DateTimeFormat.LongTimePattern)}";
			excludeSeperators = new char[] { Culture.NumberFormat.NumberDecimalSeparator[0], Culture.NumberFormat.NumberGroupSeparator[0] };
			
			excludeSeperatorsWithDate =  new char[] { Culture.DateTimeFormat.TimeSeparator[0], Culture.DateTimeFormat.DateSeparator[0], Culture.NumberFormat.NumberDecimalSeparator[0], Culture.NumberFormat.NumberGroupSeparator[0] };

			dateSeperator = Culture.DateTimeFormat.DateSeparator[0];

			dateSeperatorFirstPosition = cleanedDatePattern.IndexOf(dateSeperator);
			dateSeperatorLastPosition = cleanedDatePattern.LastIndexOf(dateSeperator);

			timeSeperator = Culture.DateTimeFormat.TimeSeparator[0];
			timePattern = Culture.DateTimeFormat.LongTimePattern;

			timeSeperatorFirstPosition = timePattern.IndexOf(timeSeperator);
			timeSeperatorLastPosition = timePattern.LastIndexOf(timeSeperator);
            #endregion
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

            CleanBuildedStringByCultureInfo(ref buildedString, isDateOrTimeFormat, isTimePattern);

			if (isDateOrTimeFormat && buildedString.TryGetDate(dateTimeFormat, out DateTime dateValue))
            {
                return new Token(dateValue, _line, _column, isTimePattern);
            }

            var numberStyles = NumberStyles.AllowDecimalPoint | NumberStyles.Number;

            if (Double.TryParse(buildedString, numberStyles, Culture, out double doubleValue))
            {
                return new Token(doubleValue, _line, _column);
            }

            throw new Exception("Invalid date/number format in expression.");
        }

		/// <summary>
		/// Add Time Part to Datestring or Datepart to Timestring
		/// </summary>
		/// <param name="buildedString"></param>
		/// <param name="isDateOrTimeFormat"></param>
		/// <param name="isTimePattern"></param>
		/// <returns></returns>
        private void CleanBuildedStringByCultureInfo(ref string buildedString, bool isDateOrTimeFormat, bool isTimePattern)
        {
            ///Add Datepart to the TimePart
            buildedString = (isDateOrTimeFormat && isTimePattern) ?
                $"{DateTime.MinValue.ToString(cleanedDatePattern)} {TimeFormatWithSeconds(buildedString)}"
                : buildedString;


            ///Add TimePart to the Datepart
            buildedString = (isDateOrTimeFormat && !isTimePattern) ?
            $"{AddLeadingZeros(buildedString, Culture.DateTimeFormat.DateSeparator[0])} {DateTime.MinValue.ToString(CleanTimePattern(Culture.DateTimeFormat.LongTimePattern))}"
            : buildedString;

            
        }

        private static string CleanTimePattern(string longTimePattern)
        {
			if (longTimePattern.Count(p => p == 'h') == 1)
				longTimePattern = longTimePattern.Replace("h", "hh");

			if (longTimePattern.Count(p => p == 'H') == 1)
				longTimePattern = longTimePattern.Replace("H", "HH");


			if (longTimePattern.Count(p => p == 'm') == 1)
				longTimePattern = longTimePattern.Replace("m", "mm");

			if (longTimePattern.Count(p => p == 's') == 1)
				longTimePattern = longTimePattern.Replace("s", "ss");

			return longTimePattern.Replace("tt", "").Trim();
        }

        private string TimeFormatWithSeconds(string buildedString)
        {
            buildedString = AddLeadingZeros(buildedString, Culture.DateTimeFormat.TimeSeparator[0]); 
            return (buildedString.Length == 5) ? $"{buildedString}{Culture.DateTimeFormat.TimeSeparator}00" : buildedString;
        }

        private static string AddLeadingZeros(string buildedString, char seperator)
        {
            var splitted = buildedString.Split(seperator);
            var newBuildedString = new StringBuilder();
            for (int i = 0; i < splitted.Length; i++)
            {
				//Add trailing zero
                newBuildedString.Append((splitted[i].Length == 1) ? "0" + splitted[i] : splitted[i]);
                if (i<splitted.Length-1)
                {
					newBuildedString.Append(seperator);
				}
				
            }

			return newBuildedString.ToString(); ;
        }

        private void BuildNumberDateSeries(out string buildedString, out bool isDateOrTimeFormat, out bool isTimePattern)
        {
            buildedString = string.Empty;
            isDateOrTimeFormat = false;
            isTimePattern = false;

            StringBuilder stringToCheck = GetNumberDateBlock(true);

            if (IsTimePattern(stringToCheck.ToString(), out string timeString))
            {
                buildedString = timeString;
                isDateOrTimeFormat = true;
                isTimePattern = true;
                return;
            }

            if (IsDatePattern(stringToCheck.ToString(), out string dateString))
            {
                buildedString = dateString;
                isDateOrTimeFormat = true;
                return;
            }

			///Special Case: if value is not Date or Time but have DateSeperator, it must be Recreated wtihout DateSeperatortoken
            if (stringToCheck.ToString().Any(c => c=='/' || c == ':'))
            {
                MoveBack(stringToCheck.Length);
				stringToCheck = GetNumberDateBlock(false);
            }

            buildedString = stringToCheck.ToString();


        }

        private StringBuilder GetNumberDateBlock(bool couldBeDate)
        {
            var stringToCheck = new StringBuilder();

            while (IsValidForDateOrInt(currentChar, couldBeDate))
            {
                stringToCheck.Append(currentChar);
                NextChar();
            }

            return stringToCheck;
        }

        private bool IsDatePattern(string stringToCheck, out string dateString)
        {
			dateString = string.Empty;

			
			var finalToCheck = AddLeadingZeros(stringToCheck, dateSeperator);

			//If Pattern Size not matches String Size -> return Immediately
			//Cant match
			if (finalToCheck.Length < cleanedDatePattern.Length)
				return false;

		
			var invalidChar = false;
			var isDate = false;


			for (int i = 0; i < cleanedDatePattern.Length; i++)
			{
				char charAtPosition = finalToCheck[i];
				
				if (i == cleanedDatePattern.Length-1)
				{
					isDate = !invalidChar;// && !IsValidForDateOrInt(charAtPosition, true);
					break;
				}
				
				if (i == dateSeperatorFirstPosition || i == dateSeperatorLastPosition)
				{
					//Expect Time Seperator at this Position!
					invalidChar = (charAtPosition != dateSeperator) || invalidChar;
				}
		
			}

			dateString = isDate ? finalToCheck : string.Empty;

			return isDate;

		}

		private bool IsTimePattern(string stringToCheck, out string dateString)
		{
			dateString = string.Empty;
			if (stringToCheck.Length != 5 && stringToCheck.Length != 8)
				return false;

			
			var invalidChar = false;

			//Format 00:00
			var isTwoBlocksTime = false;
			//Format 00:00:00 
			var isThreeBlocksTime = false;
			
			for (int i = 0; i < stringToCheck.Length; i++)
			{
				var charAtPosition = stringToCheck[i];

				if (i == 4) isTwoBlocksTime = !invalidChar;//&& !IsValidForDateOrInt(charAtPosition, true);
				if (i == 7) isThreeBlocksTime = !invalidChar;// && !IsValidForDateOrInt(charAtPosition, true);
		
				if (i == timeSeperatorFirstPosition || i == timeSeperatorLastPosition)
				{
					//Expect Time Seperator at this Position!
					invalidChar = (charAtPosition != timeSeperator) || invalidChar;
                }
			}

			dateString = isTwoBlocksTime || isThreeBlocksTime ? stringToCheck : string.Empty;

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

        public bool IsValidForDateOrInt(char currentChar, bool couldBeDate)
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
