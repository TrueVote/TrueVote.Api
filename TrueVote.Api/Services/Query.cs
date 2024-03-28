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

        public async Task<IReadOnlyList<CandidateModel>> GetCandidateByPartyAffiliation([GraphQLName("partyAffiliation")] string partyAffiliation)
        {
            var items = await _trueVoteDbContext.Candidates.Where(c => c.PartyAffiliation == partyAffiliation).OrderByDescending(c => c.DateCreated).ToListAsync();

            return items;
        }

        public async Task<IReadOnlyList<ElectionModel>> GetElection()
        {
            var items = await _trueVoteDbContext.Elections.OrderByDescending(c => c.DateCreated).ToListAsync();

            return items;
        }

        public async Task<IReadOnlyList<ElectionModel>> GetElectionById([GraphQLName("electionId")] string electionId)
        {
            var items = await _trueVoteDbContext.Elections.Where(e => e.ElectionId == electionId).OrderByDescending(c => c.DateCreated).ToListAsync();

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

        public async Task<BallotList> GetBallotById([GraphQLName("ballotId")] string ballotId)
        {
            var items = new BallotList
            {
                Ballots = await _trueVoteDbContext.Ballots.Where(e => e.BallotId == ballotId).OrderByDescending(c => c.DateCreated).ToListAsync(),
                BallotHashes = await _trueVoteDbContext.BallotHashes.Where(e => e.BallotId == ballotId).OrderByDescending(e => e.DateCreated).ToListAsync()
            };

            return items;
        }
    }
}
