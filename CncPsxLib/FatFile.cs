namespace CncPsxLib
{
    public class FatFile
    {
        public const int FAT_HEADER_SIZE_IN_BYTES = 8;

        public static FatFile HeaderFromBytes(string filePath, byte[] bytes)
        {
            if (bytes.Length < FAT_HEADER_SIZE_IN_BYTES)
            {
                throw new InvalidDataException($"Unable to parse FAT file header, expected {FAT_HEADER_SIZE_IN_BYTES} bytes, got {bytes.Length}");
            }

            return new FatFile
            {
                Path = filePath,
                MixEntryCount = BitConverter.ToInt32(bytes[..4]),
                XaEntryCount = BitConverter.ToInt32(bytes[4..])
            };
        }

        public string Path { get; set; }

        public Dictionary<string, FatFileEntry> MixFileEntries { get; set; }

        public Dictionary<string, FatFileEntry> XaFileEntries { get; set; }

        public int MixEntryCount { get; set; }

        public int XaEntryCount { get; set; }
    }
}
