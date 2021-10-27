using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathExpressinTests
{
    public static class CharArrayExtensions
    {
        public static int Peek(this char[] chars, long currentPos)
        {
            if (currentPos + 1 > chars.Length)
            {
                return -1;
            }
            return chars[currentPos];
        }

        public static int Read(this char[] chars, ref long currentPos)
        {
            currentPos++;
            if (currentPos > chars.Length)
            {
                return -1;
            }
            return chars[currentPos -1];
        }

        /// <summary>
		/// Returns the next char from currentposition after skipping the stepOver Positions
		/// </summary>
		/// <param name="stepOver"></param>
		/// <returns></returns>
		public static int PeekSkip(this char[] chars, long currentPos, long stepOver)
        {
            if (currentPos + stepOver >= chars.Length)
            {
                return -1;
            }

            return chars[currentPos + stepOver];

        }
    }
}
