using System.Text.RegularExpressions;

namespace Core_Engine.BaseClasses;

struct Identifier
{
    private static Regex _NamespaceRegex = new Regex("^[a-z0-9.-_]$");
    private static Regex _ValueRegex = new Regex("^[a-z0-9.-_/]$");

    public bool _Valid = true;

    public string? _Namespace,
        _Value;

    public Identifier(string IdentifierString)
    {
        string[] parts = IdentifierString.Split(":");
        if (
            parts.Length != 2
            || !_NamespaceRegex.Match(parts[0]).Success
            || !_ValueRegex.Match(parts[1]).Success
        )
        {
            _Valid = false;
            return;
        }
        _Namespace = parts[0];
        _Value = parts[1];
    }
}
