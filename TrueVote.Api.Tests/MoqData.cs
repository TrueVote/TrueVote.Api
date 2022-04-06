using System;
using System.Collections.Generic;
using System.Linq;
using TrueVote.Api.Models;

namespace TrueVote.Api.Tests
{
    internal static class MoqData
    {
        internal static IQueryable<UserModel> MockUserData => new List<UserModel>
        {
            new UserModel { Email = "foo@foo.com", DateCreated = DateTime.Now, FirstName = "Foo", UserId = "1" }
        }.AsQueryable();

        internal static IQueryable<ElectionModel> MockElectionData => new List<ElectionModel>
        {
            new ElectionModel { Name = "California State", DateCreated = DateTime.Now }
        }.AsQueryable();

        internal static IQueryable<RaceModel> MockRaceData => new List<RaceModel>
        {
            new RaceModel { Name = "President", DateCreated = DateTime.Now, RaceType = RaceTypes.ChooseOne }
        }.AsQueryable();

        internal static IQueryable<CandidateModel> MockCandidateData => new List<CandidateModel>
        {
            new CandidateModel { Name = "John Smith", DateCreated = DateTime.Now, PartyAffiliation = "Republican", CandidateId =  "1" },
            new CandidateModel { Name = "Jane Doe", DateCreated = DateTime.Now, PartyAffiliation = "Demoocrat", CandidateId = "2" }
        }.AsQueryable();

        internal static ICollection<CandidateModel> MockCandidateDataCollection => new List<CandidateModel>
        {
            new CandidateModel { Name = "John Smith", DateCreated = DateTime.Now, PartyAffiliation = "Republican", CandidateId = "1" },
            new CandidateModel { Name = "Jane Doe", DateCreated = DateTime.Now, PartyAffiliation = "Demoocrat", CandidateId = "2" }
        };
    }
}
