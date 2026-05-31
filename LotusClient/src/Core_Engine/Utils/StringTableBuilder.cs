namespace LotusCore.Utils;

public class StringTableBuilder
{
    private static string BuildTableRow(int columnWidth, string[] values)
    {
        string returner = "";
        foreach (string val in values)
        {
            returner += val.PadRight(columnWidth);
        }
        return returner;
    }

    public static string CreateTable(int columnWidth, string[] headers, string[][] values)
    {
        string returner = BuildTableRow(columnWidth, headers) + "\n\n";
        foreach (string[] curVal in values)
        {
            returner += BuildTableRow(columnWidth, curVal) + "\n";
        }
        return returner;
    }
}
