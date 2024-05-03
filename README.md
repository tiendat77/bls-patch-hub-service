# PatchHub Service

PatchHub Service is a Windows service that listens for deploy commands from developers and automatically downloads and installs new software patches without any human effort.

## Installation

To install the PatchHub Service, follow these steps:

1. Download the latest build from [releases](https://github.com/tiendat77/patch-hub-service/releases/latest).
2. Extract contents of the latest build to  `C:\Program Files\PatchHubService` (Make sure binary location has the Write permissions to just to SYSTEM, Administrator groups. Authenticated users should and only have Read and Execute.)
3. In an elevated Powershell console, run the following:

```
powershell.exe -ExecutionPolicy Bypass -File install.ps1
```

4. Start the PatchHub Service by running the following command in an elevated Powershell console:

```
Start-Service PatchHubService
```

5. Setup `PatchHub Service` to auto-start

```
Set-Service PatchHubService -StartupType Automatic
```

## Usage

To use the PatchHub Service, follow these steps:

1. Upload the software patch to the designated location.
2. Send a deploy command to the PatchHub Service.
3. The service will automatically download and install the patch.

## Contributing

Contributions are welcome! If you find any issues or have suggestions for improvements, please open an issue or submit a pull request.

## License

This project is licensed under the [MIT License](LICENSE).