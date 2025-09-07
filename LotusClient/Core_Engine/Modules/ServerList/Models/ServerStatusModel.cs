namespace LotusCore.Modules.ServerList.Models;

public class ServerStatusModel
{
    public ServerStatusModelVersion version { get; set; }
    public ServerStatusModelPlayers players { get; set; }
    public ServerStatusModelDescription description { get; set; }
    public string favicon { get; set; }
    public bool enforcesSecureChat { get; set; }
}

public class ServerStatusModelExtra
{
    public string color { get; set; }
    public ServerStatusModelExtra extra { get; set; }
    public string text { get; set; }
}

public class ServerStatusModelVersion
{
    public string name { get; set; }
    public int protocol { get; set; }
}

public class ServerStatusModelPlayers
{
    public int max { get; set; }
    public int online { get; set; }
    public ServerStatusModelPlayerSamples[] sample { get; set; }
}

public class ServerStatusModelPlayerSamples
{
    public string name { get; set; }
    public string id { get; set; }
}

public class ServerStatusModelDescription
{
    public List<TextComponent> extra { get; set; }
    public string text { get; set; }
}

public class TextComponent
{
    public string text { get; set; }

    // Optional formatting
    public string color { get; set; }
    public bool? bold { get; set; }
    public bool? underlined { get; set; }
    public bool? strikethrough { get; set; }

    // Recursive nesting
    public List<TextComponent> extra { get; set; }
}
