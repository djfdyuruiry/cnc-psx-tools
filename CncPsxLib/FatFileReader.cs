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
            var fatEntry = FatFileEntry.FromBytes(index, fileEntryBytes); 
            var sanitisedFileName = fatEntry.FileName;

            if (fatFile.MixFileEntries.ContainsKey(sanitisedFileName))
            {
                // detect duplicate filename entries
                sanitisedFileName = sanitisedFileName.Replace(".", "-1.");
            }

            if (index <= fatFile.MixEntryCount)
            {
                fatFile.MixFileEntries[sanitisedFileName] = fatEntry;
            }
            else
            {
                fatFile.XaFileEntries[sanitisedFileName] = fatEntry;
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
                var (readOk, fileEntryBytes) = await fatFileHandle.ReadExactlyAsync(FatFileEntry.FAT_ENTRY_SIZE_IN_BYTES);

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
                if (fatFileHandle.Length < (FatFile.FAT_HEADER_SIZE_IN_BYTES + FatFileEntry.FAT_ENTRY_SIZE_IN_BYTES))
                {
                    throw new InvalidDataException($"Path is not a FAT file or contains zero entries: {filePath}");
                }

                var (_, headerBytes) = await fatFileHandle.ReadExactlyAsync(FatFile.FAT_HEADER_SIZE_IN_BYTES);
                fatFile = FatFile.HeaderFromBytes(filePath, headerBytes);

                fatFile.MixFileEntries = new Dictionary<string, FatFileEntry>();
                fatFile.XaFileEntries = new Dictionary<string, FatFileEntry>();

                await ReadEntries(fatFileHandle, fatFile);
            }

            return fatFile;
        }
    }
}
