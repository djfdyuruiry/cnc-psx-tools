using System.Text.RegularExpressions;

namespace CncPsxLib
{
    public static class StringExtensions
    {
        public static string StripLeadingWhitespace(this string s) =>
            (new Regex(@"^\s+", RegexOptions.Multiline)).Replace(s, string.Empty);
    }
}
