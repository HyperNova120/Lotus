namespace LotusCore.Modules.ServerList.Models;

public class ServerStatusModel
{
    public required ServerStatusModelVersion version { get; set; }
    public required ServerStatusModelPlayers players { get; set; }
    public required ServerStatusModelDescription description { get; set; }
    public required string favicon { get; set; }
    public bool enforcesSecureChat { get; set; }
}

public class ServerStatusModelExtra
{
    public required string color { get; set; }
    public required ServerStatusModelExtra extra { get; set; }
    public required string text { get; set; }
}

public class ServerStatusModelVersion
{
    public required string name { get; set; }
    public int protocol { get; set; }
}

public class ServerStatusModelPlayers
{
    public int max { get; set; }
    public int online { get; set; }
    public required ServerStatusModelPlayerSamples[] sample { get; set; }
}

public class ServerStatusModelPlayerSamples
{
    public required string name { get; set; }
    public required string id { get; set; }
}

public class ServerStatusModelDescription
{
    public required List<TextComponent> extra { get; set; }
    public required string text { get; set; }
}

public class TextComponent
{
    public required string text { get; set; }

    // Optional formatting
    public required string color { get; set; }
    public bool? bold { get; set; }
    public bool? underlined { get; set; }
    public bool? strikethrough { get; set; }

    // Recursive nesting
    public required List<TextComponent> extra { get; set; }
}
