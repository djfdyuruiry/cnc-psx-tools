namespace CncPsxLib
{
    public class FatFileWriter
    {
        private async Task WriteEntries(
            FileStream fatFile,
            List<FatFileEntry> entries
        )
        {
            foreach (var entry in entries)
            {
                await fatFile.WriteAsync(entry.ToBytes());
            }
        }

        public async Task Write(FatFile fatFile, string filePath)
        {
            using (var fatFileHandle = File.OpenWrite(filePath))
            {
                await fatFileHandle.WriteAsync(BitConverter.GetBytes(fatFile.MixEntryCount));
                await fatFileHandle.WriteAsync(BitConverter.GetBytes(fatFile.XaEntryCount));

                await WriteEntries(fatFileHandle, fatFile.MixFileEntries);
                await WriteEntries(fatFileHandle, fatFile.XaFileEntries);
            }
        }
    }
}
