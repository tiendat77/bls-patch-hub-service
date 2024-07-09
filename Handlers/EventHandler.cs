namespace App.Handlers;

using App.Models;
using System.Management.Automation;

public class EventHandler
{
    public async Task<ResponseBase> HandlePatch(Patch patch)
    {
        switch (patch.Software)
        {
            case "reports-service":
                using (var pws = PowerShell.Create()) {
                    try
                    {
                        var path = Path.Combine(
                            "C:/Program Files (x86)/PatchHubService",
                            "Scripts",
                            "update-reports-service.ps1"
                        );

                        string command = $"Start-Process powershell.exe -Verb RunAs -ArgumentList '-File \"{path}\" {patch.Path}'";

                        pws.AddScript(command);
                        await pws.InvokeAsync();
                        return new SuccessResponse(true, "Patch applied successfully");
                    }
                    catch (System.Exception)
                    {
                        return new ErrorResponse(false, "Failed to apply patch", "");
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