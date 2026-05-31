public static class ConsoleUtils
{
    public static bool AskUserYNQuestion(string prompt)
    {
        while (true)
        {
            Console.Write(prompt + " [Y/N]: ");
            char response = (char)Console.Read();
            if (response == 'Y' || response == 'N' || response == 'y' || response == 'n')
            {
                return (response == 'Y' || response == 'y');
            }
        }
    }

    public static string AskUserLineResponseQuestion(string prompt)
    {
        string? response = null;
        while (String.IsNullOrWhiteSpace(response))
        {
            Console.Write(prompt + ": ");

            response = Console.ReadLine();
        }
        //Logging.LogDebug($"Response:{response};");
        return response!;
    }
}
