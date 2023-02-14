using System.ComponentModel.DataAnnotations;

namespace CncPsxLib
{
    public class FatFile
    {
        public const int FAT_HEADER_SIZE_IN_BYTES = 8;

        public static FatFile HeaderFromBytes(string filePath, byte[] bytes)
        {
            if (bytes.Length < FAT_HEADER_SIZE_IN_BYTES)
            {
                throw new InvalidDataException(
                    $"Unable to parse FAT file header, expected {FAT_HEADER_SIZE_IN_BYTES} bytes, got {bytes.Length}"
                );
            }

            return new FatFile
            {
                Path = filePath,
                MixEntryCount = BitConverter.ToInt32(bytes[..4]),
                XaEntryCount = BitConverter.ToInt32(bytes[4..])
            };
        }

        public string Path { get; set; }

        public List<FatFileEntry> MixFileEntries { get; set; }

        public List<FatFileEntry> XaFileEntries { get; set; }

        public int MixEntryCount { get; set; }

        public int XaEntryCount { get; set; }


        private void AddFileEntry(FatFileEntry entry, List<FatFileEntry> destination)
        {
            var lastEntry = destination[-1];

            entry.Index = lastEntry.Index + 1;
            entry.OffsetInCdSectors = lastEntry.OffsetInCdSectors + lastEntry.SizeInSectors;

            destination.Add(entry);
        }

        public void AddMixFileEntry(FatFileEntry entry)
        {
            AddFileEntry(entry, MixFileEntries);
            MixEntryCount++;
        }

        public void AddXaFileEntry(FatFileEntry entry)
        {
            AddFileEntry(entry, XaFileEntries);
            XaEntryCount++;
        }
    }
}
