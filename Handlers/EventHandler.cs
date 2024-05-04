namespace App.Handlers;

using App.Models;
using System.Management.Automation;

public class EventHandler
{
    public void HandlePatch(Patch patch)
    {
        string script = string.Empty;

        switch (patch.Name)
        {
            case "reports-service":
                var path = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "Scripts",
                    "update-reports-service.ps1"
                );

                script = File.ReadAllText(path);
                break;
            default:
                break;
        }

        var result = PowerShell.Create().AddScript(script)
            .AddParameter("arg1", patch.Path)
            .Invoke();

        Console.WriteLine("Result: " + result);
    }
}