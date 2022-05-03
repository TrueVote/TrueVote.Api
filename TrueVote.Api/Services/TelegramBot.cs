using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using Newtonsoft.Json;
using TrueVote.Api.Models;
using System.Text;

namespace TrueVote.Api.Services
{
    [ExcludeFromCodeCoverage] // TODO Write tests. This requires mocking the Telegram API
    public class TelegramBot
    {
        private static TelegramBotClient botClient = null; // To connect to bot: http://t.me/TrueVoteAPI_bot
        private static readonly string TelegramRuntimeChannel = "@TrueVote_Api_Runtime_Channel";  // To connect to channel: https://t.me/TrueVote_Api_Runtime_Channel
        private static readonly string HelpText = "📖 TrueVote API Bot enables you execute some commands on the API. Simply use / in this chat to see a list of commands. To view broadcast messages, be sure and join the TrueVote API Runtime Channel: https://t.me/TrueVote_Api_Runtime_Channel";

        // TODO Self referrential. Needs context instead of just pointing to production.
        // Would be better to pull this from the environment. e.g. For local it would be https://localhost:7071/api
        private static readonly string BaseApiUrl = "https://truevote-api.azurewebsites.net/api";

        public static async void Init()
        {
            if (botClient != null)
                return;

            using var cts = new CancellationTokenSource();

            // List of BotCommands
            var commands = new List<BotCommand>() {
                new BotCommand { Command = "help", Description = "📖 View summary of what the bot can do" },
                new BotCommand { Command = "status", Description = "🖥 View the API status" },
                new BotCommand { Command = "version", Description = "🤖 View the API version" }
            };

            // Get the Bot key
            var botKey = Environment.GetEnvironmentVariable("TelegramBotKey");
            if (string.IsNullOrEmpty(botKey))
            {
                Console.WriteLine("Error retreiving Telegram BotKey");
                return;
            }

            try
            {
                botClient = new TelegramBotClient(botKey);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error creating Telegram BotClient: {e.Message}");
                return;
            }

            var testKey = await botClient.TestApiAsync();
            if (!testKey)
            {
                Console.WriteLine($"Error with Telegram Api Key - Failure to connect");
                return;
            }

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { } // receive all update types
            };

            try
            {
                // Inject the bot with these command options
                var commandStatus = botClient.SetMyCommandsAsync(commands, null, null, cts.Token);

                botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken: cts.Token);

                var me = await botClient.GetMeAsync();

                Console.WriteLine($"Start listening for @{me.Username}");

                // This keeps it running
                new ManualResetEvent(false).WaitOne();

                // Send cancellation request to stop bot
                cts.Cancel();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error handling Telegram Bot Message: {e.Message}");
                return;
            }
        }

        private async static Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Only process Message updates: https://core.telegram.org/bots/api#message
            if (update.Type != UpdateType.Message)
                return;

            // Only process text messages
            if (update.Message!.Type != MessageType.Text)
                return;

            var chatId = update.Message.Chat.Id;

            var messageText = update.Message.Text;

            Console.WriteLine($"Type: {update.Type} Received: '{messageText}' message in bot {chatId}.");

            var messageResponse = string.Empty;

            var command = messageText.ToLower().Split(' ').First();

            switch (command)
            {
                case "/help":
                    {
                        messageResponse = HelpText;
                        break;
                    }

                case "/status":
                    {
                        var status = await GetStatus();
                        messageResponse = $"<code>{status}</code>";
                        break;
                    }

                case "/version":
                    {
                        messageResponse = await GetVersion();
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

            // Post command to global group channel
            await SendChannelMessage($"Bot received command: {command} from user: {update.Message.Chat.Username}");
        }

        private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine($"HandleErrorAsync() Error: {ErrorMessage}");

            return Task.CompletedTask;
        }

        private async static Task<Message> SendMessage(ChatId chatId, string text, CancellationToken cancellationToken)
        {
            try
            {
                return await botClient.SendTextMessageAsync(chatId, text, ParseMode.Html, null, null, null, null, null, null, cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error sending Telegram Bot Message: {e.Message}");
                return null;
            }
        }

        public async static Task<Message> SendChannelMessage(string text)
        {
            try
            {
                return await botClient.SendTextMessageAsync(TelegramRuntimeChannel, text);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error sending Telegram Channel Message: {e.Message}");
                return null;
            }
        }

        private static async Task<string> GetStatus()
        {
            var client = new HttpClient();

            var ret = await client.GetAsync($"{BaseApiUrl}/status");

            var result = await ret.Content.ReadAsAsync<StatusModel>();

            // Convert it back to string
            var sresult = JsonConvert.SerializeObject(result, Formatting.Indented);

            return sresult;
        }

        // TODO Need to really get version from assembly info. Better than Git tag
        private static async Task<string> GetVersion()
        {
            var client = new HttpClient();

            var ret = await client.GetAsync($"{BaseApiUrl}/status");

            var result = await ret.Content.ReadAsAsync<StatusModel>();

            return result.BuildInfo.LastTag;
        }
    }
}
