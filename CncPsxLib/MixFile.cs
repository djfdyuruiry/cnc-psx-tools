using System.Text;

namespace CncPsxLib
{
    public class MixFile
    {
        public FatFile FileTable { get; }

        public string MixFilePath { get; }

        public bool IsXaMixFile => Path.GetExtension(MixFilePath).ToUpper() == $".{FileConstants.XA_EXTENSION}";

        public List<FatFileEntry> FileEntries => IsXaMixFile ? FileTable.XaFileEntries : FileTable.MixFileEntries;

        public MixFile(FatFile fileTable, string mixFilePath)
        {
            FileTable = fileTable;
            MixFilePath = mixFilePath;
        }

        public async Task<byte[]> ReadFile(FatFileEntry entry)
        {
            using (var reader = MixFileReader.Open(MixFilePath))
            {
                return await reader.ReadFile(entry);
            }
        }

        public async Task<string> ReadFileText(FatFileEntry entry) =>
            Encoding.ASCII.GetString(await ReadFile(entry));

        public async Task AddFile(FatFileEntry entry, Stream entryData)
        {
            if (IsXaMixFile)
            {
                throw new NotImplementedException("Adding files to a XA archive is not working yet");
            }

            FileEntries.Add(entry);

            using (var writer = MixFileWriter.Open(MixFilePath))
            {
                entry.Index = FileEntries.Count;
                entry.OffsetInBytes = (uint)writer.MixStream.Position;

                await writer.WriteFile(entry, entryData);
            }
        }

        public async Task ReplaceFile(FatFileEntry fileEntry, Stream newData)
        {
            if (IsXaMixFile)
            {
                throw new NotImplementedException("Replacing files in a XA archive is not working yet");
            }

            var tmpFile = Path.GetTempFileName();

            fileEntry.SizeInBytes = (uint)newData.Length;

            using (var reader = MixFileReader.Open(MixFilePath))
            {
                using (var writer = MixFileWriter.Open(tmpFile))
                {
                    foreach (var entry in FileEntries)
                    {
                        var currentOffset = (uint)writer.MixStream.Position;

                        if (entry != fileEntry)
                        {
                            await writer.WriteFile(entry, await reader.ReadFile(entry));
                        }
                        else
                        {
                            await writer.WriteFile(entry, newData);
                        }

                        entry.OffsetInBytes = currentOffset;
                    }
                }
            }

            File.Move(tmpFile, MixFilePath, true);
            File.Delete(FileTable.Path);

            var fatWriter = new FatFileWriter();

            await fatWriter.Write(FileTable, FileTable.Path);
        }
    }
}
