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

        private void ReadEntry(int index, byte[] fileEntryBytes, Dictionary<string, FatFileEntry> entries)
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

            entries[sanitisedFileName] = new FatFileEntry
            {
                Index = index,
                FileName = fileName,
                OffsetInBytes = offsetInChunks * FAT_FILE_CHUNK_SIZE,
                SizeInBytes = DeserialiseInt32(sizeBytes)
            };
        }

        private async Task ReadEntries(FileStream fatFile, Dictionary<string, FatFileEntry> entries)
        {
            // scan past file header
            fatFile.Seek(FAT_HEADER_SIZE_IN_BYTES, SeekOrigin.Begin);

            var index = 0;
            var fileEntryBytes = new byte[FAT_ENTRY_SIZE_IN_BYTES];

            while ((await fatFile.ReadAsync(fileEntryBytes, 0, fileEntryBytes.Length)) > 0)
            {
                ReadEntry(index, fileEntryBytes, entries);
                index++;
            }
        }

        public async Task<FatFile> Read(string filePath)
        {
            var entries = new Dictionary<string, FatFileEntry>();

            using (var fatFile = File.OpenRead(filePath))
            {
                if (fatFile.Length < (FAT_HEADER_SIZE_IN_BYTES + FAT_ENTRY_SIZE_IN_BYTES))
                {
                    throw new FormatException($"Path is not a FAT file or contains zero entries: {filePath}");
                }

                await ReadEntries(fatFile, entries);
            }

            return new FatFile
            {
                Path = filePath,
                FileEntries = entries
            };
        }
    }
}
