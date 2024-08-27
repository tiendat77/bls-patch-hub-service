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
        switch (patch.Software)
        {
            case "reports-service":
                using (var pws = PowerShell.Create()) {
                    try
                    {
                        var path = Path.Combine(
                            _configuration["Location"],
                            "Scripts",
                            "update-reports-service.ps1"
                        );

                        string command = File.ReadAllText(path);

                        pws.AddScript(command);
                        pws.AddParameter("url", patch.Path);

                        var result = await pws.InvokeAsync();

                        if (result != null) {
                            var error = result.FirstOrDefault();

                            if (error != null) {
                                _logger.LogError(error.ToString());
                                return new ErrorResponse(false, error.ToString());
                            }
                        }

                        return new SuccessResponse(true, "Patch applied successfully");
                    }
                    catch (System.Exception ex)
                    {
                        _logger.LogError(ex, message: "Failed to apply patch");
                        return new ErrorResponse(false, ex.ToString());
                    }
                }

            default:
                return new ErrorResponse(false, "Invalid patch name");
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