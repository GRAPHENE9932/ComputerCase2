using System;
using System.Collections.Generic;
using System.Linq;

public static class Extensions
{
    //Removes char from string.
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
}
