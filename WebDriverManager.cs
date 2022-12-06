using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace PlexAutoIntroSkip
{
    public static class WebDriverManager
    {
        /// <summary>
        /// MS Edge web driver executable path.
        /// </summary>
        public static readonly string MsEdgeDriverFilePath = Path.GetFullPath(@"./edgedriver_win64/msedgedriver.exe");

        public static readonly string MsEdgeDriverDirectoryName = "edgedriver_win64";

        public static readonly string MsEdgeDriverFileName = "msedgedriver.exe";

        private static readonly string _stableChannelAddress = "https://msedgedriver.azureedge.net/<<VERSION>>/edgedriver_win64.zip";

        private static readonly Regex _versionRegEx = new(@"^(\d+\.\d+\.\d+)");

        /// <summary>
        /// Automatically download and update MS Edge web driver.
        /// </summary>
        public static void AutoInstall()
        {
            var msEdgeVersion = GetMsEdgeVersion();
            var webDriverVersion = File.Exists(MsEdgeDriverFilePath) ? GetMsEdgeDriverVersion() : string.Empty;

            if (_versionRegEx.Match(msEdgeVersion).Value != webDriverVersion)
            {
                var webClient = new WebClient();

                var downloadFileName = $"./{MsEdgeDriverDirectoryName}.zip";

                webClient.DownloadFile(_stableChannelAddress.Replace("<<VERSION>>", msEdgeVersion), downloadFileName);
                ZipFile.ExtractToDirectory(downloadFileName, MsEdgeDriverDirectoryName, overwriteFiles: true);

                webClient.Dispose();
            }
        }

        /// <summary>
        /// Get current version of installed MS Edge.
        /// </summary>
        /// <returns>Version of MS Edge.</returns>
        private static string GetMsEdgeVersion()
        {
            if (OperatingSystem.IsWindows())
            {
                return Registry.GetValue(
                    @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Edge\BLBeacon", "version", null).ToString();
            }
            else
            {
                throw new Exception("Unable to determine current version of MS Edge on non-Windows platforms.");
            }
        }

        /// <summary>
        /// Get current version of downloaded MS Edge web driver.
        /// </summary>
        /// <returns>Version of MS Edge web driver.</returns>
        private static string GetMsEdgeDriverVersion()
        {
            var msEdgeDriver = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = MsEdgeDriverFilePath,
                    Arguments = "--version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                }
            };

            msEdgeDriver.Start();

            return msEdgeDriver.StandardOutput.EndOfStream
                ? string.Empty
                : _versionRegEx.Match(msEdgeDriver.StandardOutput.ReadLine()).Value;
        }
    }
}
