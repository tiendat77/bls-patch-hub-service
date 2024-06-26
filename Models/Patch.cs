using System.Text.Json.Serialization;


namespace App.Models;

public class Patch : Base
{
    [JsonPropertyName("name")]
    public string Name {get; set;}

    [JsonPropertyName("description")]
    public string Description {get; set;}

    [JsonPropertyName("version")]
    public string Version {get; set;}

    [JsonPropertyName("path")]
    public string Path {get; set;}

    [JsonPropertyName("env")]
    public string Env {get; set;}

    [JsonPropertyName("author")]
    public string Author {get; set;}

    [JsonPropertyName("software")]
    public string Software {get; set;}
}

[JsonSerializable(typeof(Patch))]
internal partial class PatchContext : JsonSerializerContext {}
