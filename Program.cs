using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using CommandLine;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

namespace PlexAutoIntroSkip
{
    /// <summary>
    /// Program entry point.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Sets the specified window's show state.
        /// </summary>
        /// <remarks>
        /// See <see href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showwindow">ShowWindow</see> MS Docs for more information.
        /// </remarks>
        /// <param name="hWnd">A handle to the window.</param>
        /// <param name="nCmdShow">Controls how the window is to be shown.</param>
        /// <returns>
        /// If the window was previously visible, the return value is nonzero.
        /// If the window was previously hidden, the return value is zero.
        /// </returns>
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>
        /// Program entry point.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        public static void Main(string[] args)
        {
            var options = GetProgramOptions(args);

            HandleConsoleWindow(options.ShowConsoleWindow);

            var edgeOptions = new EdgeOptions
            {
                UseChromium = true,
            };

            // Disable "Chrome is being controlled by automated test software" infobar.
            edgeOptions.AddExcludedArgument("enable-automation");
            edgeOptions.AddAdditionalOption("useAutomationExtension", false);

            edgeOptions.AddArguments(
                $"user-data-dir={Directory.GetCurrentDirectory()}\\User Data",
                "profile-directory=Profile 1",
                $"app={options.PlexUrl}");

            var edgeProcessName = "msedge";
            var edgeProcessIds = Process.GetProcessesByName(edgeProcessName).Select(p => p.Id);

            EdgeDriverService service;
            if (options.ManualHandleWebDriver)
            {
                service = EdgeDriverService.CreateDefaultService();
            }
            else
            {
                WebDriverManager.AutoUpdate();

                service = EdgeDriverService.CreateDefaultService(
                    WebDriverManager.MsEdgeDriverDirectoryName, WebDriverManager.MsEdgeDriverFileName);
            }

            var driver = new EdgeDriver(service, edgeOptions);
            var browserProcessId = Process.GetProcessesByName(edgeProcessName).Select(p => p.Id)
                .Except(edgeProcessIds)
                .First();

            while (ProcessExistsById(browserProcessId))
            {
                try
                {
                    MainProgramLoop(driver, options, browserProcessId);
                }
                catch (Exception exception)
                {
                    LogException(exception);
                }
            }

            Process.GetProcessById(service.ProcessId).Kill();
        }

        private static void HandleConsoleWindow(bool showConsoleWindow)
        {
            var hWnd = Process.GetCurrentProcess().MainWindowHandle;

            // Hide console window and called using own process window?
            if (showConsoleWindow == false && hWnd.ToInt32() != 0)
            {
                ShowWindow(hWnd, 0);
            }
        }

        /// <summary>
        /// Run main program loop.
        /// </summary>
        /// <param name="driver"><see name="RemoteWebDriver"/> to be used to drive web browser.</param>
        /// <param name="options"><see name="ProgramOptions"/>.</param>
        /// <param name="browserProcessId">Web browser process ID to check if exists.</param>
        private static void MainProgramLoop(RemoteWebDriver driver, ProgramOptions options, int browserProcessId)
        {
            var waitDriver = new WebDriverWait(driver, TimeSpan.FromDays(365));
            var nullWebElement = new RemoteWebElement(driver, string.Empty);

            var skipIntroButtonXPath = "//button[text()='Skip Intro']";
            while (ProcessExistsById(browserProcessId))
            {
                // Waiting for Skip Intro button to be visible, or for browser to be closed manually.
                var skipIntroButton = (RemoteWebElement)waitDriver.Until(webDriver =>
                    ProcessExistsById(browserProcessId)
                        ? webDriver.FindElement(By.XPath(skipIntroButtonXPath))
                        : nullWebElement);

                // Skip Intro button will only be equal to `nullWebElement` if the browser process
                // no longer exists.
                if (skipIntroButton == nullWebElement)
                {
                    break;
                }

                // Skip Intro button is visible before intro starts, so wait for the actual intro to start.
                Thread.Sleep(options.SkipButtonWaitTime);

                skipIntroButton.Click();

                // Waiting for Skip Intro button to no longer be visible.
                waitDriver.Until(webDriver =>
                {
                    try
                    {
                        webDriver.FindElement(By.XPath(skipIntroButtonXPath));

                        return false;
                    }
                    catch
                    {
                        return true;
                    }
                });
            }
        }

        /// <summary>
        /// Parse <paramref name="args"/> into <see cref="ProgramOptions"/>.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        /// <returns><see cref="ProgramOptions"/></returns>
        private static ProgramOptions GetProgramOptions(string[] args)
        {
            ProgramOptions options = null;
            CommandLine.Parser.Default.ParseArguments<ProgramOptions>(args)
                .WithParsed(parsedOptions => options = parsedOptions);

            return options;
        }

        /// <summary>
        /// Recursively write <see name="Exception"/> to error log file.
        /// </summary>
        /// <param name="exception">Root <see name="Exception"/>.</param>
        private static void LogException(Exception exception)
        {
            using var writer = new StreamWriter("error.log", append: true);
            writer.WriteLine("--------------------------------------------------");
            writer.WriteLine($"[{DateTime.Now}]");
            writer.WriteLine();

            while (exception != null)
            {
                writer.WriteLine(exception.GetType().FullName);
                writer.WriteLine(exception.Message);
                writer.WriteLine(exception.StackTrace);
                writer.WriteLine();

                exception = exception.InnerException;
            }
        }

        /// <summary>
        /// Check if the process exists by given <paramref name="processId"/>.
        /// </summary>
        /// <param name="processId">Process ID.</param>
        /// <returns>True if process exists, otherwise false.</returns>
        private static bool ProcessExistsById(int processId)
            => Process.GetProcesses().Any(p => p.Id == processId);
    }
}
