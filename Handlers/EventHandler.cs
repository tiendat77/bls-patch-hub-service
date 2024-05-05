namespace App.Handlers;

using App.Models;
using System.Management.Automation;

public class EventHandler
{
    public async Task<string> HandlePatch(Patch patch)
    {
        switch (patch.Software)
        {
            case "reports-service":
                using (var pws = PowerShell.Create()) {
                    try
                    {
                        var path = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "Scripts",
                            "update-reports-service.ps1"
                        );

                        string command = $"Start-Process powershell.exe -Verb RunAs -ArgumentList '-File \"{path}\" {patch.Path}'";

                        pws.AddScript(command);
                        await pws.InvokeAsync();
                        return "Patch applied successfully";
                    }
                    catch (System.Exception)
                    {
                        return "Failed to apply patch";
                    }
                }

            default:
                return "Invalid patch name";
        }
    }

    public async Task<string> HandleCommand(string script)
    {
        using (var pws = PowerShell.Create()) {
            try
            {
                pws.AddScript(script);
                var result = await pws.InvokeAsync();
                return "Command executed successfully";
            }
            catch (System.Exception)
            {
                return "Failed to execute command";
            }
        }
    }
}