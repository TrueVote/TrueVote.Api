using TrueVote.Api.Helpers;
using TrueVote.Api.Interfaces;
using Newtonsoft.Json;
using TrueVote.Api.Models;
using Newtonsoft.Json.Linq;

namespace TrueVote.Api.Services
{
    public interface IHasher
    {
        Task<BallotHashModel> HashBallotAsync(BallotModel ballot);
        Task<TimestampModel> HashBallotsAsync();
        Task StoreTimestampAsync(TimestampModel timestamp);
        Task StoreBallotHashAsync(BallotHashModel ballotHashModel);
    }

    public class Hasher : IHasher
    {
        private readonly ITrueVoteDbContext _trueVoteDbContext;
        private readonly IOpenTimestampsClient _openTimestampsClient;
        private readonly IServiceBus _serviceBus;
        private readonly ILogger _log;

        public Hasher(ILogger log, ITrueVoteDbContext trueVoteDbContext, IOpenTimestampsClient openTimestampsClient, IServiceBus serviceBus)
        {
            _trueVoteDbContext = trueVoteDbContext;
            _openTimestampsClient = openTimestampsClient;
            _serviceBus = serviceBus;
            _log = log;
        }

        public async virtual Task<BallotHashModel> HashBallotAsync(BallotModel ballot)
        {
            // Determine if this ballot hash record already exists
            var items = _trueVoteDbContext.BallotHashes.Where(e => e.BallotId == ballot.BallotId).ToList();
            if (items.Any())
            {
                // TODO Localize msg
                var msg = $"Ballot: {ballot.BallotId} has already been hashed. Ballot Hash Id: {items.First().BallotHashId}";

                _log.LogError(msg);
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
                BallotId = ballot.BallotId,
                DateCreated = UtcNowProviderFactory.GetProvider().UtcNow,
                DateUpdated = UtcNowProviderFactory.GetProvider().UtcNow,
                BallotHashId = Guid.NewGuid().ToString()
            };

            try
            {
                // Store the BallotHash in Database
                await StoreBallotHashAsync(ballotHashModel);
                await _trueVoteDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Error saving ballot hash to database: {ballot.BallotId}");
                throw;
            }

            var ballotHashJson = JsonConvert.SerializeObject(ballotHashModel, Formatting.Indented);

            await _serviceBus.SendAsync($"New Ballot Hash created for Ballot: {ballot.BallotId}. BallotHash: {ballotHashJson}");

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
                _log.LogError($"Exception stamping merkleRoot: {ex.Message}");
                throw;
            }

            // Store the timestamp record in a model
            var timestamp = new TimestampModel
            {
                MerkleRoot = merkleRoot,
                MerkleRootHash = merkleRootHash,
                TimestampId = Guid.NewGuid().ToString(),
                TimestampHash = result,
                TimestampHashS = (string) JToken.Parse(Utf8Json.JsonSerializer.ToJsonString(result)),
                TimestampAt = UtcNowProviderFactory.GetProvider().UtcNow,
                DateCreated = UtcNowProviderFactory.GetProvider().UtcNow,
                CalendarServerUrl = string.Empty
            };
            timestamp.CalendarServerUrl = timestamp.TimestampHashS.ExtractUrl();

            // TODO Do we need to wrap these 2 separate DB operations in a Transaction?
            // https://learn.microsoft.com/en-us/ef/ef6/saving/transactions#what-ef-does-by-default

            try
            {
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
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Error saving or updating timestamp to database: {timestamp.TimestampId}");
                throw;
            }

            var timestampJson = JsonConvert.SerializeObject(timestamp, Formatting.Indented);

            await _serviceBus.SendAsync($"New Ballot Timestamp created for {items.Count()} Ballots. Timestamp: {timestampJson}");

            return timestamp;
        }

        public async virtual Task StoreTimestampAsync(TimestampModel timestamp)
        {
            try
            {
                await _trueVoteDbContext.Timestamps.AddAsync(timestamp);
            }
            catch (Exception ex)
            {
                _log.LogError($"Exception storing timestamp: {ex.Message}");
                throw;
            }
        }

        public async virtual Task StoreBallotHashAsync(BallotHashModel ballotHashModel)
        {
            try
            {
                await _trueVoteDbContext.BallotHashes.AddAsync(ballotHashModel);
            }
            catch (Exception ex)
            {
                _log.LogError($"Exception storing ballot hash: {ex.Message}");
                throw;
            }
        }
    }
}
