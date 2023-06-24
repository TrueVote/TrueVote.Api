using Microsoft.Extensions.Logging;
using System;
using TrueVote.Api.Helpers;
using System.Linq;
using TrueVote.Api.Interfaces;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TrueVote.Api.Models;

namespace TrueVote.Api.Services
{
    public class Validator : LoggerHelper
    {
        private readonly ITrueVoteDbContext _trueVoteDbContext;
        private readonly TelegramBot _telegramBot;
        private readonly IOpenTimestampsClient _openTimestampsClient;

        public Validator(ILogger log, ITrueVoteDbContext trueVoteDbContext, TelegramBot telegramBot, IOpenTimestampsClient openTimestampsClient) : base(log, telegramBot)
        {
            _trueVoteDbContext = trueVoteDbContext;
            _telegramBot = telegramBot;
            _openTimestampsClient = openTimestampsClient;
        }

        public async Task<TimestampModel> HashBallotsAsync()
        {
            // Get all the ballots
            var items = _trueVoteDbContext.Ballots.OrderByDescending(e => e.DateCreated).ToList();

            // Generate Merkle root from data list
            var merkleRoot = MerkleTree.CalculateMerkleRoot(items);

            // Hash the Merkle root
            var merkleRootHash = MerkleTree.GetHash(merkleRoot);

            // Timestamp the Merkle root
            byte[] result;
            try
            {
                result = await _openTimestampsClient.Stamp(merkleRootHash);
            }
            catch (Exception ex)
            {
                LogError($"Exception stamping merkleRoot: {ex.Message}");
                throw;
            }

            // Store the timestamp record in a model
            var timestamp = new TimestampModel
            {
                MerkleRoot = merkleRoot,
                MerkleRootHash = merkleRootHash,
                TimestampHash = result,
                TimestampHashS = Encoding.UTF8.GetString(result),
                TimestampAt = DateTime.UtcNow
            };
            timestamp.CalendarServerUrl = timestamp.TimestampHashS.ExtractUrl();

            // Store the timestamp in Database
            await StoreTimestampAsync(timestamp);

            var timestampJson = JsonConvert.SerializeObject(timestamp, Formatting.Indented);
            await _telegramBot.SendChannelMessageAsync($"New Ballot Timestamp created. Timestamp: {timestampJson}");

            return timestamp;
        }

        private async Task StoreTimestampAsync(TimestampModel timestamp)
        {
            try
            {
                await _trueVoteDbContext.EnsureCreatedAsync();
                await _trueVoteDbContext.Timestamps.AddAsync(timestamp);
                await _trueVoteDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                LogError($"Exception storing timestamp: {ex.Message}");
                throw;
            }
        }
    }
}
