using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrueVote.Api.Helpers;
using TrueVote.Api.Interfaces;
using TrueVote.Api.Models;

namespace TrueVote.Api.Tests
{
    public static class MoqData
    {
        public static readonly DateTime startDate = DateTime.Parse("2023-02-25");
        public static readonly DateTime endDate = DateTime.Parse("2023-02-25").AddDays(30);
        public static readonly DateTime createDate = DateTime.Parse("2022-12-17");
        public static readonly DateTime createDate2 = DateTime.Parse("2022-12-17").AddHours(1);
        public static readonly DateTime createDate3 = DateTime.Parse("2022-12-17").AddHours(2);

        public static List<UserModel> MockUserData => new()
        {
            new UserModel { UserId = "userid1", Email = "foo@foo.com", DateCreated = createDate, FirstName = "Foo", NostrPubKey = "npub" },
            new UserModel { UserId = "userid2", Email = "foo2@bar.com", DateCreated = createDate2, FirstName = "Foo2", NostrPubKey = "npub" },
            new UserModel { UserId = "userid3", Email = "boo@bar.com", DateCreated = createDate3, FirstName = "Boo", NostrPubKey = "npub" }
        };

        public static List<ElectionModel> MockElectionData => new()
        {
            new ElectionModel { ElectionId = "electionid1", Name = "California State", DateCreated = createDate, StartDate = startDate, EndDate = endDate, Description = "desc1", HeaderImageUrl = "url1", Races = [] },
            new ElectionModel { ElectionId = "electionid2", Name = "Los Angeles County", DateCreated = createDate2, StartDate = startDate, EndDate = endDate, Description = "desc2", HeaderImageUrl = "url2", Races = [] },
            new ElectionModel { ElectionId = "electionid3", Name = "Federal", DateCreated = createDate3, StartDate = startDate, EndDate = endDate, Description = "desc3", HeaderImageUrl = "url3", Races = [] },
        };

        public static List<BallotModel> MockBallotData => new()
        {
            new BallotModel { BallotId = "ballotid1", DateCreated = createDate, Election = MockElectionData[0] },
            new BallotModel { BallotId = "ballotid2", DateCreated = createDate2, Election = MockElectionData[0] },
            new BallotModel { BallotId = "ballotid3", DateCreated = createDate3, Election = MockElectionData[0] },
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

        public static List<TimestampModel> MockTimestampData => new()
        {
            new TimestampModel { TimestampId = "1", TimestampHashS = "SampleHash1", DateCreated = new DateTime(2023, 01, 01, 11, 11, 11), CalendarServerUrl = "url1", MerkleRoot = [], MerkleRootHash = [], TimestampHash = [] },
            new TimestampModel { TimestampId = "2", TimestampHashS = "SampleHash2", DateCreated = new DateTime(2023, 01, 01, 11, 11, 21), CalendarServerUrl = "url2", MerkleRoot = [], MerkleRootHash = [], TimestampHash = [] }
        };

        public static List<BallotHashModel> MockBallotHashData => new()
        {
            new BallotHashModel { BallotId = "ballotid1", DateCreated = createDate, DateUpdated = createDate, ServerBallotHashS = "123", BallotHashId = "hash1", ServerBallotHash = [] }
        };

        public static BallotList MockBallotList => new()
        {
            Ballots = MockBallotData,
            BallotHashes = MockBallotHashData
        };
    }

    public class MoqDataAccessor
    {
        public readonly Mock<MoqTrueVoteDbContext> mockUserContext;
        public readonly Mock<MoqTrueVoteDbContext> mockElectionContext;
        public readonly Mock<MoqTrueVoteDbContext> mockBallotContext;
        public readonly Mock<MoqTrueVoteDbContext> mockCandidateContext;
        public readonly Mock<MoqTrueVoteDbContext> mockRaceContext;
        public readonly Mock<MoqTrueVoteDbContext> mockTimestampContext;
        public readonly Mock<MoqTrueVoteDbContext> mockBallotHashContext;
        public readonly IQueryable<UserModel> mockUserDataQueryable;
        public readonly ICollection<UserModel> mockUserDataCollection;
        public readonly IQueryable<ElectionModel> mockElectionDataQueryable;
        public readonly ICollection<ElectionModel> mockElectionDataCollection;
        public readonly IQueryable<BallotModel> mockBallotDataQueryable;
        public readonly ICollection<BallotModel> mockBallotDataCollection;
        public readonly IQueryable<CandidateModel> mockCandidateDataQueryable;
        public readonly ICollection<CandidateModel> mockCandidateDataCollection;
        public readonly IQueryable<RaceModel> mockRaceDataQueryable;
        public readonly ICollection<RaceModel> mockRaceDataCollection;
        public readonly IQueryable<TimestampModel> mockTimestampDataQueryable;
        public readonly ICollection<TimestampModel> mockTimestampDataCollection;
        public readonly IQueryable<BallotHashModel> mockBallotHashDataQueryable;
        public readonly ICollection<BallotHashModel> mockBallotHashDataCollection;

        public Mock<DbSet<UserModel>> MockUserSet { get; private set; }
        public Mock<DbSet<RaceModel>> MockRaceSet { get; private set; }
        public Mock<DbSet<CandidateModel>> MockCandidateSet { get; private set; }
        public Mock<DbSet<ElectionModel>> MockElectionSet { get; private set; }
        public Mock<DbSet<BallotModel>> MockBallotSet { get; private set; }
        public Mock<DbSet<TimestampModel>> MockTimestampSet { get; private set; }
        public Mock<DbSet<BallotHashModel>> MockBallotHashSet { get; private set; }

        // https://docs.microsoft.com/en-us/ef/ef6/fundamentals/testing/mocking?redirectedfrom=MSDN
        // https://github.com/romantitov/MockQueryable
        public MoqDataAccessor()
        {
            mockUserContext = new Mock<MoqTrueVoteDbContext>();
            mockUserDataQueryable = MoqData.MockUserData.AsQueryable();
            mockUserDataCollection = MoqData.MockUserData;
            MockUserSet = DbMoqHelper.GetDbSet(mockUserDataQueryable);
            mockUserContext.Setup(m => m.Users).Returns(MockUserSet.Object);
            mockUserContext.Setup(m => m.EnsureCreatedAsync()).Returns(Task.FromResult(true));

            mockElectionContext = new Mock<MoqTrueVoteDbContext>();
            mockElectionDataQueryable = MoqData.MockElectionData.AsQueryable();
            mockElectionDataCollection = MoqData.MockElectionData;
            MockElectionSet = DbMoqHelper.GetDbSet(mockElectionDataQueryable);
            mockElectionContext.Setup(m => m.Elections).Returns(MockElectionSet.Object);
            mockElectionContext.Setup(m => m.EnsureCreatedAsync()).Returns(Task.FromResult(true));

            mockTimestampContext = new Mock<MoqTrueVoteDbContext>();
            mockTimestampDataQueryable = MoqData.MockTimestampData.AsQueryable();
            mockTimestampDataCollection = MoqData.MockTimestampData;
            MockTimestampSet = DbMoqHelper.GetDbSet(mockTimestampDataQueryable);
            mockTimestampContext.Setup(m => m.Timestamps).Returns(MockTimestampSet.Object);
            mockTimestampContext.Setup(m => m.EnsureCreatedAsync()).Returns(Task.FromResult(true));

            mockBallotHashContext = new Mock<MoqTrueVoteDbContext>();
            mockBallotHashDataQueryable = MoqData.MockBallotHashData.AsQueryable();
            mockBallotHashDataCollection = MoqData.MockBallotHashData;
            MockBallotHashSet = DbMoqHelper.GetDbSet(mockBallotHashDataQueryable);
            mockBallotHashContext.Setup(m => m.BallotHashes).Returns(MockBallotHashSet.Object);
            mockBallotHashContext.Setup(m => m.EnsureCreatedAsync()).Returns(Task.FromResult(true));

            mockBallotContext = new Mock<MoqTrueVoteDbContext>();
            mockBallotDataQueryable = MoqData.MockBallotData.AsQueryable();
            mockBallotDataCollection = MoqData.MockBallotData;
            MockBallotSet = DbMoqHelper.GetDbSet(mockBallotDataQueryable);
            mockBallotContext.Setup(m => m.Ballots).Returns(MockBallotSet.Object);
            mockBallotContext.Setup(m => m.BallotHashes).Returns(MockBallotHashSet.Object);
            mockBallotContext.Setup(m => m.Timestamps).Returns(MockTimestampSet.Object);
            mockBallotContext.Setup(m => m.EnsureCreatedAsync()).Returns(Task.FromResult(true));

            mockCandidateContext = new Mock<MoqTrueVoteDbContext>();
            mockCandidateDataQueryable = MoqData.MockCandidateData.AsQueryable();
            mockCandidateDataCollection = MoqData.MockCandidateData;
            MockCandidateSet = DbMoqHelper.GetDbSet(mockCandidateDataQueryable);
            mockCandidateContext.Setup(m => m.Candidates).Returns(MockCandidateSet.Object);
            mockCandidateContext.Setup(m => m.EnsureCreatedAsync()).Returns(Task.FromResult(true));

            mockRaceContext = new Mock<MoqTrueVoteDbContext>();
            mockRaceDataQueryable = MoqData.MockRaceData.AsQueryable();
            mockRaceDataCollection = MoqData.MockRaceData;
            MockRaceSet = DbMoqHelper.GetDbSet(mockRaceDataQueryable);
            mockRaceContext.Setup(m => m.Races).Returns(MockRaceSet.Object);
            mockRaceContext.Setup(m => m.EnsureCreatedAsync()).Returns(Task.FromResult(true));

            var mockUtcNowProvider = new Mock<IUtcNowProvider>();
            mockUtcNowProvider.Setup(p => p.UtcNow).Returns(MoqData.startDate);
            UtcNowProviderFactory.SetProvider(mockUtcNowProvider.Object);
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
        public virtual DbSet<TimestampModel> Timestamps { get; set; }
        public virtual DbSet<BallotHashModel> BallotHashes { get; set; }

        protected MoqDataAccessor _moqDataAccessor;

        public MoqTrueVoteDbContext()
        {
            _moqDataAccessor = new MoqDataAccessor();

            Users = _moqDataAccessor.MockUserSet.Object;
            Elections = _moqDataAccessor.MockElectionSet.Object;
            Races = _moqDataAccessor.MockRaceSet.Object;
            Candidates = _moqDataAccessor.MockCandidateSet.Object;
            Ballots = _moqDataAccessor.MockBallotSet.Object;
            Timestamps = _moqDataAccessor.MockTimestampSet.Object;
            BallotHashes = _moqDataAccessor.MockBallotHashSet.Object;
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
