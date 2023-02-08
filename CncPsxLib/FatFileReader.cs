using System.Text;

namespace CncPsxLib
{
    public class FatFileReader
    {
        private const int FAT_HEADER_SIZE_IN_BYTES = 8;
        private const int FAT_ENTRY_SIZE_IN_BYTES = 28;
        private const int FAT_FILE_CHUNK_SIZE = 2048;

        private static int DeserialiseInt32(byte[] bytes) => BitConverter.ToInt32(bytes);

        private static string DeserialiseAsciiString(byte[] bytes) =>
            Encoding.ASCII.GetString(bytes).Replace("\0", string.Empty);

        private void ReadEntry(
            int index,
            byte[] fileEntryBytes,
            int fileEntryCount,
            Dictionary<string, FatFileEntry> entries,
            Dictionary<string, FatFileEntry> appendixEntries
        )
        {
            var fileNameBytes = fileEntryBytes[..12];
            var offsetBytes = fileEntryBytes[16..20];
            var sizeBytes = fileEntryBytes[20..];

            var fileName = DeserialiseAsciiString(fileNameBytes);
            var sanitisedFileName = fileName;

            if (entries.ContainsKey(fileName))
            {
                // detect duplicate filename entries
                sanitisedFileName = fileName.Replace(".", "-1.");
            }

            var offsetInChunks = DeserialiseInt32(offsetBytes);

            var fatEntry = new FatFileEntry
            {
                Index = index,
                FileName = fileName,
                OffsetInBytes = offsetInChunks * FAT_FILE_CHUNK_SIZE,
                SizeInBytes = DeserialiseInt32(sizeBytes)
            };

            if (index <= fileEntryCount)
            {
                entries[sanitisedFileName] = fatEntry;
            }
            else
            {
                // note: there are entries listed in FAT files that point to unknown extra files
                appendixEntries[sanitisedFileName] = fatEntry;
            }
        }

        private async Task ReadEntries(
            FileStream fatFile,
            int fileEntryCount,
            Dictionary<string, FatFileEntry> entries,
            Dictionary<string, FatFileEntry> appendixEntries
        )
        {
            var index = 1;

            while (true)
            {
                var (readOk, fileEntryBytes) = await fatFile.ReadExactlyAsync(FAT_ENTRY_SIZE_IN_BYTES);

                if (!readOk)
                {
                    break;
                }

                ReadEntry(index, fileEntryBytes, fileEntryCount, entries, appendixEntries);
                index++;
            }
        }

        public async Task<FatFile> Read(string filePath)
        {
            int fileEntryCount, extraFileEntryCount = 0;
            var entries = new Dictionary<string, FatFileEntry>();
            var appendixEntries = new Dictionary<string, FatFileEntry>();

            using (var fatFile = File.OpenRead(filePath))
            {
                if (fatFile.Length < (FAT_HEADER_SIZE_IN_BYTES + FAT_ENTRY_SIZE_IN_BYTES))
                {
                    throw new InvalidDataException($"Path is not a FAT file or contains zero entries: {filePath}");
                }

                var (_, fileHeaderBytes) = await fatFile.ReadExactlyAsync(FAT_HEADER_SIZE_IN_BYTES);
                fileEntryCount = DeserialiseInt32(fileHeaderBytes[..4]);
                extraFileEntryCount = DeserialiseInt32(fileHeaderBytes[4..]);

                await ReadEntries(fatFile, fileEntryCount, entries, appendixEntries);
            }

            return new FatFile
            {
                Path = filePath,
                EntryCount = fileEntryCount,
                FileEntries = entries,
                ExtraEntryCount = extraFileEntryCount,
                ExtraFileEntries = appendixEntries
            };
        }
    }
}
