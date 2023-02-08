using System.Text;

namespace CncPsxLib
{
    public class FatFileWriter
    {
        private async Task WriteEntries(
            FileStream fatFile,
            Dictionary<string, FatFileEntry> entries
        )
        {
            foreach (var (_, entry) in entries)
            {
                await fatFile.WriteAsync(entry.ToBytes());
            }
        }

        public async Task Write(FatFile fatFile, string filePath)
        {
            using (var fatFileHandle = File.OpenWrite(filePath))
            {
                await fatFileHandle.WriteAsync(BitConverter.GetBytes(fatFile.EntryCount));
                await fatFileHandle.WriteAsync(BitConverter.GetBytes(fatFile.ExtraEntryCount));

                await WriteEntries(fatFileHandle, fatFile.FileEntries);
                await WriteEntries(fatFileHandle, fatFile.ExtraFileEntries);
            }
        }
    }
}
