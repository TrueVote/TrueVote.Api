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
            new ElectionModel { ElectionId = "electionid6", Name = "Club", DateCreated = createDate5, StartDate = DateTime.Now.AddDays(10), EndDate = DateTime.Now.AddDays(30), Description = "desc6", HeaderImageUrl = "url6", Races = [] },
        };

        public static List<BallotModel> MockBallotData => new()
        {
            new BallotModel { BallotId = "ballotid1", DateCreated = createDate, Election = MockElectionData[0], ElectionId = MockElectionData[0].ElectionId },
            new BallotModel { BallotId = "ballotid2", DateCreated = createDate2, Election = MockElectionData[0], ElectionId = MockElectionData[0].ElectionId },
            new BallotModel { BallotId = "ballotid3", DateCreated = createDate3, Election = MockElectionData[0], ElectionId = MockElectionData[0].ElectionId },
            new BallotModel { BallotId = "ballotid4", DateCreated = createDate4, Election = MockElectionData[3], ElectionId = MockElectionData[3].ElectionId },
            new BallotModel { BallotId = "ballotid5", DateCreated = createDate5, Election = MockElectionData[4], ElectionId = MockElectionData[4].ElectionId },
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

        public static List<AccessCodeModel> MockElectionAccessCodeData => new()
        {
            new AccessCodeModel { DateCreated = createDate, RequestId = "122", ElectionId = MockElectionData[0].ElectionId, AccessCode = "accesscode0", RequestDescription = "Mock Election Access Code Harness 0", RequestedByUserId = MockUserData[0].UserId },
            new AccessCodeModel { DateCreated = createDate, RequestId = "123", ElectionId = MockElectionData[0].ElectionId, AccessCode = "accesscode1", RequestDescription = "Mock Election Access Code Harness 1", RequestedByUserId = MockUserData[0].UserId },
            new AccessCodeModel { DateCreated = createDate, RequestId = "124", ElectionId = MockElectionData[0].ElectionId, AccessCode = "accesscode2", RequestDescription = "Mock Election Access Code Harness 2", RequestedByUserId = MockUserData[0].UserId },
            new AccessCodeModel { DateCreated = createDate, RequestId = "125", ElectionId = "blah", AccessCode = "accesscode3", RequestDescription = "Mock Election Access Code Harness 3", RequestedByUserId = MockUserData[0].UserId }
        };

        public static List<UsedAccessCodeModel> MockUsedAccessCodeData => new()
        {
            new UsedAccessCodeModel { AccessCode = "accesscode1", DateCreated = createDate.Date },
            new UsedAccessCodeModel { AccessCode = "accesscode2", DateCreated = createDate.Date },
        };

        public static List<ElectionUserBindingModel> MockElectionUserBindingsData => new()
        {
            new ElectionUserBindingModel { UserId = MockUserData[2].UserId, ElectionId = MockElectionData[0].ElectionId, DateCreated = createDate.Date },
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
        public readonly Mock<MoqTrueVoteDbContext> mockElectionAccessCodeContext;
        public readonly Mock<MoqTrueVoteDbContext> mockUsedAccessCodeContext;
        public readonly Mock<MoqTrueVoteDbContext> mockElectionUserBindingsContext;

        public Mock<DbSet<UserModel>> MockUserSet { get; private set; }
        public Mock<DbSet<RaceModel>> MockRaceSet { get; private set; }
        public Mock<DbSet<CandidateModel>> MockCandidateSet { get; private set; }
        public Mock<DbSet<ElectionModel>> MockElectionSet { get; private set; }
        public Mock<DbSet<BallotModel>> MockBallotSet { get; private set; }
        public Mock<DbSet<TimestampModel>> MockTimestampSet { get; private set; }
        public Mock<DbSet<BallotHashModel>> MockBallotHashSet { get; private set; }
        public Mock<DbSet<FeedbackModel>> MockFeedbackSet { get; private set; }
        public Mock<DbSet<AccessCodeModel>> MockElectionAccessCodeSet { get; private set; }
        public Mock<DbSet<UsedAccessCodeModel>> MockUsedAccessCodeSet { get; private set; }
        public Mock<DbSet<ElectionUserBindingModel>> MockElectionUserBindingsSet { get; private set; }

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
            MockElectionAccessCodeSet = MoqData.MockElectionAccessCodeData.AsQueryable().BuildMockDbSet();
            MockUsedAccessCodeSet = MoqData.MockUsedAccessCodeData.AsQueryable().BuildMockDbSet();
            MockElectionUserBindingsSet = MoqData.MockElectionUserBindingsData.AsQueryable().BuildMockDbSet();

            mockUserContext = new Mock<MoqTrueVoteDbContext>();
            mockUserContext.Setup(m => m.Feedbacks).Returns(MockFeedbackSet.Object);
            mockUserContext.Setup(m => m.Users).Returns(MockUserSet.Object);

            mockElectionContext = new Mock<MoqTrueVoteDbContext>();
            mockElectionContext.Setup(m => m.Elections).Returns(MockElectionSet.Object);
            mockElectionContext.Setup(m => m.Races).Returns(MockRaceSet.Object);
            mockElectionContext.Setup(m => m.Users).Returns(MockUserSet.Object);
            mockElectionContext.Setup(m => m.ElectionAccessCodes).Returns(MockElectionAccessCodeSet.Object);
            mockElectionContext.Setup(m => m.UsedAccessCodes).Returns(MockUsedAccessCodeSet.Object);

            mockTimestampContext = new Mock<MoqTrueVoteDbContext>();
            mockTimestampContext.Setup(m => m.Timestamps).Returns(MockTimestampSet.Object);

            mockBallotHashContext = new Mock<MoqTrueVoteDbContext>();
            mockBallotHashContext.Setup(m => m.BallotHashes).Returns(MockBallotHashSet.Object);

            mockBallotContext = new Mock<MoqTrueVoteDbContext>();
            mockBallotContext.Setup(m => m.Elections).Returns(MockElectionSet.Object);
            mockBallotContext.Setup(m => m.Races).Returns(MockRaceSet.Object);
            mockBallotContext.Setup(m => m.Candidates).Returns(MockCandidateSet.Object);
            mockBallotContext.Setup(m => m.Ballots).Returns(MockBallotSet.Object);
            mockBallotContext.Setup(m => m.BallotHashes).Returns(MockBallotHashSet.Object);
            mockBallotContext.Setup(m => m.Timestamps).Returns(MockTimestampSet.Object);
            mockBallotContext.Setup(m => m.ElectionAccessCodes).Returns(MockElectionAccessCodeSet.Object);
            mockBallotContext.Setup(m => m.UsedAccessCodes).Returns(MockUsedAccessCodeSet.Object);
            mockBallotContext.Setup(m => m.ElectionUserBindings).Returns(MockElectionUserBindingsSet.Object);

            mockCandidateContext = new Mock<MoqTrueVoteDbContext>();
            mockCandidateContext.Setup(m => m.Candidates).Returns(MockCandidateSet.Object);

            mockRaceContext = new Mock<MoqTrueVoteDbContext>();
            mockRaceContext.Setup(m => m.Candidates).Returns(MockCandidateSet.Object);
            mockRaceContext.Setup(m => m.Races).Returns(MockRaceSet.Object);

            mockFeedbacksContext = new Mock<MoqTrueVoteDbContext>();
            mockFeedbacksContext.Setup(m => m.Feedbacks).Returns(MockFeedbackSet.Object);

            mockElectionAccessCodeContext = new Mock<MoqTrueVoteDbContext>();
            mockElectionAccessCodeContext.Setup(m => m.ElectionAccessCodes).Returns(MockElectionAccessCodeSet.Object);

            mockUsedAccessCodeContext = new Mock<MoqTrueVoteDbContext>();
            mockUsedAccessCodeContext.Setup(m => m.UsedAccessCodes).Returns(MockUsedAccessCodeSet.Object);

            mockElectionUserBindingsContext = new Mock<MoqTrueVoteDbContext>();
            mockElectionUserBindingsContext.Setup(m => m.ElectionUserBindings).Returns(MockElectionUserBindingsSet.Object);

            // Leaving commented code. This is for Mocking UTC time. Helpful for test consistency.
            // var mockUtcNowProvider = new Mock<IUtcNowProvider>();
            // mockUtcNowProvider.Setup(p => p.UtcNow).Returns(MoqData.startDate);
            // UtcNowProviderFactory.SetProvider(mockUtcNowProvider.Object);
        }
    }

    // By implementing ITrueVoteDbContext, override the properties and set them to use Mocked data
    public class MoqTrueVoteDbContext : DbContext, ITrueVoteDbContext
    {
        public virtual required DbSet<UserModel> Users { get; set; }
        public virtual required DbSet<ElectionModel> Elections { get; set; }
        public virtual required DbSet<RaceModel> Races { get; set; }
        public virtual required DbSet<CandidateModel> Candidates { get; set; }
        public virtual required DbSet<BallotModel> Ballots { get; set; }
        public virtual required DbSet<TimestampModel> Timestamps { get; set; }
        public virtual required DbSet<BallotHashModel> BallotHashes { get; set; }
        public virtual required DbSet<FeedbackModel> Feedbacks { get; set; }
        public virtual required DbSet<AccessCodeModel> ElectionAccessCodes { get; set; }
        public virtual required DbSet<UsedAccessCodeModel> UsedAccessCodes { get; set; }
        public virtual required DbSet<ElectionUserBindingModel> ElectionUserBindings { get; set; }

        protected MoqDataAccessor _moqDataAccessor;

        public MoqTrueVoteDbContext(DbContextOptions<MoqTrueVoteDbContext> options) : base(options)
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
            ElectionAccessCodes = _moqDataAccessor.MockElectionAccessCodeSet.Object;
            UsedAccessCodes = _moqDataAccessor.MockUsedAccessCodeSet.Object;
            ElectionUserBindings = _moqDataAccessor.MockElectionUserBindingsSet.Object;
        }

        // Keep parameterless constructor if needed
        public MoqTrueVoteDbContext() : this(new DbContextOptionsBuilder<MoqTrueVoteDbContext>().UseInMemoryDatabase(databaseName: "TestDb").Options)
        {
        }

        public virtual async Task<bool> EnsureCreatedAsync()
        {
            return await Database.EnsureCreatedAsync();
        }

        public virtual async Task<int> SaveChangesAsync()
        {
            return await base.SaveChangesAsync();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseInMemoryDatabase(databaseName: "TestDb");
            }
        }
    }
}
