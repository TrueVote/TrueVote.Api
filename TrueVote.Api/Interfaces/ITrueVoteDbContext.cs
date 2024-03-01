using Microsoft.EntityFrameworkCore;
using TrueVote.Api2.Models;

namespace TrueVote.Api2.Interfaces
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
