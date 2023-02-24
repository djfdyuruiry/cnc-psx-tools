using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using mkpsxisoUI.Model;

namespace mkpsxisoUI.Services
{
    public class ReleaseDownloader
    {
        private const string RELEASES_PATH = "Lameguy64/mkpsxiso/releases";
        private const string RELEASES_URL = $"https://github.com/{RELEASES_PATH}";

        private readonly HttpClient _httpClient = new();

        public async Task<Release> GetLatestRelease()
        {
            var releasesHtml = await _httpClient.GetStringAsync(RELEASES_URL);

            var tagRegex = new Regex($@"{RELEASES_PATH}/tag/([^/""]+)");
            var tag = tagRegex.Match(releasesHtml).Groups[1].Value;

            var assetsHtml = await _httpClient.GetStringAsync($"{RELEASES_URL}/expanded_assets/{tag}");

            var architecture = "win64";

            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                architecture = "Linux";
            }

            var escapedTag = tag.Replace(".", "[.]");
            var downloadUrlRegex = new Regex($@"href=""/{RELEASES_PATH}/(download/{escapedTag}/[^""/]+-{architecture}.zip)""");
            var downloadPath = downloadUrlRegex.Match(assetsHtml).Groups[1].Value;

            return new()
            {
                Version = tag,
                DownloadUrl = $"{RELEASES_URL}/{downloadPath}"
            };
        }

        public async Task DownloadAndInstallRelease(Release release, string installPath)
        {
            var zipBytes = await _httpClient.GetByteArrayAsync(release.DownloadUrl);
            var zipTempFile = Path.GetTempFileName();
            
            await File.WriteAllBytesAsync(zipTempFile, zipBytes);

            if (Directory.Exists(installPath))
            {
                Directory.Delete(installPath, true);
            }

            ZipFile.ExtractToDirectory(zipTempFile, installPath);
        }
    }
}
