namespace App.Handlers;

using App.Models;
using System.Management.Automation;

public class EventHandler
{
    public string HandlePatch(Patch patch)
    {
        switch (patch.Software)
        {
            case "reports-service":
                var path = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "Scripts",
                    "update-reports-service.ps1"
                );

                string command = $"Start-Process powershell.exe -Verb RunAs -ArgumentList '-File \"{path}\" {patch.Path}'";

                using (var pws = PowerShell.Create()) {
                    pws.AddScript(command);

                    var result = pws.Invoke().FirstOrDefault()?.ToString();
                    return result ?? "Failed to apply patch";
                }

            default:
                return "Invalid patch name";
        }
    }

    public string HandleCommand(string script)
    {
        using (var pws = PowerShell.Create()) {
            pws.AddScript(script);

            var result = pws.Invoke().FirstOrDefault()?.ToString();
            return result ?? "Failed to execute command";
        }
    }
}