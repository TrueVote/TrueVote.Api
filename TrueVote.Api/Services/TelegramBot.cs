using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace TrueVote.Api.Services
{
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    public class TelegramBot
    {
        public static string botKey = Environment.GetEnvironmentVariable("TelegramBotKey");
        public static string botChatId = string.Empty;
        public static TelegramBotClient? botClient = null;

        [FunctionName("TelegramBot")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name ??= data?.name;

            var responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        public static async void Init()
        {
            string? botChatId = null;
            using var cts = new CancellationTokenSource();

            // List of BotCommands
            var commands = new List<BotCommand>() {
                new BotCommand { Command = "help", Description = "View summary of what the bot can do" },
                new BotCommand { Command = "status", Description = "View the API status" },
                new BotCommand { Command = "version", Description = "View the API version" }
            };

            // Get the Bot key
            var botKey = TelegramBot.botKey;

            // Try and get a ChatId
            botChatId = TelegramBot.botKey;

            botClient = new TelegramBotClient(botKey);

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { } // receive all update types
            };

            // Inject the bot with these command options
            var commandStatus = botClient.SetMyCommandsAsync(commands, null, null, cts.Token);

            botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken: cts.Token);

            var me = await botClient.GetMeAsync();

            Console.WriteLine($"Start listening for @{me.Username}");

            // If we have this ID, we can send notifications. Send a welcome message.
            if (!string.IsNullOrEmpty(botChatId))
            {
                // var welcomeMessage = await SendMessage(botChatId, $"Connected TrueVote.Api to {me.Username}", cancellationToken: cts.Token);
            }

            // This keeps it running
            new ManualResetEvent(false).WaitOne();

            // Send cancellation request to stop bot
            cts.Cancel();
        }

        async static Task<Message> SendMessage(ChatId chatId, string text, CancellationToken cancellationToken)
        {
            return await botClient.SendTextMessageAsync(chatId, text, cancellationToken: cancellationToken);
        }

        async static Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Only process Message updates: https://core.telegram.org/bots/api#message
            if (update.Type != UpdateType.Message)
                return;

            // Only process text messages
            if (update.Message!.Type != MessageType.Text)
                return;

            var chatId = update.Message.Chat.Id;
            var messageText = update.Message.Text;

            if (string.IsNullOrEmpty(botChatId))
            {
                // Store it in appSettings
                botChatId = chatId.ToString();
                // SetupConfiguration.AddOrUpdateAppSetting("TelegramBotChatId", botChatId);
                // TODO SAVE IT
                Console.WriteLine($"Persisting ChatId: {botChatId} to appsettings.json");
            }

            Console.WriteLine($"Type: {update.Type} Received: '{messageText}' message in bot {chatId}.");

            var messageResponse = string.Empty;

            switch (messageText.ToLower().Split(' ').First())
            {
                case "/help":
                    {
                        messageResponse = "help go here";
                        break;
                    }

                case "/status":
                    {
                        messageResponse = "status go here";
                        break;
                    }

                case "/version":
                    {
                        messageResponse = "version go here";
                        break;
                    }

                default:
                    {
                        break;
                    }
            }

            if (messageResponse.Length > 0)
            {
                var _ = await SendMessage(chatId: chatId, text: messageResponse, cancellationToken: cancellationToken);
                Console.WriteLine($"Sent Message: {messageResponse}");
            }
        }

        static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine($"HandleErrorAsync() Error: {ErrorMessage}");

            return Task.CompletedTask;
        }
    }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
}
