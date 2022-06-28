using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public readonly Mock<TrueVoteDbContext> mockUserContext;
        public readonly Mock<TrueVoteDbContext> mockElectionContext;
        public readonly Mock<TrueVoteDbContext> mockCandidateContext;
        public readonly Mock<TrueVoteDbContext> mockRaceContext;
        public readonly IQueryable<UserModel> mockUserDataQueryable;
        public readonly IQueryable<RaceModel> mockRaceDataQueryable;
        public readonly IQueryable<CandidateModel> mockCandidateDataQueryable;
        public readonly ICollection<CandidateModel> mockCandidateDataCollection;
        public readonly IQueryable<ElectionModel> mockElectionDataQueryable;

        // https://docs.microsoft.com/en-us/ef/ef6/fundamentals/testing/mocking?redirectedfrom=MSDN
        // https://github.com/romantitov/MockQueryable
        public MoqDataAccessor()
        {
            mockUserContext = new Mock<TrueVoteDbContext>();
            mockUserDataQueryable = MoqData.MockUserData.AsQueryable();
            var mockUserSet = DbMoqHelper.GetDbSet(mockUserDataQueryable);
            mockUserContext.Setup(m => m.Users).Returns(mockUserSet.Object);

            mockElectionContext = new Mock<TrueVoteDbContext>();
            mockElectionDataQueryable = MoqData.MockElectionData.AsQueryable();
            var mockElectionSet = DbMoqHelper.GetDbSet(mockElectionDataQueryable);
            mockElectionContext.Setup(m => m.Elections).Returns(mockElectionSet.Object);

            mockCandidateContext = new Mock<TrueVoteDbContext>();
            mockCandidateDataQueryable = MoqData.MockCandidateData.AsQueryable();
            mockCandidateDataCollection = MoqData.MockCandidateData;
            var mockCandidateSet = DbMoqHelper.GetDbSet(mockCandidateDataQueryable);
            mockCandidateContext.Setup(m => m.Candidates).Returns(mockCandidateSet.Object);

            mockRaceContext = new Mock<TrueVoteDbContext>();
            MoqData.MockRaceData[0].RaceId = "1";
            // TODO Fix this assignment
            MoqData.MockRaceData[0].Candidates = mockCandidateDataCollection;
            MoqData.MockRaceData[1].RaceId = "2";
            MoqData.MockRaceData[2].RaceId = "3";
            mockRaceDataQueryable = MoqData.MockRaceData.AsQueryable();
            var mockRaceSet = DbMoqHelper.GetDbSet(mockRaceDataQueryable);
            mockRaceContext.Setup(m => m.Races).Returns(mockRaceSet.Object);
        }
    }
}
