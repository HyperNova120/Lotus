using Core_Engine;
using Core_Engine.Utils;
using Core_Engine.Utils.NBTInternals.Tags;
using Microsoft.Extensions.Configuration;

namespace LotusClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Logging.mut = new Mutex();

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            Environment.SetEnvironmentVariable("AppID", (string)configuration!["AzureApp:AppID"]!);

            /* NBT testEdit = new NBT("Level")
                .WriteTag(
                    new NBT("nested compound test")
                        .WriteTag(
                            new NBT("Egg").WriteTag("name", "Eggbert").WriteTag("value", 0.5f)
                        )
                        .WriteTag(
                            new NBT("Ham").WriteTag("name", "Hampus").WriteTag("value", 0.75f)
                        )
                )
                .WriteTag("intTest", 2147483647)
                .WriteTag("byteTest", (byte)127)
                .WriteTag("stringTest", "HELLO WORLD THIS IS A TEST STRING!")
                .WriteListTag("listTest (long)", (long[])[11, 12, 13, 14, 15])
                .WriteTag("doubleTest", 0.49312871321823148d)
                .WriteTag("floatTest", 0.49823147058486938f)
                .WriteTag("longTest", (long)9223372036854775807)
                .WriteListTag(
                    "listTest (compound)",
                    [
                        new NBT()
                            .WriteTag("created-on", 1264099775885L)
                            .WriteTag("name", "Compound Tag #0"),
                        new NBT()
                            .WriteTag("created-on", 1264099775885L)
                            .WriteTag("name", "Compound Tag #1"),
                    ]
                )
                .WriteTag(
                    "byteArrayTest (the first 1000 values of (n*n*255+n*7)%100, starting with n=0 (0, 62, 34, 16, 8, ...))",
                    (byte[])[0x00, 0x01, 0x02, 0x03]
                )
                .WriteTag("shortTest", (short)32767);

            Console.WriteLine(testEdit.GetNBTAsString());

            Console.WriteLine(
                "TryGetTag Test: "
                    + ((testEdit.TryGetTag<TAG_Compound>("Egg") == null) ? "FAILED" : "PASS")
            );

            testEdit.TryGetTag<TAG_String>("stringTest")!.Value += " Modified";
            testEdit.TryGetTag<TAG_Long>("longTest")!.Value = 12;
            Console.WriteLine(testEdit.GetNBTAsString()); */

            Core_Engine.Core_Engine.InitCore();
            await Core_Engine.Core_Engine.GoInteractiveMode();
        }
    }
}
