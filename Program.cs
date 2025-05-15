using CommandLine;
using DisplayController.Models;
using DisplayController.Services;
using DisplayController.Options;

namespace DisplayController
{
    public class Program
    {
        public static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<CommandLineOptions>(args)
                .MapResult(RunWithOptions, HandleParseError);
        }

        private static int RunWithOptions(CommandLineOptions options)
        {
            try
            {
                var displayService = new DisplayService();

                // Get current configuration
                if (options.GetConfig)
                {
                    var config = displayService.GetCurrentConfig();
                    Console.WriteLine("Current Monitor Configuration:");
                    foreach (var monitor in config.Monitors)
                    {
                        Console.WriteLine(monitor);
                    }
                    return 0;
                }

                // Save configuration to file
                if (!string.IsNullOrEmpty(options.SaveConfigFile))
                {
                    var config = displayService.GetCurrentConfig();
                    config.SaveToFile(options.SaveConfigFile);
                    Console.WriteLine($"Configuration saved to {options.SaveConfigFile}");
                    return 0;
                }

                // Load and apply configuration from file
                if (!string.IsNullOrEmpty(options.LoadConfigFile))
                {
                    var config = MonitorConfigCollection.LoadFromFile(options.LoadConfigFile);
                    bool success = displayService.ApplyConfig(config);
                    if (success)
                    {
                        Console.WriteLine("Configuration applied successfully");
                        return 0;
                    }
                    else
                    {
                        Console.Error.WriteLine("Failed to apply configuration");
                        return 1;
                    }
                }

                // Set primary monitor
                if (!string.IsNullOrEmpty(options.SetPrimaryId))
                {
                    bool success = displayService.SetPrimaryMonitor(options.SetPrimaryId);
                    if (success)
                    {
                        Console.WriteLine($"Monitor {options.SetPrimaryId} set as primary");
                        return 0;
                    }
                    else
                    {
                        Console.Error.WriteLine($"Failed to set monitor {options.SetPrimaryId} as primary");
                        return 1;
                    }
                }

                // Set resolution
                if (options.SetResolutionParams != null && options.SetResolutionParams.Count() == 3)
                {
                    var parameters = options.SetResolutionParams.ToArray();
                    string id = parameters[0];

                    if (int.TryParse(parameters[1], out int width) &&
                        int.TryParse(parameters[2], out int height))
                    {
                        bool success = displayService.ChangeResolution(id, width, height);
                        if (success)
                        {
                            Console.WriteLine($"Resolution for monitor {id} set to {width}x{height}");
                            return 0;
                        }
                        else
                        {
                            Console.Error.WriteLine($"Failed to set resolution for monitor {id}");
                            return 1;
                        }
                    }
                    else
                    {
                        Console.Error.WriteLine("Invalid resolution parameters. Width and height must be integers.");
                        return 1;
                    }
                }

                // Set display mode
                if (!string.IsNullOrEmpty(options.SetDisplayMode))
                {
                    bool mirrored = options.SetDisplayMode.ToLower() == "mirrored";
                    bool success = displayService.SetDisplayMode(mirrored);
                    if (success)
                    {
                        Console.WriteLine($"Display mode set to {options.SetDisplayMode}");
                        return 0;
                    }
                    else
                    {
                        Console.Error.WriteLine($"Failed to set display mode to {options.SetDisplayMode}");
                        return 1;
                    }
                }

                // If no option was selected, show help
                Console.WriteLine("No operation specified. Use --help to see available options.");
                return 1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }

        private static int HandleParseError(IEnumerable<Error> errors)
        {
            // Just let CommandLineParser handle displaying the error
            return 1;
        }
    }
}