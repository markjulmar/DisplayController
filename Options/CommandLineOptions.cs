using CommandLine;
using CommandLine.Text;

namespace DisplayController.Options
{
    /// <summary>
    /// Represents the command line options for the application
    /// </summary>
    public class CommandLineOptions
    {
        [Option('g', "get-config", Required = false, HelpText = "Get the current monitor configuration.")]
        public bool GetConfig { get; set; }

        [Option('s', "save-config", Required = false, HelpText = "Save the current monitor configuration to a file.")]
        public string? SaveConfigFile { get; set; }

        [Option('l', "load-config", Required = false, HelpText = "Load and apply a monitor configuration from a file.")]
        public string? LoadConfigFile { get; set; }

        [Option('p', "set-primary", Required = false, HelpText = "Set the primary monitor by its ID.")]
        public string? SetPrimaryId { get; set; }

        [Option('r', "set-resolution", Required = false, HelpText = "Set the resolution for a monitor by ID, width, and height.")]
        public IEnumerable<string>? SetResolutionParams { get; set; }

        [Option('m', "set-mode", Required = false, HelpText = "Set the display mode (extended|mirrored).")]
        public string? SetDisplayMode { get; set; }

        [Usage(ApplicationAlias = "DisplayController")]
        public static IEnumerable<Example> Examples => new List<Example>() {
                    new("Get the current monitor configuration",
                        new CommandLineOptions { GetConfig = true }),
                    new("Save the current configuration to a file",
                        new CommandLineOptions { SaveConfigFile = "config.json" }),
                    new("Load and apply a configuration from a file",
                        new CommandLineOptions { LoadConfigFile = "config.json" }),
                    new("Set a monitor as primary by ID",
                        new CommandLineOptions { SetPrimaryId = "MONITOR-ID-HERE" }),
                    new("Set resolution for a monitor",
                        new CommandLineOptions { SetResolutionParams = new[] { "MONITOR-ID-HERE", "1920", "1080" } }),
                    new("Set display mode to extended",
                        new CommandLineOptions { SetDisplayMode = "extended" })
                };
    }
}