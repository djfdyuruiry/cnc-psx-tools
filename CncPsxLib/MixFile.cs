namespace CncPsxLib
{
    public class MixFile : IDisposable
    {
        private readonly FileStream _mixStream;

        public string Path { get; }

        public static MixFile Open(string path) => new(path);

        private MixFile(string path)
        {
            Path = path;

            _mixStream = File.OpenRead(Path);
        }

        public async Task<byte[]> ReadFile(FatFileEntry mixFileEntry)
        {
            _mixStream.Position = mixFileEntry.OffsetInBytes;

            var fileByteBuffer = new byte[mixFileEntry.SizeInBytes];
            await _mixStream.ReadAsync(fileByteBuffer, 0, fileByteBuffer.Length);

            return fileByteBuffer;
        }

        public void Dispose() => _mixStream.Dispose();
    }
}
