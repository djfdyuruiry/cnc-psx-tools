using System.Text;

namespace CncPsxLib
{
    public class FatFileEntry
    {
        private const int FAT_ENTRY_SIZE_IN_BYTES = 28;
        private const int FAT_FILE_CHUNK_SIZE = 2048;

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
