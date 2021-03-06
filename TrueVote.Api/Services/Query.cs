using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrueVote.Api.Helpers;
using TrueVote.Api.Interfaces;
using TrueVote.Api.Models;

namespace TrueVote.Api.Services
{
    // TODO Add parameter support (filtering) to GraphQL queries so you can do operations such as:
    // { candidate(partyAffiliation: "Republican") { candidateId, name, partyAffiliation }
    // TODO Add sorting support to replace the 'OrderBy' directives below
    public class Query : LoggerHelper
    {
        private readonly ITrueVoteDbContext _trueVoteDbContext;
        private readonly TelegramBot _telegramBot;

        public Query(ILogger log, ITrueVoteDbContext trueVoteDbContext, TelegramBot telegramBot) : base(log, telegramBot)
        {
            _trueVoteDbContext = trueVoteDbContext;
            _telegramBot = telegramBot;
        }

        public async Task<List<CandidateModel>> GetCandidate()
        {
            var items = await _trueVoteDbContext.Candidates.OrderByDescending(c => c.DateCreated).ToListAsync();

            return items;
        }

        public async Task<List<ElectionModel>> GetElection()
        {
            var items = await _trueVoteDbContext.Elections.OrderByDescending(c => c.DateCreated).ToListAsync();

            return items;
        }

        public async Task<List<RaceModel>> GetRace()
        {
            var items = await _trueVoteDbContext.Races.OrderByDescending(c => c.DateCreated).ToListAsync();

            return items;
        }

        public async Task<List<UserModel>> GetUser()
        {
            var items = await _trueVoteDbContext.Users.OrderByDescending(c => c.DateCreated).ToListAsync();

            return items;
        }
    }
}
