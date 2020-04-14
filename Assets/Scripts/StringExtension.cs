public static class StringExtension
{
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
