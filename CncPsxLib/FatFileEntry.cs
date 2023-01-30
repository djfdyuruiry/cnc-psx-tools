﻿using System.Diagnostics.Metrics;

namespace CncPsxLib
{
    public class FatFileEntry
    {
        public int Index { get; set; }

        public string FileName { get; set;  }

        public string FileExtension => Path.GetExtension(FileName).Remove(0, 1);

        public string HexFileName => $"{Index.ToString("X").PadLeft(8, '0')}.{FileExtension}";

        public int OffsetInBytes { get; set; }

        public string HexOffsetInBytes => OffsetInBytes.ToString("X").PadLeft(8, '0');

        public int SizeInBytes { get; set; }

        public string SizeInByteUnits => SizeInBytes.FormatAsByteUnit();
    }
}
