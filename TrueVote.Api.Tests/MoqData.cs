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
    public static class MoqData
    {
        public static DateTime startDate = DateTime.Parse("2023-02-25");
        public static DateTime createDate = DateTime.Parse("2022-12-17");

        public static List<UserModel> MockUserData => new()
        {
            new UserModel { Email = "foo@foo.com", DateCreated = createDate, FirstName = "Foo", UserId = "1" },
            new UserModel { Email = "foo2@bar.com", DateCreated = createDate.AddSeconds(1), FirstName = "Foo2", UserId = "2" },
            new UserModel { Email = "boo@bar.com", DateCreated = createDate.AddSeconds(2), FirstName = "Boo", UserId = "3" }
        };

        public static List<ElectionModel> MockElectionData => new()
        {
            new ElectionModel { Name = "California State", DateCreated = createDate, StartDate = startDate, EndDate = startDate.AddDays(30) },
            new ElectionModel { Name = "Los Angeles County", DateCreated = createDate.AddSeconds(1), StartDate = startDate, EndDate = startDate.AddDays(30) },
            new ElectionModel { Name = "Federal", DateCreated = createDate.AddSeconds(1), StartDate = startDate, EndDate = startDate.AddDays(30), ElectionId = "68" },
        };

        public static List<BallotModel> MockBallotData => new()
        {
            new BallotModel { BallotId = "ballotid1", ElectionId = "68", Election = MockElectionData[0] },
            new BallotModel { BallotId = "ballotid2", ElectionId = "68", Election = MockElectionData[1] },
            new BallotModel { BallotId = "ballotid3", ElectionId = "68", Election = MockElectionData[2] },
        };

        public static List<CandidateModel> MockCandidateData => new()
        {
            new CandidateModel { Name = "John Smith", DateCreated = createDate, PartyAffiliation = "Republican", CandidateId =  "1" },
            new CandidateModel { Name = "Jane Doe", DateCreated = createDate.AddSeconds(1), PartyAffiliation = "Democrat", CandidateId = "2" }
        };

        public static List<RaceModel> MockRaceData => new()
        {
            new RaceModel { Name = "President", DateCreated = createDate, RaceType = RaceTypes.ChooseOne, RaceId = "1" },
            new RaceModel { Name = "Judge", DateCreated = createDate.AddSeconds(1), RaceType = RaceTypes.ChooseMany, RaceId = "2" },
            new RaceModel { Name = "Governor", DateCreated = createDate.AddSeconds(2), RaceType = RaceTypes.ChooseOne, RaceId = "3" }
        };
    }

    public class MoqDataAccessor
    {
        public readonly Mock<MoqTrueVoteDbContext> mockUserContext;
        public readonly Mock<MoqTrueVoteDbContext> mockElectionContext;
        public readonly Mock<MoqTrueVoteDbContext> mockBallotContext;
        public readonly Mock<MoqTrueVoteDbContext> mockCandidateContext;
        public readonly Mock<MoqTrueVoteDbContext> mockRaceContext;
        public readonly IQueryable<UserModel> mockUserDataQueryable;
        public readonly IQueryable<ElectionModel> mockElectionDataQueryable;
        public readonly IQueryable<BallotModel> mockBallotDataQueryable;
        public readonly IQueryable<CandidateModel> mockCandidateDataQueryable;
        public readonly ICollection<CandidateModel> mockCandidateDataCollection;
        public readonly IQueryable<RaceModel> mockRaceDataQueryable;
        public readonly ICollection<RaceModel> mockRaceDataCollection;

        public Mock<DbSet<UserModel>> mockUserSet { get; private set; }
        public Mock<DbSet<RaceModel>> mockRaceSet { get; private set; }
        public Mock<DbSet<CandidateModel>> mockCandidateSet { get; private set; }
        public Mock<DbSet<ElectionModel>> mockElectionSet { get; private set; }
        public Mock<DbSet<BallotModel>> mockBallotSet { get; private set; }

        // https://docs.microsoft.com/en-us/ef/ef6/fundamentals/testing/mocking?redirectedfrom=MSDN
        // https://github.com/romantitov/MockQueryable
        public MoqDataAccessor()
        {
            mockUserContext = new Mock<MoqTrueVoteDbContext>();
            mockUserDataQueryable = MoqData.MockUserData.AsQueryable();
            mockUserSet = DbMoqHelper.GetDbSet(mockUserDataQueryable);
            mockUserContext.Setup(m => m.Users).Returns(mockUserSet.Object);
            mockUserContext.Setup(m => m.EnsureCreatedAsync()).Returns(Task.FromResult(true));

            mockElectionContext = new Mock<MoqTrueVoteDbContext>();
            mockElectionDataQueryable = MoqData.MockElectionData.AsQueryable();
            mockElectionSet = DbMoqHelper.GetDbSet(mockElectionDataQueryable);
            mockElectionContext.Setup(m => m.Elections).Returns(mockElectionSet.Object);
            mockElectionContext.Setup(m => m.EnsureCreatedAsync()).Returns(Task.FromResult(true));

            mockBallotContext = new Mock<MoqTrueVoteDbContext>();
            mockBallotDataQueryable = MoqData.MockBallotData.AsQueryable();
            mockBallotSet = DbMoqHelper.GetDbSet(mockBallotDataQueryable);
            mockBallotContext.Setup(m => m.Ballots).Returns(mockBallotSet.Object);
            mockBallotContext.Setup(m => m.EnsureCreatedAsync()).Returns(Task.FromResult(true));

            mockCandidateContext = new Mock<MoqTrueVoteDbContext>();
            mockCandidateDataQueryable = MoqData.MockCandidateData.AsQueryable();
            mockCandidateDataCollection = MoqData.MockCandidateData;
            mockCandidateSet = DbMoqHelper.GetDbSet(mockCandidateDataQueryable);
            mockCandidateContext.Setup(m => m.Candidates).Returns(mockCandidateSet.Object);
            mockCandidateContext.Setup(m => m.EnsureCreatedAsync()).Returns(Task.FromResult(true));

            mockRaceContext = new Mock<MoqTrueVoteDbContext>();
            MoqData.MockRaceData[0].RaceId = "1";
            // TODO Fix this assignment
            MoqData.MockRaceData[0].Candidates = mockCandidateDataCollection;
            MoqData.MockRaceData[1].RaceId = "2";
            MoqData.MockRaceData[2].RaceId = "3";
            mockRaceDataQueryable = MoqData.MockRaceData.AsQueryable();
            mockRaceDataCollection = MoqData.MockRaceData;
            mockRaceSet = DbMoqHelper.GetDbSet(mockRaceDataQueryable);
            mockRaceContext.Setup(m => m.Races).Returns(mockRaceSet.Object);
            mockRaceContext.Setup(m => m.EnsureCreatedAsync()).Returns(Task.FromResult(true));
        }
    }

    // By implementing ITrueVoteDbContext, override the properties and set them to use Mocked data
    public class MoqTrueVoteDbContext : DbContext, ITrueVoteDbContext
    {
        public virtual DbSet<UserModel> Users { get; set; }
        public virtual DbSet<ElectionModel> Elections { get; set; }
        public virtual DbSet<RaceModel> Races { get; set; }
        public virtual DbSet<CandidateModel> Candidates { get; set; }
        public virtual DbSet<BallotModel> Ballots { get; set; }

        protected MoqDataAccessor _moqDataAccessor;

        public MoqTrueVoteDbContext()
        {
            _moqDataAccessor = new MoqDataAccessor();

            Users = _moqDataAccessor.mockUserSet.Object;
            Elections = _moqDataAccessor.mockElectionSet.Object;
            Races = _moqDataAccessor.mockRaceSet.Object;
            Candidates = _moqDataAccessor.mockCandidateSet.Object;
            Ballots = _moqDataAccessor.mockBallotSet.Object;
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
