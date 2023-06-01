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
        public static DateTime endDate = DateTime.Parse("2023-02-25").AddDays(30);
        public static DateTime createDate = DateTime.Parse("2022-12-17");
        public static DateTime createDate2 = DateTime.Parse("2022-12-17").AddHours(1);
        public static DateTime createDate3 = DateTime.Parse("2022-12-17").AddHours(2);

        public static List<UserModel> MockUserData => new()
        {
            new UserModel { UserId = "userid1", Email = "foo@foo.com", DateCreated = createDate, FirstName = "Foo" },
            new UserModel { UserId = "userid2", Email = "foo2@bar.com", DateCreated = createDate2, FirstName = "Foo2" },
            new UserModel { UserId = "userid3", Email = "boo@bar.com", DateCreated = createDate3, FirstName = "Boo" }
        };

        public static List<ElectionModel> MockElectionData => new()
        {
            new ElectionModel { ElectionId = "electionid1", Name = "California State", DateCreated = createDate, StartDate = startDate, EndDate = endDate },
            new ElectionModel { ElectionId = "electionid2", Name = "Los Angeles County", DateCreated = createDate2, StartDate = startDate, EndDate = endDate },
            new ElectionModel { ElectionId = "electionid3", Name = "Federal", DateCreated = createDate3, StartDate = startDate, EndDate = endDate },
        };

        public static List<BallotModel> MockBallotData => new()
        {
            new BallotModel { BallotId = "ballotid1", DateCreated = createDate, ElectionId = "electionid1", Election = MockElectionData[0] },
            new BallotModel { BallotId = "ballotid2", DateCreated = createDate2, ElectionId = "electionid1", Election = MockElectionData[0] },
            new BallotModel { BallotId = "ballotid3", DateCreated = createDate3, ElectionId = "electionid1", Election = MockElectionData[0] },
        };

        public static List<CandidateModel> MockCandidateData => new()
        {
            new CandidateModel { CandidateId = "candidateid1", Name = "John Smith", DateCreated = createDate, PartyAffiliation = "Republican" },
            new CandidateModel { CandidateId = "candidateid2", Name = "Jane Doe", DateCreated = createDate2, PartyAffiliation = "Democrat" }
        };

        public static List<RaceModel> MockRaceData => new()
        {
            new RaceModel { RaceId = "raceid1", Name = "President", DateCreated = createDate, RaceType = RaceTypes.ChooseOne },
            new RaceModel { RaceId = "raceid2", Name = "Judge", DateCreated = createDate2, RaceType = RaceTypes.ChooseMany },
            new RaceModel { RaceId = "raceid3", Name = "Governor", DateCreated = createDate3, RaceType = RaceTypes.ChooseOne }
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
