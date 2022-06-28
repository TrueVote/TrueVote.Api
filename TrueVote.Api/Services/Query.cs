using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using TrueVote.Api.Helpers;
using TrueVote.Api.Models;

namespace TrueVote.Api.Services
{
    public class Query : LoggerHelper
    {
        private readonly TrueVoteDbContext _trueVoteDbContext;
        private readonly TelegramBot _telegramBot;

        public Query(ILogger log, TrueVoteDbContext trueVoteDbContext, TelegramBot telegramBot) : base(log, telegramBot)
        {
            _trueVoteDbContext = trueVoteDbContext;
            _telegramBot = telegramBot;
        }

        public CandidateModel GetCandidate()
        {
            var items = _trueVoteDbContext.Candidates.OrderByDescending(c => c.DateCreated).First();

            return items;
        }

        public ElectionModel GetElection()
        {
            return new ElectionModel { ElectionId = "1", DateCreated = System.DateTime.Now, Name = "LA County" };
        }

        // TODO Add Get() commands for all Services
    }
}
