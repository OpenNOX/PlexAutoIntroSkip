using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

namespace PlexAutoIntroSkip
{
    public class Program
    {
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static void Main(string[] args)
        {
            // Hide console application's console window.
            var hWnd = Process.GetCurrentProcess().MainWindowHandle;

            // Called using own process window?
            if (hWnd.ToInt32() != 0)
            {
                ShowWindow(hWnd, 0);
            }

            var driver = OpenBrowserToPlexApp(args);
            MainProgramLoop(driver);
        }

        private static RemoteWebDriver OpenBrowserToPlexApp(string[] args)
        {
            var plexUrl = args[0];
            var currentWorkingPath = Directory.GetCurrentDirectory();
            var options = new EdgeOptions();
            options.UseChromium = true;

            // Disable infobar "Chrome is being controlled by automated test software".
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);

            options.AddArguments(
                $"user-data-dir={currentWorkingPath}\\User Data",
                "profile-directory=Profile 1",
                $"app={plexUrl}");

            var driver = new EdgeDriver(options);;

            return driver;
        }

        private static void MainProgramLoop(RemoteWebDriver driver)
        {
            var waitDriver = new WebDriverWait(driver, TimeSpan.FromDays(365));

            var skipIntroButtonXPath = "//button[text()='Skip Intro']";
            RemoteWebElement skipIntroButton;
            while (true)
            {
                try
                {
                    // Waiting for Skip Intro button...
                    skipIntroButton = (RemoteWebElement)waitDriver.Until(webDriver =>
                    {
                        // Check if web browser window exists by getting window's position.
                        _ = driver.Manage().Window.Position;

                        return webDriver.FindElement(By.XPath(skipIntroButtonXPath));
                    });
                }
                catch (WebDriverException)
                {
                    // Unable to connect to web browser window, was most likely closed manually.
                    driver.Quit();
                    break;
                }

                // Skip Intro button is visible before intro starts, so wait for the actual intro to start.
                Thread.Sleep(2000);

                skipIntroButton.Click();

                // Waiting for Skip Intro button to not be visible...
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
    }
}
