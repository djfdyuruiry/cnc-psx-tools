namespace CncPsxLib
{
    public static class DirectoryExtensions
    {
        public static void EnsureDirectoryExists(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }            
        }

        public static void EnsureDirectoryExistsAndIsEmpty(string directoryPath)
        {
            EnsureDirectoryExists(directoryPath);
            Directory.CreateDirectory(directoryPath);
        }
    }
}
