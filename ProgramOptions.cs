using CommandLine;

namespace PlexAutoIntroSkip
{
    public class ProgramOptions
    {
        [Option('d', "debug", Required = false, HelpText = "Show console window.")]
        public bool ShowConsoleWindow { get; set; }

        [Option('w', "wait-time", Required = false, Default = 3000,
            HelpText = "Time to wait after Skip Button becomes visible before clicking.")]
        public int SkipButtonWaitTime { get; set; }

        [Value(0, MetaName = "plex-url", HelpText = "Plex URL to use.")]
        public string PlexUrl { get; set; }
    }
}
