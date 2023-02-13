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

            var (readOk, fileByteBuffer) = await _mixStream.ReadExactlyAsync((int)mixFileEntry.SizeInBytes);

            if (!readOk)
            {
                throw new InvalidDataException("Corrupt MIX file or FAT/MIX mismatch: data for file entry missing");
            }

            return fileByteBuffer;
        }

        public void Dispose() => _mixStream.Dispose();
    }
}
