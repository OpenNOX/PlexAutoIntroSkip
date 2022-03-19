using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;

namespace PlexAutoIntroSkip
{
    public static class WebDriverManager
    {
        public static readonly string MsEdgeDriverFilePath = Path.GetFullPath(@"./edgedriver_win64/msedgedriver.exe");

        public static readonly string MsEdgeDriverDirectoryName = "edgedriver_win64";

        public static readonly string MsEdgeDriverFileName = "msedgedriver.exe";

        private static readonly string _stableChannelLatestVersionAddress = "https://msedgedriver.azureedge.net/LATEST_STABLE";

        private static readonly string _stableChannelAddress = "https://msedgedriver.azureedge.net/<<VERSION>>/edgedriver_win64.zip";

        private static readonly Regex _msEdgeDriverVersionRegEx = new(@"(\d+\.\d+\.\d+)");

        public static void AutoUpdate()
        {
            var webClient = new WebClient();
            var httpStream = webClient.OpenRead(_stableChannelLatestVersionAddress);
            var streamReader = new StreamReader(httpStream);
            var latestVersion = streamReader.ReadLine();

            streamReader.Close();
            httpStream.Close();

            var currentVersion = string.Empty;
            if (File.Exists(MsEdgeDriverFilePath))
            {
                currentVersion = GetMsEdgeDriverVersion(MsEdgeDriverFilePath);
            }

            if (currentVersion != _msEdgeDriverVersionRegEx.Match(latestVersion).Value)
            {
                var downloadFileName = $"./{MsEdgeDriverDirectoryName}.zip";

                webClient.DownloadFile(_stableChannelAddress.Replace("<<VERSION>>", latestVersion), downloadFileName);
                ZipFile.ExtractToDirectory(downloadFileName, MsEdgeDriverDirectoryName, overwriteFiles: true);
            }

            webClient.Dispose();
        }

        private static string GetMsEdgeDriverVersion(string msEdgeDriverPath)
        {
            var msEdgeDriver = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = msEdgeDriverPath,
                    Arguments = "--version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                }
            };

            msEdgeDriver.Start();
            while (msEdgeDriver.StandardOutput.EndOfStream == false)
            {
                return _msEdgeDriverVersionRegEx.Match(msEdgeDriver.StandardOutput.ReadLine()).Value;
            }

            return string.Empty;
        }
    }
}
