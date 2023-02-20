namespace CncPsxLib
{
    public class MixFileWriter : IDisposable
    {
        public FileStream MixStream { get; }

        public string Path { get; }

        public static MixFileWriter Open(string path) => new(path);

        private MixFileWriter(string path)
        {
            Path = path;

            MixStream = File.OpenWrite(Path);
        }

        private async Task WriteEndSectorPaddingIfRequired(uint sectorSizeInBytes)
        {
            var dataBytesInEndSector = MixStream.Position % sectorSizeInBytes;

            if (dataBytesInEndSector == 0)
            {
                return;
            }

            var bytesRequiredToFillEndSector = sectorSizeInBytes - dataBytesInEndSector;
            var paddingBytes = Enumerable.Repeat<byte>(0, (int)bytesRequiredToFillEndSector).ToArray();

            await MixStream.WriteAsync(paddingBytes);
        }

        public async Task WriteFile(FatFileEntry mixFileEntry, Stream entryDataStream)
        {
            await entryDataStream.CopyToAsync(MixStream);

            await WriteEndSectorPaddingIfRequired(mixFileEntry.CdSectorSizeInBytes);
        }
        
        public async Task WriteFile(FatFileEntry mixFileEntry, byte[] entryData)
        {
            await MixStream.WriteAsync(entryData);

            await WriteEndSectorPaddingIfRequired(mixFileEntry.CdSectorSizeInBytes);
        }

        public void Dispose() => MixStream.Dispose();
    }
}
