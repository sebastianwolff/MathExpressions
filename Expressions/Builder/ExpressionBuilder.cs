using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

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
	public class ExpressionBuilder : IExpressionBuilder
	{
		/// <summary>
		/// Use this method for convience
		/// </summary>
		/// <param name="pattern">the expression in form of a mathematical pattern</param>
		/// <returns>the compiled expression tree</returns>
		public static Node ParseExpression(string pattern)
		{
			ExpressionBuilder builder = new ExpressionBuilder();

			return builder.Parse(pattern);
		}

		#region IExpressionBuilder Members
		public Node Parse(string expressionPattern)
		{
			initializeLexer(expressionPattern);

			Node result = Expression(false);

			if (currentToken.type != Token.Type.None)
				throw new UnexpectedTokenException(currentToken.stringValue, currentToken.line, currentToken.column);

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

			if (currentToken.type == Token.Type.Contains)
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
				switch (currentToken.type)
				{
					case Token.Type.And:
					case Token.Type.Or:
					case Token.Type.XOr:
						result = new CompareNode(TokenType2Compare(currentToken.type), result, RelExpr(true));
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

			switch (currentToken.type)
			{
				case Token.Type.Less:
				case Token.Type.LessEqual:
				case Token.Type.Equal:
				case Token.Type.UnEqual:
				case Token.Type.GreaterEqual:
				case Token.Type.Greater:
					result = new CompareNode(TokenType2Compare(currentToken.type), result, RangeExpr(true));
					break;
				default:
					break;
			}
			return result;
		}

		private Node RangeExpr(bool get)
		{
			Node result = AddExpr(get);

			if (currentToken.type == Token.Type.DblPeriod)
				result = new RangeNode(result, AddExpr(true));

			return result;
		}

		private CompareNode.Compare TokenType2Compare(Token.Type type)
		{
			switch (type)
			{
				case Token.Type.Less: return CompareNode.Compare.Less;
				case Token.Type.LessEqual: return CompareNode.Compare.LessEqual;
				case Token.Type.Equal: return CompareNode.Compare.Equal;
				case Token.Type.UnEqual: return CompareNode.Compare.UnEqual;
				case Token.Type.GreaterEqual: return CompareNode.Compare.GreaterEqual;
				case Token.Type.Greater: return CompareNode.Compare.Greater;
				case Token.Type.And: return CompareNode.Compare.And;
				case Token.Type.Or: return CompareNode.Compare.Or;
				case Token.Type.XOr: return CompareNode.Compare.XOr;
				default:
					throw new Exception("Internal error: Invalid token-to-compare conversion.");
			}
		}

		private Node AddExpr(bool get)
		{
			Node result = MulExpr(get);

			while (true)
			{
				switch (currentToken.type)
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
				switch (currentToken.type)
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

			while (currentToken.type == Token.Type.Pow)
				result = new PowNode(result, QualExpr(true));

			return result;
		}

		private Node QualExpr(bool get)
		{
			Node result = ValueExpr(get);

			while (currentToken.type == Token.Type.Period)
			{
				nextToken();

				if (currentToken.type == Token.Type.Symbol)
				{
					if (currentToken.stringValue == "tag")
						result = new DateQualExpr(DateQualExpr.Qual.Day, result);
					else if (currentToken.stringValue == "monat")
						result = new DateQualExpr(DateQualExpr.Qual.Month, result);
					else if (currentToken.stringValue == "jahr")
						result = new DateQualExpr(DateQualExpr.Qual.Year, result);
					else
						continue;

					nextToken(); // consume qualification symbol
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
				nextToken();

			switch (currentToken.type)
			{
				case Token.Type.Number:
				{
					Node result = new NumberNode(currentToken.numberValue);
					nextToken();
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
					return conditional(true);
				case Token.Type.Plus:
					return ValueExpr(true);
				case Token.Type.Neg:
					return new NegNode(ValueExpr(true));
				case Token.Type.Symbol:
				{
					string symbolName = currentToken.stringValue;
					nextToken();

					if (currentToken.type == Token.Type.RndOpen)
					{
						Node arg = Expression(true);
						Consume(Token.Type.RndClose);
						var symbol = symbolName.ToLower().StartsWith("round") || symbolName.ToLower().StartsWith("runden") ? "round" : symbolName.ToLower();

						switch (symbol) {
							case "tage":
							case "days":
								return new TimeSpanCastExpr(TimeSpanCastExpr.Unit.Day, arg);
							case "monate":
							case "months":
									return new TimeSpanCastExpr(TimeSpanCastExpr.Unit.Month, arg);
							case "jahre":
							case "years":
									return new TimeSpanCastExpr(TimeSpanCastExpr.Unit.Year, arg);
							case "jahr":
							case "year":
								return new DateQualExpr(DateQualExpr.Qual.Year, arg);
							case "monat":
							case "month":
									return new DateQualExpr(DateQualExpr.Qual.Month, arg);
							case "tag":
							case "day":
									return new DateQualExpr(DateQualExpr.Qual.Day, arg);
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
					Node result = new DateExpr(currentToken.dateValue);
					nextToken();
					return result;
				}
				case Token.Type.Text: {
					Node result = new TextNode(currentToken.stringValue);
					nextToken();
					return result;
				}
				default:
					throw new UnexpectedTokenException(currentToken.stringValue, currentToken.line, currentToken.column);
			}
		}

		private Node conditional(bool get)
		{
			if (get)
				Consume(Token.Type.If);

			Node condExpr = Expression(false);

			Consume(Token.Type.Then);

			Node thenExpr = Expression(false);

			Node elseExpr = (currentToken.type == Token.Type.Else) 
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
			if (currentToken.type != tokenType)
				throw new UnexpectedTokenException(currentToken.stringValue, currentToken.line, currentToken.column);

			if (get)
				nextToken();
		}
		#endregion

		#region lexical analizing
		private TextReader input;
		private int line;
		private int column;
		private int currentChar;
		private Token currentToken;

		private int peekedNextChar
		{
			get
			{
				return input.Peek();
			}
		}

		private void initializeLexer(string pattern)
		{
			input = new StringReader(pattern);

			line = 1;
			column = 0;
			currentChar = 0;

			nextChar();
			nextToken();
		}

		private int nextChar()
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
			return currentChar = input.Read();
		}

		private Token nextToken()
		{
			skipWhiteSpaces();

			switch (currentChar)
			{
				case '+':
					nextChar();
					return currentToken = new Token(Token.Type.Plus, line, column);
				case '-':
					nextChar();
					return currentToken = new Token(Token.Type.Neg, line, column);
				case '*':
					nextChar();
					return currentToken = new Token(Token.Type.Mul, line, column);
				case '/':
					nextChar();
					return currentToken = new Token(Token.Type.Div, line, column);
				case '^':
					nextChar();
					return currentToken = new Token(Token.Type.Pow, line, column);
				case '(':
					nextChar();
					return currentToken = new Token(Token.Type.RndOpen, line, column);
				case ')':
					nextChar();
					return currentToken = new Token(Token.Type.RndClose, line, column);
				case '{':
					nextChar();
					return currentToken = new Token(Token.Type.SetOpen, line, column);
				case '[':
					nextChar();
					return currentToken = new Token(Token.Type.BrOpen, line, column);
				case ']':
					nextChar();
					return currentToken = new Token(Token.Type.BrClose, line, column);
				case '}':
					nextChar();
					return currentToken = new Token(Token.Type.SetClose, line, column);
				case '!':
					if (peekedNextChar == '=') {
						nextChar();
						nextChar();
						return currentToken = new Token(Token.Type.UnEqual, line, column);
					}
					break;
				case '<':
					switch (nextChar())
					{
						case '=':
							nextChar();
							return currentToken = new Token(Token.Type.LessEqual, line, column);
						case '>':
							nextChar();
							return currentToken = new Token(Token.Type.UnEqual, line, column);
						default:
							return currentToken = new Token(Token.Type.Less, line, column);
					}
				case '>':
					switch (nextChar())
					{
						case '=':
							nextChar();
							return currentToken = new Token(Token.Type.GreaterEqual, line, column);
						default:
							return currentToken = new Token(Token.Type.Greater, line, column);
					}
				case '=':
					nextChar();
					if (currentChar == '=')
						nextChar();
					return currentToken = new Token(Token.Type.Equal, line, column);
				case '.':
					nextChar();

					if (currentChar == '.')
					{
						nextChar();

						return currentToken = new Token(Token.Type.DblPeriod, line, column);
					}
					return currentToken = new Token(Token.Type.Period, line, column);
				case ':':
					nextChar();
					return currentToken = new Token(Token.Type.Colon, line, column);
				case '\'':
				case '"':
					return currentToken = ParseText();
				default:
					break;
			}

			if (isDigit(currentChar)) 
				return currentToken = ParseNumberOrDate();

			if (isAlpha_(currentChar))
			{
				int _line = line;
				int _column = column;
				string symbol = parseSymbol();

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

		private static Dictionary<string, Token.Type> keywords = createKeywordMappings();

		private static Dictionary<string, Token.Type> createKeywordMappings()
		{
			Dictionary<string, Token.Type> keywords = new Dictionary<string, Token.Type>();

			keywords["WENN"] = Token.Type.If;
			keywords["DANN"] = Token.Type.Then;
			keywords["SONST"] = Token.Type.Else;

			keywords["INNERHALB"] = Token.Type.Contains;

			keywords["UND"] = Token.Type.And;
			keywords["ODER"] = Token.Type.Or;
			keywords["XODER"] = Token.Type.XOr;


			keywords["IF"] = Token.Type.If;
			keywords["THEN"] = Token.Type.Then;
			keywords["ELSE"] = Token.Type.Else;

			keywords["BETWEEN"] = Token.Type.Contains;

			keywords["AND"] = Token.Type.And;
			keywords["OR"] = Token.Type.Or;
			keywords["XOR"] = Token.Type.XOr;

			return keywords;
		}

		private bool isDigit(int ch)
		{
			return ch >= '0' && ch <= '9';
		}

		private bool isAlpha(int ch)
		{
			return (ch >= 'a' && ch <= 'z')
				|| (ch >= 'A' && ch <= 'Z');
		}

		private bool isAlpha_(int ch)
		{
			return isAlpha(ch) || ch == '_';
		}

		private Token ParseSimpleNumber()
		{
			int _line = line;
			int _column = column;

			StringBuilder sb = new StringBuilder(10);

			for (; isDigit(currentChar); nextChar())
				sb.Append((char)currentChar);

			if (sb.Length == 0)
				throw new UnexpectedTokenException(sb.ToString(), _line, _column); // XXX parsed string is empty... might be confusing!

			return new Token(Convert.ToInt32(sb.ToString()), _line, _column);
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

			StringBuilder sb = new StringBuilder(10);

			for (; isDigit(currentChar); nextChar())
				sb.Append((char)currentChar);

			// it MAY be a date
			if (currentChar == '.' && sb.Length >= 1 && sb.Length <= 2 && peekedNextChar != '.')
			{
				int dayLength = sb.Length; // DD.

				sb.Append('.');

				for (nextChar(); isDigit(currentChar); nextChar()) // possibly MM (or the .floating point value)
					sb.Append((char)currentChar);

				if (currentChar == '.' && sb.Length >= dayLength + 2 && sb.Length <= dayLength + 3)
				{
					int dayMonthLength = sb.Length; // DD.MM. has been parsed already

					sb.Append('.');

					for (nextChar(); isDigit(currentChar); nextChar())
						sb.Append((char)currentChar);

					if (sb.Length != dayMonthLength + 5)
						throw new Exception("Invalid date/number format in expression.");

					string pattern = sb.ToString();
					int day = Int32.Parse(pattern.Substring(0, dayLength));
					int month = Int32.Parse(pattern.Substring(dayLength + 1, dayMonthLength - (dayLength + 1)));
					int year = Int32.Parse(pattern.Substring(sb.Length - dayMonthLength + 1));

					// check wether we got a fully qualified date-time or just a date-without-time ...
					skipWhiteSpaces();
					if (!isDigit(currentChar) || !isDigit(peekedNextChar))
						return new Token(new DateTime(year, month, day), _line, _column);

					int hour = (int)ParseSimpleNumber().numberValue;

					nextToken();
					Consume(Token.Type.Colon, false);

					int minute = (int)ParseSimpleNumber().numberValue;

					nextToken();
					Consume(Token.Type.Colon, false);

					int second = (int)ParseSimpleNumber().numberValue;

					// XXX: Aktuell werden die ZEIT-Werte eh vom DateExpr node weggeschnitten,
					// jedoch ergibt sich offenbar die Not Zeit formate mit parsen zu muessen *und*
					// zu ignorieren. Soll der Kunde entscheiden, ob er sie braucht oder nicht;
					// Ggf. werden die intermediate form anpassen.
					return new Token(new DateTime(year, month, day, hour, minute, second), _line, _column);
				}
			}
			else if ((currentChar == '.' && peekedNextChar != '.') || currentChar == ',')
			{
				sb.Append((char)currentChar);
				nextChar();

				for (; isDigit(currentChar); nextChar())
					sb.Append((char)currentChar);
			}

			return new Token(Double.Parse(sb.ToString()), _line, _column);
		}

		// SYMBOL ::= ALPHA_ [ALPHA_ | DIGIT]...
		// ALPHA_ ::= 'a'..'z' | 'A'..'Z' | '_'
		private string parseSymbol()
		{
			StringBuilder sb = new StringBuilder(16);

			if (isAlpha_(currentChar))
			{
				sb.Append((char)currentChar);

				for (nextChar(); isAlpha_(currentChar) || isDigit(currentChar); nextChar())
					sb.Append((char)currentChar);
			}

			return sb.ToString();
		}

		private Token ParseText() {
			int oldline = line;
			int oldcolumn = column;

			char delimiter = (char)currentChar;
			StringBuilder sb = new StringBuilder();
			
			while (nextChar() != delimiter)
				sb.Append((char)currentChar);
			
			nextChar();

			return new Token(Token.Type.Text, sb.ToString(), oldline, oldcolumn);
		}

		// S ::= \x20 | \t | \r | \n
		private void skipWhiteSpaces()
		{
			// TODO: skip comments as well
			while (isWhiteSpace(currentChar))
				nextChar();
		}

		private bool isWhiteSpace(int ch)
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
