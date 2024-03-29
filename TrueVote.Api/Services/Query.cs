using Microsoft.EntityFrameworkCore;
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
            var items = await _trueVoteDbContext.Elections.OrderByDescending(c => c.DateCreated).ToListAsync();

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
    }
}
