using Microsoft.EntityFrameworkCore;
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
        DbSet<FeedbackModel> Feedbacks { get; set; }
        DbSet<AccessCodeModel> ElectionAccessCodes { get; set; }
        DbSet<UsedAccessCodeModel> UsedAccessCodes { get; set; }
        DbSet<ElectionUserBindingModel> ElectionUserBindings { get; set; }
        DbSet<RoleModel> Roles { get; set; }
        DbSet<UserRoleModel> UserRoles { get; set; }
        DbSet<CommunicationEventModel> CommunicationEvents { get; set; }

        Task<bool> EnsureCreatedAsync();
        Task<int> SaveChangesAsync();
    }
}
