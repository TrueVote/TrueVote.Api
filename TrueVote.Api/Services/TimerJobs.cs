using Microsoft.EntityFrameworkCore;
using TrueVote.Api.Interfaces;
using TrueVote.Api.Models;

namespace TrueVote.Api.Services
{
    public class TimerJobs : BackgroundService
    {
        private readonly ILogger _log;
        private readonly IServiceProvider _serviceProvider;
        private const int BALLOT_HASHING_INTERVAL_MINUTES = 5;

        public TimerJobs(ILogger log, IServiceProvider serviceProvider)
        {
            _log = log;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _log.LogInformation("Timer trigger function ExecuteAsync() executed at: {time} UTC", DateTime.UtcNow);
                    await ProcessPendingBallots(stoppingToken);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "An error occurred during ExecuteAsync");
                }

                await Task.Delay(TimeSpan.FromMinutes(BALLOT_HASHING_INTERVAL_MINUTES), stoppingToken);
            }
        }

        public async Task<List<BallotModel>> GetBallotsWithoutHashesAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var trueVoteDbContext = scope.ServiceProvider.GetRequiredService<ITrueVoteDbContext>();

            try
            {
                var allBallotHashIds = await trueVoteDbContext.BallotHashes.Select(bh => bh.BallotId)
                    .ToListAsync(cancellationToken);

                var ballotHashIdSet = new HashSet<string>(allBallotHashIds);

                var ballotsWithoutHashes = await trueVoteDbContext.Ballots.Where(ballot => !ballotHashIdSet.Contains(ballot.BallotId))
                    .OrderByDescending(e => e.DateCreated)
                    .ToListAsync(cancellationToken);

                _log.LogDebug("Found {count} ballots without hashes", ballotsWithoutHashes.Count);

                return ballotsWithoutHashes;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while fetching ballots without hashes");
                throw;
            }
        }

        private async Task ProcessPendingBallots(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var ballotValidator = scope.ServiceProvider.GetRequiredService<IBallotValidator>();
                var ballotsWithoutHashes = await GetBallotsWithoutHashesAsync(cancellationToken);

                var tasks = new List<Task>();

                foreach (var ballot in ballotsWithoutHashes)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            _log.LogDebug("Hashing Ballot: {ballotId}", ballot.BallotId);

                            await ballotValidator.HashBallotAsync(ballot);
                        }
                        catch (Exception ex)
                        {
                            _log.LogError(ex, "Error hashing ballot {ballotId}", ballot.BallotId);
                        }
                    }, cancellationToken));
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error processing pending ballots");
            }
        }
    }
}
