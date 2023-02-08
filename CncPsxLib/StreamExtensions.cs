namespace CncPsxLib
{
    public static class StreamExtensions
    {
        public static async Task<(bool, byte[])> ReadExactlyAsync(this Stream stream, int byteCount)
        {
            var buffer = new byte[byteCount];
            var bytesRead = await stream.ReadAsync(buffer, 0, byteCount);

            return (bytesRead == byteCount, buffer);
        }
    }
}
