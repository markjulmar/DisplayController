# DisplayController

A .NET command line tool for configuring monitors on Windows.

## Features

- View current monitor configuration
- Save monitor configurations to JSON files
- Load and apply saved configurations
- Set a monitor as primary
- Change monitor resolution
- Configure extended or mirrored display modes

## Usage

```
DisplayController [options]
```

### Options

- `-g, --get-config`: Display the current monitor configuration
- `-s, --save-config <filename>`: Save the current configuration to a JSON file
- `-l, --load-config <filename>`: Load and apply a configuration from a JSON file
- `-p, --set-primary <id>`: Set a monitor as primary by its ID
- `--set-resolution <id> <width> <height>`: Set the resolution for a specific monitor
- `-m, --set-mode <extended|mirrored>`: Set the display mode to extended or mirrored

### Examples

```bash
# Display current configuration
DisplayController --get-config

# Save configuration to a file
DisplayController --save-config config.json

# Load and apply a saved configuration
DisplayController --load-config config.json

# Set a monitor as primary
DisplayController --set-primary MONITOR-ID-HERE

# Set resolution for a monitor
DisplayController --set-resolution MONITOR-ID-HERE 1920 1080

# Set display mode to extended
DisplayController --set-mode extended

# Set display mode to mirrored
DisplayController --set-mode mirrored
```

## Requirements

- Windows operating system
- .NET 9.0 or higher

## Building from Source

```bash
# Clone the repository
git clone https://github.com/yourusername/DisplayController.git
cd DisplayController

# Build the project
dotnet build

# Run the application
dotnet run -- --get-config
```

## License

MIT