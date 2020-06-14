using System;
using System.Collections.Generic;
using System.Linq;

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
}
