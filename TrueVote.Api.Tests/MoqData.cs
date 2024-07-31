using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
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
        private static readonly DateTime startDate = DateTime.Parse("2024-02-25");
        private static readonly DateTime endDate = DateTime.Parse("2026-02-25").AddDays(30);
        private static readonly DateTime createDate = DateTime.Parse("2023-12-17");
        private static readonly DateTime createDate2 = DateTime.Parse("2023-12-17").AddHours(1);
        private static readonly DateTime createDate3 = DateTime.Parse("2023-12-17").AddHours(2);
        private static readonly DateTime createDate4 = DateTime.Parse("2023-12-17").AddHours(3);
        private static readonly DateTime createDate5 = DateTime.Parse("2023-12-17").AddHours(4);

        public static List<UserModel> MockUserData => new()
        {
            new UserModel { UserId = "userid1", Email = "foo@foo.com", DateCreated = createDate, DateUpdated = DateTime.MinValue, FullName = "Foo Bar", NostrPubKey = "npub1", UserPreferences = new UserPreferencesModel() },
            new UserModel { UserId = "userid2", Email = "foo2@bar.com", DateCreated = createDate2, DateUpdated = DateTime.MinValue, FullName = "Foo2 Bar", NostrPubKey = "npub2", UserPreferences = new UserPreferencesModel() },
            new UserModel { UserId = "userid3", Email = "boo@bar.com", DateCreated = createDate3, DateUpdated = DateTime.MinValue, FullName = "Boo Bar", NostrPubKey = "npub3", UserPreferences = new UserPreferencesModel() }
        };

        public static List<ElectionModel> MockElectionData => new()
        {
            new ElectionModel { ElectionId = "electionid1", Name = "California State", DateCreated = createDate, StartDate = startDate, EndDate = endDate, Description = "desc1", HeaderImageUrl = "url1", Races = MockRaceData },
            new ElectionModel { ElectionId = "electionid2", Name = "Los Angeles County", DateCreated = createDate2, StartDate = startDate, EndDate = endDate, Description = "desc2", HeaderImageUrl = "url2", Races = [] },
            new ElectionModel { ElectionId = "electionid3", Name = "Federal", DateCreated = createDate3, StartDate = startDate, EndDate = endDate, Description = "desc3", HeaderImageUrl = "url3", Races = [] },
            new ElectionModel { ElectionId = "electionid4", Name = "Association", DateCreated = createDate4, StartDate = DateTime.Now.AddDays(-30), EndDate = DateTime.Now.AddDays(-10), Description = "desc4", HeaderImageUrl = "url4", Races = [] },
            new ElectionModel { ElectionId = "electionid5", Name = "Union", DateCreated = createDate5, StartDate = DateTime.Now.AddDays(10), EndDate = DateTime.Now.AddDays(30), Description = "desc5", HeaderImageUrl = "url5", Races = [] },
        };

        public static List<BallotModel> MockBallotData => new()
        {
            new BallotModel { BallotId = "ballotid1", DateCreated = createDate, Election = MockElectionData[0] },
            new BallotModel { BallotId = "ballotid2", DateCreated = createDate2, Election = MockElectionData[0] },
            new BallotModel { BallotId = "ballotid3", DateCreated = createDate3, Election = MockElectionData[0] },
            new BallotModel { BallotId = "ballotid4", DateCreated = createDate4, Election = MockElectionData[3] },
            new BallotModel { BallotId = "ballotid5", DateCreated = createDate5, Election = MockElectionData[4] },
        };

        public static List<CandidateModel> MockCandidateData => new()
        {
            new CandidateModel { CandidateId = "candidateid1", Name = "John Smith", DateCreated = createDate, PartyAffiliation = "Republican", Selected = false },
            new CandidateModel { CandidateId = "candidateid2", Name = "Jane Doe", DateCreated = createDate2, PartyAffiliation = "Democrat", Selected = false }
        };

        public static List<CandidateModel> MockCandidateData2 => new()
        {
            new CandidateModel { CandidateId = "candidateid21", Name = "John Smith 2", DateCreated = createDate, PartyAffiliation = "Republican", Selected = false },
            new CandidateModel { CandidateId = "candidateid22", Name = "Jane Doe 2", DateCreated = createDate2, PartyAffiliation = "Democrat", Selected = false }
        };

        public static List<CandidateModel> MockCandidateData3 => new()
        {
            new CandidateModel { CandidateId = "candidateid31", Name = "John Smith 3", DateCreated = createDate, PartyAffiliation = "Republican", Selected = false },
            new CandidateModel { CandidateId = "candidateid32", Name = "Jane Doe 3", DateCreated = createDate2, PartyAffiliation = "Democrat", Selected = false }
        };

        public static List<RaceModel> MockRaceData => new()
        {
            new RaceModel { RaceId = "raceid1", Name = "President", DateCreated = createDate, RaceType = RaceTypes.ChooseOne, Candidates = MockCandidateData },
            new RaceModel { RaceId = "raceid2", Name = "Judge", DateCreated = createDate2, RaceType = RaceTypes.ChooseMany, Candidates = MockCandidateData2 },
            new RaceModel { RaceId = "raceid3", Name = "Governor", DateCreated = createDate3, RaceType = RaceTypes.ChooseOne, Candidates = MockCandidateData3 }
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

        public static List<FeedbackModel> MockFeedbackData => new()
        {
            new FeedbackModel { DateCreated = createDate, Feedback = "Some Feedback", FeedbackId = "123", UserId = MockUserData[0].UserId }
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
        public readonly Mock<MoqTrueVoteDbContext> mockFeedbacksContext;

        public Mock<DbSet<UserModel>> MockUserSet { get; private set; }
        public Mock<DbSet<RaceModel>> MockRaceSet { get; private set; }
        public Mock<DbSet<CandidateModel>> MockCandidateSet { get; private set; }
        public Mock<DbSet<ElectionModel>> MockElectionSet { get; private set; }
        public Mock<DbSet<BallotModel>> MockBallotSet { get; private set; }
        public Mock<DbSet<TimestampModel>> MockTimestampSet { get; private set; }
        public Mock<DbSet<BallotHashModel>> MockBallotHashSet { get; private set; }
        public Mock<DbSet<FeedbackModel>> MockFeedbackSet { get; private set; }

        // https://docs.microsoft.com/en-us/ef/ef6/fundamentals/testing/mocking?redirectedfrom=MSDN
        // https://github.com/romantitov/MockQueryable
        public MoqDataAccessor()
        {
            MockUserSet = MoqData.MockUserData.AsQueryable().BuildMockDbSet();
            MockElectionSet = MoqData.MockElectionData.AsQueryable().BuildMockDbSet();
            MockTimestampSet = MoqData.MockTimestampData.AsQueryable().BuildMockDbSet();
            MockBallotHashSet = MoqData.MockBallotHashData.AsQueryable().BuildMockDbSet();
            MockBallotSet = MoqData.MockBallotData.AsQueryable().BuildMockDbSet();
            MockCandidateSet = MoqData.MockCandidateData.AsQueryable().BuildMockDbSet();
            MockRaceSet = MoqData.MockRaceData.AsQueryable().BuildMockDbSet();
            MockFeedbackSet = MoqData.MockFeedbackData.AsQueryable().BuildMockDbSet();

            mockUserContext = new Mock<MoqTrueVoteDbContext>();
            mockUserContext.Setup(m => m.Feedbacks).Returns(MockFeedbackSet.Object);
            mockUserContext.Setup(m => m.Users).Returns(MockUserSet.Object);
            mockUserContext.Setup(m => m.EnsureCreatedAsync()).Returns(Task.FromResult(true));

            mockElectionContext = new Mock<MoqTrueVoteDbContext>();
            mockElectionContext.Setup(m => m.Elections).Returns(MockElectionSet.Object);
            mockElectionContext.Setup(m => m.Races).Returns(MockRaceSet.Object);
            mockElectionContext.Setup(m => m.EnsureCreatedAsync()).Returns(Task.FromResult(true));

            mockTimestampContext = new Mock<MoqTrueVoteDbContext>();
            mockTimestampContext.Setup(m => m.Timestamps).Returns(MockTimestampSet.Object);
            mockTimestampContext.Setup(m => m.EnsureCreatedAsync()).Returns(Task.FromResult(true));

            mockBallotHashContext = new Mock<MoqTrueVoteDbContext>();
            mockBallotHashContext.Setup(m => m.BallotHashes).Returns(MockBallotHashSet.Object);
            mockBallotHashContext.Setup(m => m.EnsureCreatedAsync()).Returns(Task.FromResult(true));

            mockBallotContext = new Mock<MoqTrueVoteDbContext>();
            mockBallotContext.Setup(m => m.Elections).Returns(MockElectionSet.Object);
            mockBallotContext.Setup(m => m.Races).Returns(MockRaceSet.Object);
            mockBallotContext.Setup(m => m.Candidates).Returns(MockCandidateSet.Object);
            mockBallotContext.Setup(m => m.Ballots).Returns(MockBallotSet.Object);
            mockBallotContext.Setup(m => m.BallotHashes).Returns(MockBallotHashSet.Object);
            mockBallotContext.Setup(m => m.Timestamps).Returns(MockTimestampSet.Object);
            mockBallotContext.Setup(m => m.EnsureCreatedAsync()).Returns(Task.FromResult(true));

            mockCandidateContext = new Mock<MoqTrueVoteDbContext>();
            mockCandidateContext.Setup(m => m.Candidates).Returns(MockCandidateSet.Object);
            mockCandidateContext.Setup(m => m.EnsureCreatedAsync()).Returns(Task.FromResult(true));

            mockRaceContext = new Mock<MoqTrueVoteDbContext>();
            mockRaceContext.Setup(m => m.Candidates).Returns(MockCandidateSet.Object);
            mockRaceContext.Setup(m => m.Races).Returns(MockRaceSet.Object);
            mockRaceContext.Setup(m => m.EnsureCreatedAsync()).Returns(Task.FromResult(true));

            mockFeedbacksContext = new Mock<MoqTrueVoteDbContext>();
            mockFeedbacksContext.Setup(m => m.Feedbacks).Returns(MockFeedbackSet.Object);
            mockFeedbacksContext.Setup(m => m.EnsureCreatedAsync()).Returns(Task.FromResult(true));

            // Leaving commented code. This is for Mocking UTC time. Helpful for test consistency.
            // var mockUtcNowProvider = new Mock<IUtcNowProvider>();
            // mockUtcNowProvider.Setup(p => p.UtcNow).Returns(MoqData.startDate);
            // UtcNowProviderFactory.SetProvider(mockUtcNowProvider.Object);
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
        public virtual DbSet<FeedbackModel> Feedbacks { get; set; }

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
            Feedbacks = _moqDataAccessor.MockFeedbackSet.Object;
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
