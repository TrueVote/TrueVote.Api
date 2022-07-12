using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrueVote.Api.Interfaces;
using TrueVote.Api.Models;

namespace TrueVote.Api.Tests
{
    internal static class MoqData
    {
        internal static List<UserModel> MockUserData => new()
        {
            new UserModel { Email = "foo@foo.com", DateCreated = DateTime.Now, FirstName = "Foo", UserId = "1" },
            new UserModel { Email = "foo2@bar.com", DateCreated = DateTime.Now.AddSeconds(1), FirstName = "Foo2", UserId = "2" },
            new UserModel { Email = "boo@bar.com", DateCreated = DateTime.Now.AddSeconds(2), FirstName = "Boo", UserId = "3" }
        };

        internal static List<ElectionModel> MockElectionData => new()
        {
            new ElectionModel { Name = "California State", DateCreated = DateTime.Now },
            new ElectionModel { Name = "Los Angeles County", DateCreated = DateTime.Now.AddSeconds(1), StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30) },
            new ElectionModel { Name = "Federal", DateCreated = DateTime.Now.AddSeconds(1), StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30) },
        };

        internal static List<CandidateModel> MockCandidateData => new()
        {
            new CandidateModel { Name = "John Smith", DateCreated = DateTime.Now, PartyAffiliation = "Republican", CandidateId =  "1" },
            new CandidateModel { Name = "Jane Doe", DateCreated = DateTime.Now.AddSeconds(1), PartyAffiliation = "Democrat", CandidateId = "2" }
        };

        internal static List<RaceModel> MockRaceData => new()
        {
            new RaceModel { Name = "President", DateCreated = DateTime.Now, RaceType = RaceTypes.ChooseOne },
            new RaceModel { Name = "Judge", DateCreated = DateTime.Now.AddSeconds(1), RaceType = RaceTypes.ChooseMany },
            new RaceModel { Name = "Governor", DateCreated = DateTime.Now.AddSeconds(2), RaceType = RaceTypes.ChooseOne }
        };
    }

    public class MoqDataAccessor
    {
        public readonly Mock<MoqTrueVoteDbContext> mockUserContext;
        public readonly Mock<MoqTrueVoteDbContext> mockElectionContext;
        public readonly Mock<MoqTrueVoteDbContext> mockCandidateContext;
        public readonly Mock<MoqTrueVoteDbContext> mockRaceContext;
        public readonly IQueryable<UserModel> mockUserDataQueryable;
        public readonly IQueryable<ElectionModel> mockElectionDataQueryable;
        public readonly IQueryable<CandidateModel> mockCandidateDataQueryable;
        public readonly ICollection<CandidateModel> mockCandidateDataCollection;
        public readonly IQueryable<RaceModel> mockRaceDataQueryable;

        public Mock<DbSet<UserModel>> mockUserSet { get; private set; }
        public Mock<DbSet<RaceModel>> mockRaceSet { get; private set; }
        public Mock<DbSet<CandidateModel>> mockCandidateSet { get; private set; }
        public Mock<DbSet<ElectionModel>> mockElectionSet { get; private set; }

        // https://docs.microsoft.com/en-us/ef/ef6/fundamentals/testing/mocking?redirectedfrom=MSDN
        // https://github.com/romantitov/MockQueryable
        public MoqDataAccessor()
        {
            mockUserContext = new Mock<MoqTrueVoteDbContext>();
            mockUserDataQueryable = MoqData.MockUserData.AsQueryable();
            mockUserSet = DbMoqHelper.GetDbSet(mockUserDataQueryable);
            mockUserContext.Setup(m => m.Users).Returns(mockUserSet.Object);

            mockElectionContext = new Mock<MoqTrueVoteDbContext>();
            mockElectionDataQueryable = MoqData.MockElectionData.AsQueryable();
            mockElectionSet = DbMoqHelper.GetDbSet(mockElectionDataQueryable);
            mockElectionContext.Setup(m => m.Elections).Returns(mockElectionSet.Object);

            mockCandidateContext = new Mock<MoqTrueVoteDbContext>();
            mockCandidateDataQueryable = MoqData.MockCandidateData.AsQueryable();
            mockCandidateDataCollection = MoqData.MockCandidateData;
            mockCandidateSet = DbMoqHelper.GetDbSet(mockCandidateDataQueryable);
            mockCandidateContext.Setup(m => m.Candidates).Returns(mockCandidateSet.Object);

            mockRaceContext = new Mock<MoqTrueVoteDbContext>();
            MoqData.MockRaceData[0].RaceId = "1";
            // TODO Fix this assignment
            MoqData.MockRaceData[0].Candidates = mockCandidateDataCollection;
            MoqData.MockRaceData[1].RaceId = "2";
            MoqData.MockRaceData[2].RaceId = "3";
            mockRaceDataQueryable = MoqData.MockRaceData.AsQueryable();
            mockRaceSet = DbMoqHelper.GetDbSet(mockRaceDataQueryable);
            mockRaceContext.Setup(m => m.Races).Returns(mockRaceSet.Object);
        }
    }

    // By implementing ITrueVoteDbContext, override the properties and set them to use Mocked data
    public class MoqTrueVoteDbContext : DbContext, ITrueVoteDbContext
    {
        public virtual DbSet<UserModel> Users { get; set; }
        public virtual DbSet<ElectionModel> Elections { get; set; }
        public virtual DbSet<RaceModel> Races { get; set; }
        public virtual DbSet<CandidateModel> Candidates { get; set; }

        protected MoqDataAccessor _moqDataAccessor;

        public MoqTrueVoteDbContext()
        {
            _moqDataAccessor = new MoqDataAccessor();

            Users = _moqDataAccessor.mockUserSet.Object;
            Elections = _moqDataAccessor.mockElectionSet.Object;
            Races = _moqDataAccessor.mockRaceSet.Object;
            Candidates = _moqDataAccessor.mockCandidateSet.Object;
        }

        public virtual async Task<bool> EnsureCreatedAsync()
        {
            return await Database.EnsureCreatedAsync();
        }

        public virtual async Task<int> SaveChangesAsync()
        {
            return await base.SaveChangesAsync();
        }
    }
}
