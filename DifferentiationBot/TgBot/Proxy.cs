using System;
using System.Net;
using com.LandonKey.SocksWebProxy;
using com.LandonKey.SocksWebProxy.Proxy;
using Telegram.Bot;

namespace TgBot
{
    public static class Proxy
    {
        public static ITelegramBotClient GetBotWithSocksProxy(string apiToken)
        {
            var wp = new SocksWebProxy(
                new ProxyConfig(
                    IPAddress.Parse("127.0.0.1"),
                    GetNextFreePort(),
                    IPAddress.Parse("185.20.184.217"),
                    3693,
                    ProxyConfig.SocksVersion.Five,
                    "userid66n9",
                    "pSnEA7M"),
                false);
            var bc = new TelegramBotClient(apiToken, wp);
            Console.WriteLine("Created BotClient with SOCKS5 Proxy.");
            return bc;
        }

        private static int GetNextFreePort()
        {
            var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint) listener.LocalEndpoint).Port;
            listener.Stop();

            return port;
        }
    }
}