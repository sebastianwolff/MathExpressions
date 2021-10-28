using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressionator
{
    public static class StringExtensions
    {

        /// <summary>
        /// Convert string to DateTime by DateFormatPattern 
        /// Much faster then DateTime.[Try]Parse()
        /// </summary>
        /// <param name="SourceString">The Date String</param>
        /// <param name="dateFormat">Format Pattern i.e. dd.MM.yyyy</param>
        /// <param name="date">The DateTime Result as out</param>
        /// <returns></returns>
        /// <remarks>Thanks to James Barrett on StackOverflow</remarks>
        /// <remarks>https://stackoverflow.com/questions/15702123/faster-alternative-to-datetime-parseexact</remarks>
        public static bool TryGetDate(this String SourceString, string dateFormat, out DateTime date) // Offset eliminates need for substring
        {
            var offset = 0;
            date = DateTime.MinValue;
            int Year = 0;
            int Month = 0;
            int Day = 0;
            int Hour = 0;
            int Minute = 0;
            int Second = 0;
            int HourOffset = 0;
            int MS = 0;
            if (SourceString.Length + offset < dateFormat.Length)
                return false;
            for (int i = 0; i < dateFormat.Length; i++)
            {
                System.Char c = SourceString[offset + i];
                switch (dateFormat[i])
                {
                    case 'y':
                        Year = Year * 10 + (c - '0');
                        break;
                    case 'M':
                        Month = Month * 10 + (c - '0');
                        break;
                    case 'd':
                        Day = Day * 10 + (c - '0');
                        break;
                    case 'T':
                        if (c == 'p' || c == 'P')
                            HourOffset = 12;
                        break;
                    case 'h':
                        Hour = Hour * 10 + (c - '0');
                        if (Hour == 12) Hour = 0;
                        break;
                    case 'H':
                        Hour = Hour * 10 + (c - '0');
                        HourOffset = 0;
                        break;
                    case 'm':
                        Minute = Minute * 10 + (c - '0');
                        break;
                    case 's':
                        Second = Second * 10 + (c - '0');
                        break;
                    case 'f':
                        MS = MS * 10 + (c - '0');
                        break;
                }

            }
        

            try
            {
                date = new System.DateTime(Year, Month, Day, Hour + HourOffset, Minute, Second, MS);
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }
    }
}
