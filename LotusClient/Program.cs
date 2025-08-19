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

            NBT Test1 = new NBT(true)
                .WriteTag("TestInt", 1234)
                .WriteTag("TestString", "hello world");

            NBT Test2 = new NBT(true).WriteTag("TestFloat", 12.34).WriteTag("TestDouble", 5728d);

            /* Console.WriteLine(Test1.GetNBTAsString());
            Console.WriteLine(Test2.GetNBTAsString() + "\n\n");
            byte[] test1Bytes = Test1.GetBytes();
            byte[] test2Bytes = Test2.GetBytes();
            byte[] testCombine = [.. test1Bytes, .. test2Bytes];
            NBT TestCombine1 = new NBT();
            NBT TestCombine2 = new NBT();
            int offset = TestCombine1.ReadFromBytes(testCombine, true);
            Console.WriteLine(TestCombine1.GetNBTAsString());
            Console.WriteLine($"TEST1 SIZE:{test1Bytes.Length}; READ BACK SIZE:{offset}");

            TestCombine2.ReadFromBytes(testCombine[offset..], true);
            Console.WriteLine(TestCombine2.GetNBTAsString()); */

            /* NBT Test1 = new NBT(true).WriteTag("TestInt", 1234).WriteTag("TestString", "hello world");


            Console.WriteLine(Test1.GetNBTAsString());
            Test1.ReadFromBytes(Test1.GetBytes(), true);
            Console.WriteLine(Test1.GetNBTAsString());
            Test1.ReadFromBytes(Test1.GetBytes(), true);
            Console.WriteLine(Test1.GetNBTAsString()); */

            Core_Engine.Core_Engine.InitCore();
            await Core_Engine.Core_Engine.GoInteractiveMode();
        }
    }
}
