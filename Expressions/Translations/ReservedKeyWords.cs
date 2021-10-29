using Expressionator.Expressions.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressionator.Translations
{
    public static class ReservedKeyWord
    {
		public static Dictionary<string, Token.Type> CreateTokenTypeMappings()
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

		public static KeyWords GetKeyWordType(string keyword)
        {

			Dictionary<string, KeyWords> words = new()
            {

				{ "tage", KeyWords.Days },
				{ "days", KeyWords.Days},
				
				{"monate", KeyWords.Months },
				{"months", KeyWords.Months },

				{"jahre", KeyWords.Years },
				{"years", KeyWords.Years },

				{"stunden", KeyWords.Hours },
				{"hours", KeyWords.Hours },

				{"minuten", KeyWords.Minutes },
				{"minutes", KeyWords.Minutes },

				{"sekunden", KeyWords.Seconds },
				{"seconds", KeyWords.Seconds },


				{ "tag", KeyWords.Day },
				{ "day", KeyWords.Day},

				{"monat", KeyWords.Month },
				{"month", KeyWords.Month },

				{"jahr", KeyWords.Year },
				{"year", KeyWords.Year },

				{"stunde", KeyWords.Hour },
				{"hour", KeyWords.Hour },
				
				{"minute", KeyWords.Minute },

				{"sekunde", KeyWords.Second },
				{"second", KeyWords.Second },
				
				{"round", KeyWords.Round },
				{"runden", KeyWords.Round }


			};


            if (words.TryGetValue(keyword.ToLower(), out KeyWords value))
            {
				return value;
            }

			return KeyWords.Unknown;
        }

    }

	public enum KeyWords
	{
		Days,
		Day,
		Months,
		Month,
		Years,
		Year,
		Hours,
		Hour,
		Minutes, 
		Minute,
		Seconds,
		Second,
		Unknown,
        Round

        //     	case "tage":
        //case "days":
        //	return new TimeSpanCastExpr(TimeSpanCastExpr.Units.Day, arg);
        //case "monate":
        //case "months":
        //		return new TimeSpanCastExpr(TimeSpanCastExpr.Units.Month, arg);
        //case "jahre":
        //case "years":
        //		return new TimeSpanCastExpr(TimeSpanCastExpr.Units.Year, arg);
        //case "hours":
        //case "stunden":
        //	return new TimeSpanCastExpr(TimeSpanCastExpr.Units.Hour, arg);
        //case "minutes":
        //case "minuten":
        //	return new TimeSpanCastExpr(TimeSpanCastExpr.Units.Minute, arg);
        //case "sekunden":
        //case "seconds":

    }
}
