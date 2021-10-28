using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathExpressinTests
{
    public static class CharArrayExtensions
    {

        private static char nullvalue = (char)0;
        public static ref char Peek(this char[] chars, long currentPos)
        {
            if (currentPos + 1 > chars.Length)
            {
                return ref nullvalue;
            }

            return ref chars[currentPos];
        }

        public static ref char Read(this char[] chars, ref long currentPos)
        {
            currentPos++;
            if (currentPos > chars.Length)
            {
                return ref nullvalue;
            }
            return ref chars[currentPos -1];
        }

        /// <summary>
		/// Returns the next char from currentposition after skipping the stepOver Positions
		/// </summary>
		/// <param name="stepOver"></param>
		/// <returns></returns>
		public static ref char PeekSkip(this char[] chars, long currentPos, long stepOver)
        {
            if (currentPos + stepOver >= chars.Length)
            {
                return ref nullvalue;
            }

            return ref chars[currentPos + stepOver];

        }
    }
}
