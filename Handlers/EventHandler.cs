namespace App.Handlers;

using App.Models;
using System.Management.Automation;

public class EventHandler
{

    private readonly ILogger _logger;

    private readonly IConfiguration _configuration;

    public EventHandler(ILogger logger, IConfiguration configuration) {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<ResponseBase> HandlePatch(Patch patch)
    {
        var path = string.Empty;

        switch (patch.Software)
        {
            case "reports-service":
                path = Path.Combine(
                    _configuration["Location"],
                    "Scripts",
                    "update-reports-service.ps1"
                );
                break;

            case "blogic-connector":
                path = Path.Combine(
                    _configuration["Location"],
                    "Scripts",
                    "update-blogic-connector.ps1"
                );
                break;

            case "pos-server":
                path = Path.Combine(
                    _configuration["Location"],
                    "Scripts",
                    "update-pos-server.ps1"
                );
                break;

            case "pos-dashboard":
                path = Path.Combine(
                    _configuration["Location"],
                    "Scripts",
                    "update-pos-dashboard.ps1"
                );
                break;

            case "pos":
                path = Path.Combine(
                    _configuration["Location"],
                    "Scripts",
                    "update-pos.ps1"
                );
                break;

            case "start-kiosk":
                path = Path.Combine(
                    _configuration["Location"],
                    "Scripts",
                    "update-start-kiosk.ps1"
                );
                break;

            case "kiosk":
                path = Path.Combine(
                    _configuration["Location"],
                    "Scripts",
                    "update-kiosk.ps1"
                );
                break;

            default:
             break;
        }

        if (path == string.Empty) {
            _logger.LogError("Invalid patch name");
            return new ErrorResponse(false, "Invalid patch name");
        }

        using (var pws = PowerShell.Create()) {
            try
            {
                var result = string.Empty;

                var output = Path.Combine(
                    _configuration["Location"],
                    "Logs",
                    "powershell-output.txt"
                );

                string command = $" Start-Process powershell.exe -Verb RunAs -PassThru -Wait -ArgumentList \"-NoProfile -ExecutionPolicy Bypass -Command \"\"& '{path}' '{patch.Path}' > '{output}'\"\"\" ";
                pws.AddScript(command);

                var results = await pws.InvokeAsync();

                result = File.ReadAllText(output) ?? string.Empty;
                result = result?.Trim()?.Replace("\r", string.Empty).Replace("\n", " ") ?? string.Empty;

                if (result != string.Empty) {
                    _logger.LogError(result);
                    return new ErrorResponse(false, result);
                }

                _logger.LogInformation($"Installed {patch.Software} Patch");
                return new SuccessResponse(true, "Patch applied successfully");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, message: "Failed to apply patch");
                return new ErrorResponse(false, ex.ToString());
            }
        }
    }

    public async Task<ResponseBase> HandleCommand(string script)
    {
        using (var pws = PowerShell.Create()) {
            try
            {
                pws.AddScript(script);
                var result = await pws.InvokeAsync();
                return new SuccessResponse(true, "Command executed successfully");
            }
            catch (System.Exception)
            {
                return new ErrorResponse(false, "Failed to execute command");
            }
        }
    }
}
