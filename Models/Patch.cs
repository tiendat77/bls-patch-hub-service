namespace App.Models;

public class Patch : Base
{
    public string Name {get; set;}
    public string Description {get; set;}
    public string Version {get; set;}
    public string Path {get; set;}
    public string Env {get; set;}
    public string Author {get; set;}
    public string Software {get; set;}
}
