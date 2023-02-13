namespace CncPsxLib
{
    public static class UIntExtensions
    {
        private static readonly string[] BYTE_UNITS = { "B", "KB", "MB", "GB" };

        public static string FormatAsByteUnit(this UInt32 bytes)
        {
            int i;
            double unitSize = bytes;

            for (i = 0; i < BYTE_UNITS.Length && bytes >= 1024; i++, bytes /= 1024)
            {
                unitSize = Math.Ceiling((double)bytes / 1024);
            }

            return $"{unitSize:0.##}{BYTE_UNITS[i]}";
        }
    }
}
