using System.Text;

namespace CncPsxLib
{
    public class FatFileEntry
    {
        public const int FAT_ENTRY_SIZE_IN_BYTES = 28;
        public const int FAT_FILE_CHUNK_SIZE = 2048;

        public static FatFileEntry FromBytes(int entryIndex, byte[] bytes)
        {
            if (bytes.Length < FAT_ENTRY_SIZE_IN_BYTES)
            {
                throw new InvalidDataException(
                    $"Insufficent data to parse FAT file entry. Expected {FAT_ENTRY_SIZE_IN_BYTES} bytes, got {bytes.Length}"
                );
            }

            return new FatFileEntry
            {
                Index = entryIndex,
                FileName = Encoding.ASCII.GetString(bytes[..12]).Replace("\0", string.Empty),
                OffsetInBytes = (BitConverter.ToInt32(bytes[16..20])) * FAT_FILE_CHUNK_SIZE,
                SizeInBytes = BitConverter.ToInt32(bytes[20..])
            };
        }

        public int Index { get; set; }

        public string FileName { get; set;  }

        public string FileExtension => Path.GetExtension(FileName).Remove(0, 1);

        public string HexFileName => $"{Index.ToString("X").PadLeft(8, '0')}.{FileExtension}";

        public int OffsetInBytes { get; set; }

        public string HexOffsetInBytes => OffsetInBytes.ToString("X").PadLeft(8, '0');

        public int SizeInBytes { get; set; }

        public string SizeInByteUnits => SizeInBytes.FormatAsByteUnit();

        public byte[] ToBytes() =>
            Encoding.ASCII.GetBytes(FileName.PadRight(12, '\0'))
                .Concat(Enumerable.Repeat<byte>(0, 4))
                .Concat(BitConverter.GetBytes(OffsetInBytes / FAT_FILE_CHUNK_SIZE))
                .Concat(BitConverter.GetBytes(SizeInBytes))
                .Concat(Enumerable.Repeat<byte>(0, 4))
                .ToArray();
    }
}
