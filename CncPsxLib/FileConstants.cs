namespace CncPsxLib
{
    public static class FileConstants
    {
        public const string FAT_EXTENSION = "FAT";
        public const string MIX_EXTENSION = "MIX";
        public const string XA_EXTENSION = "XA";
        public static readonly IList<string> TEXT_EXTENSIONS;

        static FileConstants()
        {
            TEXT_EXTENSIONS = new List<string>
            {
                "INI",
                "ENG",
                "GER",
                "FRE"
            };
        }
    }
}
