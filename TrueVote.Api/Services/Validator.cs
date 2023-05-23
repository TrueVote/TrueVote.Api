using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using TrueVote.Api.Helpers;
using System.Linq;
using TrueVote.Api.Interfaces;
using System.Text;

namespace TrueVote.Api.Services
{
    public class Timestamp
    {
        public byte[] MerkleRoot { get; set; }
        public byte[] MerkleRootHash { get; set; }
        public string TimestampHash { get; set; }
        public DateTime TimestampAt { get; set; }
    }

    public class Validator : LoggerHelper
    {
        private readonly ITrueVoteDbContext _trueVoteDbContext;
        private readonly TelegramBot _telegramBot;

        public Validator(ILogger log, ITrueVoteDbContext trueVoteDbContext, TelegramBot telegramBot) : base(log, telegramBot)
        {
            _trueVoteDbContext = trueVoteDbContext;
            _telegramBot = telegramBot;
        }

        public Timestamp HashBallots()
        {
            // Get all the ballots
            var items = _trueVoteDbContext.Ballots.OrderByDescending(e => e.DateCreated).ToList();

            // Generate Merkle root from data list
            var merkleRoot = MerkleTree.CalculateMerkleRoot(items);

            // Hash the Merkle root
            var merkleRootHash = MerkleTree.GetHash(merkleRoot);

            // Timestamp the Merkle root
            var otsClient = new OpenTimestampsClient(new Uri("https://a.pool.opentimestamps.org"));
            var result = otsClient.Stamp(merkleRootHash).Result;

            // Store the timestamp record in a model
            var timestamp = new Timestamp
            {
                MerkleRoot = merkleRoot,
                MerkleRootHash = merkleRootHash,
                TimestampHash = Encoding.UTF8.GetString(result),
                TimestampAt = DateTime.UtcNow
            };

            // Store the timestamp in Database
            StoreTimestamp(timestamp);

            return timestamp;
        }

        [FunctionName("Validator")]
        public void Run([TimerTrigger("*/1 * * * *")] TimerInfo timerInfo)
        {
            LogInformation($"ValidatorTimer trigger function {timerInfo.Schedule} executed at: {DateTime.Now.ToUniversalTime().ToString("dddd, MMM dd, yyyy HH:mm:ss")}");

            HashBallots();
        }

        private void StoreTimestamp(Timestamp timestamp)
        {
            Console.WriteLine(timestamp.ToString());
        }
    }
}
