using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TrueVote.Api.Models;

namespace TrueVote.Api.Interfaces
{
    public interface ITrueVoteDbContext
    {
        DbSet<UserModel> Users { get; set; }
        DbSet<ElectionModel> Elections { get; set; }
        DbSet<RaceModel> Races { get; set; }
        DbSet<CandidateModel> Candidates { get; set; }
        DbSet<BallotModel> Ballots { get; set; }
        DbSet<TimestampModel> Timestamps { get; set; }
        DbSet<BallotHashModel> BallotHashes { get; set; }

        Task<bool> EnsureCreatedAsync();
        Task<int> SaveChangesAsync();
    }
}
