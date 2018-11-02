using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

namespace TgBot
{
    public static class TelegramBot
    {
        private static ITelegramBotClient BotClient { get; set; }

        private const string ApiToken = "666534499:AAFuj7s8Q31cbi_cEbYJAV9IzAzQO2JYKls";

        public static void Main(params string[] args)
        {
            Console.WriteLine("Enable proxy? Y/N");
            var key = Console.ReadKey();
            Console.WriteLine("\n");

            BotClient = key.Key == ConsoleKey.Y
                ? Proxy.GetBotWithSocksProxy(ApiToken)
                : new TelegramBotClient(ApiToken);

            var me = GetBot();

            Console.WriteLine(
                $"Hello, World! I am user {me.Id} and my name is {me.FirstName}."
            );

            BotClient.OnMessage += Bot_OnMessage;
            BotClient.StartReceiving();

            while (true)
                Thread.Sleep(int.MaxValue);
        }

        private static User GetBot() => BotClient.GetMeAsync().Result;

        private static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            var chatId = e.Message.Chat.Id;
            Console.WriteLine($"Received message in {chatId}");
            var result = ProvideResponseFromLibrary(e.Message.Text);
            await BotClient.SendTextMessageAsync(chatId, result);
        }

        private static string ProvideResponseFromLibrary(string inputQueryParameter)
        {
            var url = $"http://localhost:1234/?Expression={inputQueryParameter}";
            var request = WebRequest.Create(url);
            var response = request.GetResponse();
            var buffer = new byte[100];
            response.GetResponseStream().Read(buffer, 0, 100);
            return buffer.Length != 0 ? Encoding.ASCII.GetString(buffer) : string.Empty;
        }
    }
}