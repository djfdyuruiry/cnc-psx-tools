namespace CncPsxLib
{
    public static class IntExtensions
    {
        private static readonly string[] BYTE_UNITS = { "B", "KB", "MB", "GB" };

        public static string FormatAsByteUnit(this Int32 bytes)
        {
            int i;
            var unitSize = bytes;

            for (i = 0; i < BYTE_UNITS.Length && bytes >= 1024; i++, bytes /= 1024)
            {
                unitSize = bytes / 1024;
            }

            return $"{unitSize:0.##}{BYTE_UNITS[i]}";
        }
    }
}
