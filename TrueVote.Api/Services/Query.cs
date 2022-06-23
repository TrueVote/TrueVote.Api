using System.Diagnostics.CodeAnalysis;
using TrueVote.Api.Models;

namespace TrueVote.Api.Services
{
    [ExcludeFromCodeCoverage]
    public class Query
    {
        public CandidateModel GetCandidates()
        {
            // TODO Return actual query
            return new CandidateModel { CandidateId = "1", DateCreated = System.DateTime.Now, Name = "John Smith", PartyAffiliation = "Independant" };
        }

        // TODO Add Get() commands for all Services
    }
}
