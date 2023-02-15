namespace CncPsxLib
{
    public class MixFileWriter : IDisposable
    {
        private readonly FileStream _mixStream;

        public string Path { get; }

        public static MixFileWriter Open(string path) => new(path);

        private MixFileWriter(string path)
        {
            Path = path;

            _mixStream = File.OpenWrite(Path);
        }

        public async Task WriteFile(FatFileEntry mixFileEntry, Stream entryDataStream)
        {
            if (_mixStream.Position < mixFileEntry.OffsetInBytes)
            {
                var paddingLength = mixFileEntry.OffsetInBytes - _mixStream.Position;
                var paddingBytes = Enumerable.Repeat<byte>(0, (int)paddingLength).ToArray();

                await _mixStream.WriteAsync(paddingBytes);
            }

            await entryDataStream.CopyToAsync(_mixStream);
        }

        public void Dispose() => _mixStream.Dispose();
    }
}
