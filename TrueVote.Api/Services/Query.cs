using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using TrueVote.Api.Interfaces;
using TrueVote.Api.Models;

namespace TrueVote.Api.Services
{
    // TODO Add parameter support (filtering) to GraphQL queries so you can do operations such as:
    // { candidate(partyAffiliation: "Republican") { candidateId, name, partyAffiliation }
    // TODO Add sorting support to replace the 'OrderBy' directives below
    public class Query
    {
        private readonly ITrueVoteDbContext _trueVoteDbContext;

        public Query(ITrueVoteDbContext trueVoteDbContext)
        {
            _trueVoteDbContext = trueVoteDbContext;
        }

        public async Task<IReadOnlyList<CandidateModel>> GetCandidate()
        {
            var items = await _trueVoteDbContext.Candidates.OrderByDescending(c => c.DateCreated).ToListAsync();

            return items;
        }

        public async Task<IReadOnlyList<CandidateModel>> GetCandidateByPartyAffiliation([GraphQLName("PartyAffiliation")] string PartyAffiliation)
        {
            var items = await _trueVoteDbContext.Candidates.Where(c => c.PartyAffiliation == PartyAffiliation).OrderByDescending(c => c.DateCreated).ToListAsync();

            return items;
        }

        public async Task<IReadOnlyList<ElectionModel>> GetElection()
        {
            var items = await _trueVoteDbContext.Elections.Where(e => e.Unlisted == false).OrderByDescending(c => c.DateCreated).ToListAsync();

            return items;
        }

        public async Task<IReadOnlyList<ElectionModel>> GetElectionById([GraphQLName("ElectionId")] string ElectionId)
        {
            var items = await _trueVoteDbContext.Elections.Where(e => e.ElectionId == ElectionId).OrderByDescending(c => c.DateCreated).ToListAsync();

            return items;
        }

        public async Task<IReadOnlyList<RaceModel>> GetRace()
        {
            var items = await _trueVoteDbContext.Races.OrderByDescending(c => c.DateCreated).ToListAsync();

            return items;
        }

        public async Task<IReadOnlyList<UserModel>> GetUser()
        {
            var items = await _trueVoteDbContext.Users.OrderByDescending(c => c.DateCreated).ToListAsync();

            return items;
        }

        public async Task<BallotList> GetBallot()
        {
            var items = new BallotList
            {
                Ballots = await _trueVoteDbContext.Ballots.OrderByDescending(e => e.DateCreated).ToListAsync(),
                BallotHashes = await _trueVoteDbContext.BallotHashes.OrderByDescending(e => e.DateCreated).ToListAsync()
            };

            return items;
        }

        public async Task<BallotList> GetBallotById([GraphQLName("BallotId")] string BallotId)
        {
            var items = new BallotList
            {
                Ballots = await _trueVoteDbContext.Ballots.Where(e => e.BallotId == BallotId).OrderByDescending(c => c.DateCreated).ToListAsync(),
                BallotHashes = await _trueVoteDbContext.BallotHashes.Where(e => e.BallotId == BallotId).OrderByDescending(e => e.DateCreated).ToListAsync()
            };

            return items;
        }

        public async Task<IReadOnlyList<AccessCodeModel>> GetElectionAccessCodesByElectionId([GraphQLName("ElectionId")] string ElectionId)
        {
            var items = await _trueVoteDbContext.ElectionAccessCodes.Where(e => e.ElectionId == ElectionId).OrderByDescending(c => c.DateCreated).ToListAsync();

            return items;
        }

        public async Task<IReadOnlyList<AccessCodeModel>> GetElectionAccessCodesByAccessCode([GraphQLName("AccessCode")] string AccessCode)
        {
            var items = await _trueVoteDbContext.ElectionAccessCodes.Where(e => e.AccessCode == AccessCode).OrderByDescending(c => c.DateCreated).ToListAsync();

            return items;
        }

        public async Task<ElectionResults> GetElectionResultsByElectionId([GraphQLName("ElectionId")] string ElectionId, [GraphQLName("offset")] int offset = 0, [GraphQLName("limit")] int limit = 100)
        {
            var ballots = await _trueVoteDbContext.Ballots.Where(b => b.ElectionId == ElectionId).OrderByDescending(b => b.DateCreated).ToListAsync();
            if (ballots.Count == 0)
            {
                return new ElectionResults
                {
                    ElectionId = ElectionId,
                    TotalBallots = 0,
                    TotalBallotsHashed = 0,
                    Races = [],
                    PaginatedBallotIds = new PaginatedBallotIds { Items = [], Limit = limit, Offset = offset, TotalCount = 0 }
                };
            }

            // Aggregate results across all ballots
            var raceResults = ballots.SelectMany(b => b.Election.Races.Select(r => new { Ballot = b, Race = r }))
                .GroupBy(br => new { br.Race.RaceId, br.Race.Name })
                .Select(g => new RaceResult
                {
                    RaceId = g.Key.RaceId,
                    RaceName = g.Key.Name,
                    CandidateResults = g.SelectMany(br => br.Race.Candidates)
                        .GroupBy(c => new { c.CandidateId, c.Name })
                        .Select(cg => new CandidateResult
                        {
                            CandidateId = cg.Key.CandidateId,
                            CandidateName = cg.Key.Name,
                            TotalVotes = cg.Count(c => c.Selected)
                        })
                        .ToList()
                })
                .ToList();

            // TODO Optimize this. Across large data sets this is a big .Contains (IN clause). Use a batching pattern or reporting server, etc.
            var ballotHashes = await _trueVoteDbContext.BallotHashes.Where(bh => ballots.Select(b => b.BallotId).Contains(bh.BallotId)).ToListAsync();

            // Get paginated ballot IDs
            var paginatedBallotIds = ballots.Skip(offset).Take(limit).Select(b => new BallotIdInfo { BallotId = b.BallotId, DateCreated = b.DateCreated }).ToList();

            return new ElectionResults
            {
                ElectionId = ElectionId,
                TotalBallots = ballots.Count,
                TotalBallotsHashed = ballotHashes.Count,
                Races = raceResults,
                PaginatedBallotIds = new PaginatedBallotIds { Items = paginatedBallotIds, Limit = limit, Offset = offset, TotalCount = ballots.Count }
            };
        }
    }

    [ExcludeFromCodeCoverage]
    public class Subscription
    {
        [Subscribe]
        [Topic("ElectionResultsUpdated.{ElectionId}")]
        public ElectionResults ElectionResultsUpdated([EventMessage] ElectionResults results, [GraphQLName("ElectionId")] string ElectionId,  [GraphQLName("offset")] int offset = 0, [GraphQLName("limit")] int limit = 100)
        {
            if (results.ElectionId != ElectionId)
                return null;

            // Update the results to match the requested pagination
            var paginatedItems = results.PaginatedBallotIds.Items
                .Skip(offset)
                .Take(limit)
                .ToList();

            results.PaginatedBallotIds.Items = paginatedItems;
            results.PaginatedBallotIds.Offset = offset;
            results.PaginatedBallotIds.Limit = limit;

            return results;
        }
    }
}
