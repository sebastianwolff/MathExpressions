using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions.Builder
{
	public class Token
	{
		public enum Type
		{
			Invalid,
			None,
			Number,
			Symbol,
			Text,

			If,
			Then,
			Else,

			Less,
			LessEqual,
			Equal,
			UnEqual,
			GreaterEqual,
			Greater,

			Contains,
			Date,

			DblPeriod,
			Period,
			Colon,

			And,
			Or,
			XOr,

			RndOpen,
			RndClose,
			BrOpen,
			BrClose,
			SetOpen,
			SetClose,

			Plus,
			Neg,
			Mul,
			Div,
			Pow
		}

		private Type _type;
		private TypeCode _typeCode;
		private string _stringValue;
		private double _numberValue;
		private DateTime _dateValue;

		private int _line;
		private int _column;

		#region public attribut accessors
		public Type type
		{
			get
			{
				return _type;
			}
		}

		public TypeCode typeCode
		{
			get
			{
				return _typeCode;
			}
		}

		public string stringValue
		{
			get
			{
				return _stringValue;
			}
		}

		public double numberValue
		{
			get
			{
				return _numberValue;
			}
		}

		public DateTime dateValue
		{
			get
			{
				return _dateValue.Date;
			}
		}

		public int line
		{
			get
			{
				return _line;
			}
		}

		public int column
		{
			get
			{
				return _column;
			}
		}
		#endregion

		/// <summary>
		/// initializes Token object to represent a non-existing token. the default token (None).
		/// </summary>
		public Token()
		{
			_type = Type.None;
			_typeCode = TypeCode.Empty;
			_stringValue = "";
			_numberValue = 0.0;

			_line = 1;
			_column = 1;
		}

		public Token(Type type, int line, int column)
		{
			_type = type;
			_stringValue = type2Str(type);

			_line = line;
			_column = column;
		}

		/// <summary>
		/// initializes Token object to represent either a keyword OR a number-unassociated symbol
		/// </summary>
		/// <param name="symbolValue"></param>
		public Token(string symbolValue, int line, int column)
		{
			_type = Type.Symbol;
			_typeCode = TypeCode.String;
			_stringValue = symbolValue;
			_numberValue = 0.0;

			_line = line;
			_column = column;
		}

		/// <summary>
		/// initializes Token object to represent either a keyword OR a number-unassociated symbol
		/// </summary>
		/// <param name="symbolValue"></param>
		public Token(Token.Type type, string symbolValue, int line, int column)
		{
			_type = type;
			_typeCode = TypeCode.String;
			_stringValue = symbolValue;
			_numberValue = 0.0;

			_line = line;
			_column = column;
		}

		/// <summary>
		/// initializes Token object to represent a symbol<->number association
		/// </summary>
		/// <param name="symbolValue"></param>
		/// <param name="numberValue"></param>
		public Token(string symbolValue, double numberValue, int line, int column)
		{
			_type = Type.Symbol;
			_typeCode = TypeCode.Double;
			_stringValue = symbolValue;
			_numberValue = numberValue;

			_line = line;
			_column = column;
		}

		public Token(DateTime date, int line, int column)
		{
			_type = Type.Date;
			_typeCode = TypeCode.DateTime;
			_stringValue = date.ToString();
			_numberValue = date.Ticks;
			_dateValue = date;

			_line = line;
			_column = column;
		}

		/// <summary>
		/// initializes Token object to represent a number
		/// </summary>
		/// <param name="numberValue"></param>
		public Token(double numberValue, int line, int column)
		{
			_type = Type.Number;
			_typeCode = TypeCode.Double;
			_stringValue = ((Double)numberValue).ToString();
			_numberValue = numberValue;

			_line = line;
			_column = column;
		}

		private string type2Str(Type type)
		{
			switch (type)
			{
				case Type.Invalid:
					return "INVALID";
				case Type.None:
					return "NONE";
				case Type.Number:
					return "NUMBER";
				case Type.Symbol:
					return "SYMBOL";
				case Type.Text:
					return "TEXT";
				case Type.If:
					return "IF";
				case Type.Then:
					return "THEN";
				case Type.Else:
					return "ELSE";
				case Type.Less:
					return "<";
				case Type.LessEqual:
					return "<=";
				case Type.Equal:
					return "=";
				case Type.UnEqual:
					return "<>";
				case Type.GreaterEqual:
					return ">=";
				case Type.Greater:
					return ">";
				case Type.Contains:
					return "CONTAINS";
				case Type.And:
					return "AND";
				case Type.Or:
					return "OR";
				case Type.XOr:
					return "XOR";
				case Type.DblPeriod:
					return "..";
				case Type.Period:
					return ".";
				case Type.Colon:
					return ":";
				case Type.RndOpen:
					return "(";
				case Type.RndClose:
					return ")";
				case Type.BrOpen:
					return "[";
				case Type.BrClose:
					return "]";
				case Type.SetOpen:
					return "{";
				case Type.SetClose:
					return "}";
				case Type.Plus:
					return "+";
				case Type.Neg:
					return "-";
				case Type.Mul:
					return "*";
				case Type.Div:
					return "/";
				case Type.Pow:
					return "^";
				default:
					throw new Exception("Internal error occured: Unhandled token type.");
			}
		}

		public override string ToString()
		{
			switch (type)
			{
				case Type.Symbol:
				case Type.Number:
					return stringValue;
				default:
					return type2Str(type);
			}
		}
	}
}
