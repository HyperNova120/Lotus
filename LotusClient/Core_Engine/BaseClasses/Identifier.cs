using System.Text;
using System.Text.RegularExpressions;
using Core_Engine.BaseClasses.Types;

namespace Core_Engine.BaseClasses;

public class Identifier
{
    private static Regex _NamespaceRegex = new Regex("^[a-z0-9.-_]+$");
    private static Regex _ValueRegex = new Regex("^[a-z0-9.-_/]+$");

    public bool _Valid = true;

    public string? IdentifierString;

    public Identifier() { }

    public Identifier(string IdentifierString)
    {
        ReadFromIdentifierString(IdentifierString);
    }

    public int GetFromBytes(byte[] bytes)
    {
        (string value, int numBytes) = StringN.DecodeBytes(bytes);
        ReadFromIdentifierString(value);
        return numBytes;
    }

    public override bool Equals(object? obj)
    {
        if (obj is Identifier other)
        {
            return (_Valid && other._Valid) && (IdentifierString == other.IdentifierString);
        }
        return false;
    }

    public void ReadFromIdentifierString(string IdentifierString)
    {
        this.IdentifierString = IdentifierString.Replace("\r", "").Replace("\n", "");
        string[] parts = IdentifierString.Split(":");
        if (
            parts.Length != 2
            || !_NamespaceRegex.Match(parts[0]).Success
            || !_ValueRegex.Match(parts[1]).Success
        )
        {
            _Valid = false;
            /* Logging.LogDebug(
                $"ReadFromIdentifierString FAIL;  IdentifierString:{IdentifierString}"
            ); */
        }
        //this.IdentifierString = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(IdentifierString));
    }

    public string GetString()
    {
        //Logging.LogDebug($"GetString: {IdentifierString ?? "NULL"}");
        return IdentifierString ?? "NULL";
    }
}
