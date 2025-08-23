using System.Text;
using System.Text.RegularExpressions;
using LotusCore.BaseClasses.Types;

namespace LotusCore.BaseClasses;

public class Identifier
{
    private static Regex _NamespaceRegex = new Regex("^[a-z0-9.-_]+$");
    private static Regex _ValueRegex = new Regex("^[a-z0-9.-_/]+$");

    public bool _Valid = true;

    public string? IdentifierString;

    public Identifier() { }

    public Identifier(Identifier other)
    {
        _Valid = other._Valid;
        IdentifierString = other.IdentifierString;
    }

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

    public static bool operator ==(Identifier a, Identifier b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }
        if (a is null || b is null)
        {
            return false;
        }
        return a.IdentifierString == b.IdentifierString;
    }

    public static bool operator !=(Identifier a, Identifier b) => !(a == b);

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
