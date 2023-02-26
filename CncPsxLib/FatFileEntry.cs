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

            /* This 'short' will always be zero for MIX file entries - derived from C&C PS1 source code:
             *
             *    unsigned long* FAT_FILE_DATA; // 4 bytes wide
             *    f = &XA_FILE;
             *    if (*(short *)(FAT_FILE_DATA + entryIndex * 7 + 6) == 0) { // entryIndex is zero based
             *      f = &MIX_FILE;
             *    }
             */
            var isInMixFile = BitConverter.ToUInt16(bytes[24..26]) == 0;
            var chunkSize = isInMixFile ? MIX_CD_SECTOR_SIZE : XA_CD_SECTOR_SIZE;

            return new()
            {
                FileName = Encoding.ASCII.GetString(bytes[..12]).Split("\0").First(),
                LeadInBytes = bytes[12..16],
                OffsetInCdSectors = (BitConverter.ToUInt32(bytes[16..20])),
                CdSectorSizeInBytes = chunkSize,
                SizeInBytes = BitConverter.ToUInt32(bytes[20..24]),
                LeadOutBytes = bytes[26..],
                IsInMixFile = isInMixFile
            };
        }

        public int Index { get; set; }

#pragma warning disable CS8618 
        public string FileName { get; set; }
#pragma warning restore CS8618

        public string HexFileName => $"{Index.ToString("X").PadLeft(8, '0')}.{FileExtension}";

        public string FileExtension => Path.GetExtension(FileName).Remove(0, 1);

        public bool IsTextFile => FileConstants.TEXT_EXTENSIONS.Contains(FileExtension.ToUpper());

        // unknown what this value means when non-zero
#pragma warning disable CS8618
        public byte[] LeadInBytes { get; set; }
#pragma warning restore CS8618 

        public uint OffsetInCdSectors { get; set; }

        public uint CdSectorSizeInBytes { get; set; }

        public uint OffsetInBytes
        {
            get => OffsetInCdSectors * CdSectorSizeInBytes;
            set
            {
                if (value % CdSectorSizeInBytes != 0)
                {
                    throw new InvalidDataException($"File offset '{value}' was not divisible by sector size '{CdSectorSizeInBytes}'");
                }

                OffsetInCdSectors = value / CdSectorSizeInBytes;
            }
        }

        public string HexOffsetInBytes => OffsetInBytes.ToString("X").PadLeft(8, '0');

        public uint SizeInBytes { get; set; }

        public uint SizeInSectors => SizeInBytes / CdSectorSizeInBytes;

        public string SizeInByteUnits => SizeInBytes.FormatAsByteUnit();

        // unknown what this value means when non-zero
#pragma warning disable CS8618
        public byte[] LeadOutBytes { get; set; }
#pragma warning restore CS8618

        public bool IsInMixFile { get; set; }

        public bool IsInXaFile => !IsInMixFile;

        public byte[] ToBytes() =>
            Encoding.ASCII.GetBytes(FileName.PadRight(12, '\0'))
                .Concat(LeadInBytes)
                .Concat(BitConverter.GetBytes(OffsetInCdSectors))
                .Concat(BitConverter.GetBytes(SizeInBytes))
                .Concat(new byte[] { (byte)(IsInMixFile ? 0 : 1), 0 }) // see FromBytes comments
                .Concat(LeadOutBytes)
                .ToArray();
    }
}
