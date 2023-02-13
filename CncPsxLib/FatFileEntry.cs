using System.Text;

namespace CncPsxLib
{
    public class FatFileEntry
    {
        public const int ENTRY_SIZE_IN_BYTES = 28;
        public const uint MIX_CD_SECTOR_SIZE = 2048;
        public const uint XA_CD_SECTOR_SIZE = 2336;

        public static FatFileEntry FromBytes(byte[] bytes)
        {
            if (bytes.Length < ENTRY_SIZE_IN_BYTES)
            {
                throw new InvalidDataException(
                    $"Insufficent data to parse FAT file entry. Expected {ENTRY_SIZE_IN_BYTES} bytes, got {bytes.Length}"
                );
            }

            /* This 'short' will always be zero for MIX file entries - copied from C&C PS1 source code:
             *
             *    unsigned long* FAT_FILE_DATA; // 4 bytes wide
             *    f = &XA_FILE;
             *    if (*(short *)(FAT_FILE_DATA + entryIndex * 7 + 6) == 0) { // entryIndex is zero based
             *      f = &MIX_FILE;
             *    }
             */
            var isInMixFile = BitConverter.ToUInt16(bytes[24..26]) == 0;
            var chunkSize = isInMixFile ? MIX_CD_SECTOR_SIZE : XA_CD_SECTOR_SIZE;

            return new FatFileEntry
            {
                FileName = Encoding.ASCII.GetString(bytes[..12]).Replace("\0", string.Empty),
                LeadInBytes = bytes[12..16],
                OffsetInCdSectors = (BitConverter.ToUInt32(bytes[16..20])),
                CdSectorSizeInBytes = chunkSize,
                SizeInBytes = BitConverter.ToUInt32(bytes[20..24]),
                LeadOutBytes = bytes[24..],
                IsInMixFile = isInMixFile
            };
        }

        public int Index { get; set; }

        public string FileName { get; set;  }

        public string FileExtension => Path.GetExtension(FileName).Remove(0, 1);

        public string HexFileName => $"{Index.ToString("X").PadLeft(8, '0')}.{FileExtension}";

        public byte[] LeadInBytes { get; set; }

        public uint OffsetInCdSectors { get; set; }

        public uint CdSectorSizeInBytes { get; set; }

        public uint OffsetInBytes => OffsetInCdSectors * CdSectorSizeInBytes;

        public string HexOffsetInBytes => OffsetInBytes.ToString("X").PadLeft(8, '0');

        public uint SizeInBytes { get; set; }

        public string SizeInByteUnits => SizeInBytes.FormatAsByteUnit();

        public byte[] LeadOutBytes { get; set; }

        public bool IsInMixFile { get; set; }

        public bool IsInXaFile => !IsInMixFile;

        public byte[] ToBytes() =>
            Encoding.ASCII.GetBytes(FileName.PadRight(12, '\0'))
                .Concat(LeadInBytes)
                .Concat(BitConverter.GetBytes(OffsetInCdSectors))
                .Concat(BitConverter.GetBytes(SizeInBytes))
                .Concat(LeadOutBytes)
                .ToArray();
    }
}
