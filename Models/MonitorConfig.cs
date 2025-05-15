using System.Text.Json;

namespace DisplayController.Models
{
    /// <summary>
    /// Represents the display orientation of a monitor
    /// </summary>
    public enum DisplayOrientation
    {
        Landscape = 0,
        Portrait = 1,
        LandscapeFlipped = 2,
        PortraitFlipped = 3
    }

    /// <summary>
    /// Represents a single monitor configuration
    /// </summary>
    public class MonitorConfig
    {
        /// <summary>
        /// Gets or sets the device name (usually in the form \\.\DISPLAY1)
        /// </summary>
        public string DeviceName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the unique identifier for the monitor
        /// </summary>
        public string ID { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether this monitor is primary
        /// </summary>
        public bool IsPrimary { get; set; }

        /// <summary>
        /// Gets or sets the width of the monitor resolution in pixels
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the monitor resolution in pixels
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the orientation of the monitor
        /// </summary>
        public DisplayOrientation Orientation { get; set; }

        /// <summary>
        /// Gets or sets the X coordinate of the monitor position
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Gets or sets the Y coordinate of the monitor position
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Creates a string representation of the monitor configuration
        /// </summary>
        public override string ToString()
        {
            return $"{DeviceName} ({Width}x{Height}) at ({X},{Y}) - {(IsPrimary ? "Primary" : "Secondary")}";
        }
    }

    /// <summary>
    /// Represents a collection of monitor configurations
    /// </summary>
    public class MonitorConfigCollection
    {
        /// <summary>
        /// Gets or sets the list of monitor configurations
        /// </summary>
        public List<MonitorConfig> Monitors { get; set; } = new List<MonitorConfig>();

        /// <summary>
        /// Serializes the monitor configuration collection to JSON
        /// </summary>
        public string ToJson()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            return JsonSerializer.Serialize(this, options);
        }

        /// <summary>
        /// Saves the monitor configuration collection to a JSON file
        /// </summary>
        public void SaveToFile(string filePath)
        {
            File.WriteAllText(filePath, ToJson());
        }

        /// <summary>
        /// Deserializes monitor configuration collection from JSON
        /// </summary>
        public static MonitorConfigCollection FromJson(string json)
        {
            return JsonSerializer.Deserialize<MonitorConfigCollection>(json) ?? new MonitorConfigCollection();
        }

        /// <summary>
        /// Loads monitor configuration collection from a JSON file
        /// </summary>
        public static MonitorConfigCollection LoadFromFile(string filePath)
        {
            string json = File.ReadAllText(filePath);
            return FromJson(json);
        }
    }
}