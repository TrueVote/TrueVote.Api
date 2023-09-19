using Microsoft.Extensions.Logging;
using System;
using TrueVote.Api.Helpers;
using System.Linq;
using TrueVote.Api.Interfaces;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TrueVote.Api.Models;
using Newtonsoft.Json.Linq;

namespace TrueVote.Api.Services
{
    public interface IValidator
    {
        Task<BallotHashModel> HashBallotAsync(BallotModel ballot);
        Task<TimestampModel> HashBallotsAsync();
        Task StoreTimestampAsync(TimestampModel timestamp);
        Task StoreBallotHashAsync(BallotHashModel ballotHashModel);
    }

    public class Validator : LoggerHelper, IValidator
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

        public async virtual Task<BallotHashModel> HashBallotAsync(BallotModel ballot)
        {
            // Determine if this ballot hash record already exists
            var items = _trueVoteDbContext.BallotHashes.Where(e => e.BallotId == ballot.BallotId).ToList();
            if (items.Any())
            {
                // TODO Localize msg
                var msg = $"Ballot: {ballot.BallotId} has already been hashed. Ballot Hash Id: {items.First().BallotHashId}";

                LogError(msg);
                throw new Exception(msg);
            }

            // Hash this ballot
            var serverBallotHash = MerkleTree.GetHash(ballot);
            var serverBallotHashS = (string) JToken.Parse(Utf8Json.JsonSerializer.ToJsonString(serverBallotHash));

            // Store the BallotHash record in a model
            var ballotHashModel = new BallotHashModel
            {
                ServerBallotHash = serverBallotHash,
                ServerBallotHashS = serverBallotHashS,
                BallotId = ballot.BallotId
            };

            // Store the BallotHash in Database
            await StoreBallotHashAsync(ballotHashModel);
            await _trueVoteDbContext.SaveChangesAsync();

            var ballotHashJson = JsonConvert.SerializeObject(ballotHashModel, Formatting.Indented);
            await _telegramBot.SendChannelMessageAsync($"New Ballot Hash created for Ballot: {ballot.BallotId}. BallotHash: {ballotHashJson}");

            return ballotHashModel;
        }

        public async virtual Task<TimestampModel> HashBallotsAsync()
        {
            // Get all the ballots that don't have a TimestampId
            var items = _trueVoteDbContext.BallotHashes.Where(e => e.TimestampId == null).OrderByDescending(e => e.DateCreated);

            // Generate Merkle root from data list
            var merkleRoot = MerkleTree.CalculateMerkleRoot(items.Select(e => e.ServerBallotHash).ToList());

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
                TimestampHashS = (string) JToken.Parse(Utf8Json.JsonSerializer.ToJsonString(result)),
                TimestampAt = UtcNowProviderFactory.GetProvider().UtcNow
            };
            timestamp.CalendarServerUrl = timestamp.TimestampHashS.ExtractUrl();

            // TODO Do we need to wrap these 2 separate DB operations in a Transaction?
            // https://learn.microsoft.com/en-us/ef/ef6/saving/transactions#what-ef-does-by-default

            // Store the timestamp in Database
            await StoreTimestampAsync(timestamp);

            // Update all the BallotHash models and Database with the new Timestamp
            items.ToList().ForEach(e =>
            {
                e.TimestampId = timestamp.TimestampId;
                e.DateUpdated = UtcNowProviderFactory.GetProvider().UtcNow;
                _trueVoteDbContext.BallotHashes.Update(e);
            });

            await _trueVoteDbContext.SaveChangesAsync();

            var timestampJson = JsonConvert.SerializeObject(timestamp, Formatting.Indented);
            await _telegramBot.SendChannelMessageAsync($"New Ballot Timestamp created for {items.Count()} Ballots. Timestamp: {timestampJson}");

            return timestamp;
        }

        public async virtual Task StoreTimestampAsync(TimestampModel timestamp)
        {
            try
            {
                await _trueVoteDbContext.EnsureCreatedAsync();
                await _trueVoteDbContext.Timestamps.AddAsync(timestamp);
            }
            catch (Exception ex)
            {
                LogError($"Exception storing timestamp: {ex.Message}");
                throw;
            }
        }

        public async virtual Task StoreBallotHashAsync(BallotHashModel ballotHashModel)
        {
            try
            {
                await _trueVoteDbContext.EnsureCreatedAsync();
                await _trueVoteDbContext.BallotHashes.AddAsync(ballotHashModel);
            }
            catch (Exception ex)
            {
                LogError($"Exception storing ballot hash: {ex.Message}");
                throw;
            }
        }
    }
}