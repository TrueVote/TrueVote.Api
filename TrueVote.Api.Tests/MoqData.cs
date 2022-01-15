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
    }
}
