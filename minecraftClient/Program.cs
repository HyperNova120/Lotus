using System;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using MinecraftNetworking;
using MinecraftNetworking.Connection;
using MinecraftNetworking.Packets;
using MinecraftNetworking.StateHandlers;

namespace CsMinecraftClient
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
            if (!await MojangLogin.login(configuration))
            {
                Logging.LogInfo("User not signed in, cannot continue");
                return;
            }

            string serverIp = ConsoleUtils.AskUserLineResponseQuestion("Enter Server IP")!;
            if (!ConnectionHandler.ConnectToServer(serverIp))
            {
                Logging.LogInfo("Unable to connect to server");
                return;
            }

            /* StatusHandler.SendStatusRequest(serverIp);
            await Task.Delay(5000);
            StatusHandler.SendPingRequest();
            await Task.Delay(5000); */
            Logging.LogInfo("Attempting Server Login Start");
            await LoginHandler.LoginToServer(serverIp);

            /* Logging.LogDebug(
                "Sent: " + ConnectionHandler.SendPacket(new HandshakePacket(serverIp, 1)).ToString()
            );
            Logging.LogDebug(
                "Sent: " + ConnectionHandler.SendPacket(new StatusRequestPacket()).ToString()
            ); */

            /* Logging.LogDebug(
                "Sent: " + ConnectionHandler.SendPacket(new StatusPingPacket()).ToString()
            ); */

            /* LoginStartPacket loginStartPacket = new LoginStartPacket(
                Login.UserMinecraftProfile!.name,
                new Guid(Login.UserMinecraftProfile!.id)
            );
            Logging.LogDebug(ConnectionHandler.SendPacket(loginStartPacket).ToString()); */

            await Task.Delay(-1);
        }
    }
}
