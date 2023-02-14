namespace CncPsxLib
{
    public class FatFileReader
    {
        private void ReadEntry(
            int index,
            byte[] fileEntryBytes,
            FatFile fatFile
        )
        {
            var fatEntry = FatFileEntry.FromBytes(fileEntryBytes);

            fatEntry.Index = fatEntry.IsInMixFile ? index : index - fatFile.MixEntryCount;

            if (fatEntry.IsInMixFile)
            {
                fatFile.MixFileEntries.Add(fatEntry);
            }
            else
            {
                fatFile.XaFileEntries.Add(fatEntry);
            }
        }

        private async Task ReadEntries(
            FileStream fatFileHandle,
            FatFile fatFile
        )
        {
            var index = 1;

            while (true)
            {
                var (readOk, fileEntryBytes) = await fatFileHandle.ReadExactlyAsync(FatFileEntry.ENTRY_SIZE_IN_BYTES);

                if (!readOk)
                {
                    break;
                }

                ReadEntry(index, fileEntryBytes, fatFile);
                index++;
            }
        }

        public async Task<FatFile> Read(string filePath)
        {
            FatFile fatFile;

            using (var fatFileHandle = File.OpenRead(filePath))
            {
                if (fatFileHandle.Length < (FatFile.FAT_HEADER_SIZE_IN_BYTES + FatFileEntry.ENTRY_SIZE_IN_BYTES))
                {
                    throw new InvalidDataException($"Path is not a FAT file or contains zero entries: {filePath}");
                }

                var (_, headerBytes) = await fatFileHandle.ReadExactlyAsync(FatFile.FAT_HEADER_SIZE_IN_BYTES);
                fatFile = FatFile.HeaderFromBytes(filePath, headerBytes);

                fatFile.MixFileEntries = new List<FatFileEntry>();
                fatFile.XaFileEntries = new List<FatFileEntry>();

                await ReadEntries(fatFileHandle, fatFile);
            }

            return fatFile;
        }
    }
}
