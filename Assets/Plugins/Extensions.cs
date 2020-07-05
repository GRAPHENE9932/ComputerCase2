using System;
using System.Collections.Generic;
using System.Linq;

namespace KlimSoft
{
    public static class Extensions
    {
        /// <summary>
        /// Removes chars from string.
        /// </summary>
        /// <param name="c">Char, needed to remove.</param>
        public static string RemoveChar(this string str, char c)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == c)
                {
                    str = str.Remove(i, 1);
                    i--;
                }
            }
            return str;
        }

        /// <summary>
        /// Takes only first {count} chars from string.
        /// </summary>
        public static string TakeChars(this string str, int count)
        {
            string result = null;
            for (int i = 0; i < count && i < str.Length; i++)
                result += str[i];
            return result;
        }

        /// <summary>
        /// Like a common split, but ignores separator in brackets;
        /// </summary>
        /// <param name="c">Separator.</param>
        /// <param name="triggersFrom">Left brackets like a '(' or '{' or '[".</param>
        /// <param name="triggersTo">Right brackets like a ')' or '}' or ']".</param>
        /// <returns>Array with separated strings.</returns>
        public static string[] SmartSplit(this string s, char c, char[] triggersFrom, char[] triggersTo)
        {
            List<string> strings = new List<string>();
            int prevIndex = 0;
            int rank = 0;
            char? triggerFromNow = null;
            char? triggerToNow = null;
            for (int i = 0; i < s.Length; i++)
            {
                //If trigger not assigned and current char contains one of param triggers, assign the trigger.
                if (triggerFromNow == null && triggersFrom.Contains(s[i]))
                {
                    //Set trigger from.
                    triggerFromNow = s[i];
                    //Set trigger to base by trigger from.
                    triggerToNow = triggersTo[Array.IndexOf(triggersFrom, s[i])];
                }

                if (s[i] == triggerFromNow)
                    rank++;
                else if (s[i] == triggerToNow)
                    rank--;

                if (rank != 0)
                    continue;

                if (s[i] == c)
                {
                    strings.Add(s.Substring(prevIndex, i - prevIndex));
                    i = s.IndexOf(c, i);
                    prevIndex = i + 1;
                }
            }
            //Add the last element.
            strings.Add(s.Substring(prevIndex));
            //If only 1 element : "", clear it.
            if (strings.Count == 1 && string.IsNullOrEmpty(strings[0]))
                return new string[0];
            return strings.ToArray();
        }
    }
}