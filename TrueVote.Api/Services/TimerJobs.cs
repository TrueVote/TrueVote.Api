using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace TrueVote.Api.Services
{
    [ExcludeFromCodeCoverage]
    public class TimerJobs : BackgroundService
    {
        private readonly ILogger _log;
        private readonly IServiceProvider _serviceProvider;
        // TODO AD-128 - Make these intervals environment settings
        private const int BALLOT_HASHING_INTERVAL_MINUTES = 240;
        private const int OPENTIMESTAMPS_STAMPING_INTERVAL_MINUTES = 480;
        private const int STUCK_MESSAGES_CHECK_INTERVAL_MINUTES = 120;
        private const int STUCK_MESSAGES_THRESHOLD_MINUTES = STUCK_MESSAGES_CHECK_INTERVAL_MINUTES * 2; // Double the check interval

        public TimerJobs(ILogger log, IServiceProvider serviceProvider)
        {
            _log = log;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var ballotHashingTask = RunPeriodically(ProcessPendingBallots,
                TimeSpan.FromMinutes(BALLOT_HASHING_INTERVAL_MINUTES), stoppingToken);

            var otherProcessTask = RunPeriodically(ProcessPendingOpentimestamps,
                TimeSpan.FromMinutes(OPENTIMESTAMPS_STAMPING_INTERVAL_MINUTES), stoppingToken);

            var stuckMessagesTask = RunPeriodically(CheckStuckMessages,
                TimeSpan.FromMinutes(STUCK_MESSAGES_CHECK_INTERVAL_MINUTES), stoppingToken);


            // Wait for all tasks to complete
            await Task.WhenAll(ballotHashingTask, otherProcessTask);
        }

        private async Task RunPeriodically(Func<CancellationToken, Task> process, TimeSpan interval, CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await process(stoppingToken);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, $"An error occurred during {process.Method.Name}");
                }
                await Task.Delay(interval, stoppingToken);
            }
        }

        private async Task CheckStuckMessages(CancellationToken cancellationToken)
        {
            try
            {
                _log.LogDebug("Begin: CheckStuckMessages");

                using var scope = _serviceProvider.CreateScope();
                var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TrueVoteDbContext>>();
                using var trueVoteDbContext = factory.CreateDbContext();

                var stuckMessages = await trueVoteDbContext.CommunicationEvents.Where(c => c.Status == "Processing").ToListAsync(cancellationToken);

                // Do the datetime filtering in memory
                var messagesToUpdate = stuckMessages.Where(c => (DateTime.UtcNow - c.DateUpdated).TotalMinutes > STUCK_MESSAGES_THRESHOLD_MINUTES).ToList();
                if (!messagesToUpdate.Any())
                {
                    _log.LogDebug("No stuck messages found");
                    return;
                }

                var tasks = new List<Task>();
                foreach (var message in messagesToUpdate)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            _log.LogWarning("Found stuck message: {CommunicationEventId}, stuck since {DateUpdated}", message.CommunicationEventId, message.DateUpdated);

                            message.Status = "Failed";
                            message.ErrorMessage = "Message stuck in Processing state";
                            message.DateUpdated = DateTime.UtcNow;
                            message.DateProcessed = DateTime.UtcNow;

                            await trueVoteDbContext.SaveChangesAsync(cancellationToken);

                            _log.LogInformation("Updated stuck message {CommunicationEventId} to Failed status", message.CommunicationEventId);
                        }
                        catch (Exception ex)
                        {
                            _log.LogError(ex, "Error updating stuck message {CommunicationEventId}", message.CommunicationEventId);
                        }
                    }, cancellationToken));
                }

                await Task.WhenAll(tasks);
                _log.LogDebug("End: CheckStuckMessages");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error checking for stuck messages");
            }
        }

        private async Task ProcessPendingBallots(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var ballot = scope.ServiceProvider.GetRequiredService<Ballot>();
                var ballotsWithoutHashes = await ballot.GetBallotsWithoutHashesAsync(cancellationToken);
                if (ballotsWithoutHashes == null || ballotsWithoutHashes.Count == 0)
                {
                    _log.LogDebug("End: ProcessPendingBallots");

                    return;
                }

                var tasks = new List<Task>();
                var hasher = scope.ServiceProvider.GetRequiredService<IHasher>();
                var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TrueVoteDbContext>>();

                foreach (var b in ballotsWithoutHashes)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            _log.LogDebug("Hashing Ballot: {ballotId}", b.BallotId);

                            using var trueVoteDbContext = factory.CreateDbContext();
                            await hasher.HashBallotAsync(trueVoteDbContext, b);
                        }
                        catch (Exception ex)
                        {
                            _log.LogError(ex, "Error hashing ballot {ballotId}", b.BallotId);
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

#pragma warning disable IDE0060 // Remove unused parameter
        private Task ProcessPendingOpentimestamps(CancellationToken cancellationToken)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            // TODO AD-128 Fill in body
            _log.LogDebug("End: ProcessPendingOpentimestamps");

            return Task.CompletedTask;
        }
    }
}
