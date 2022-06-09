namespace TrueVote.Api.Services
{
    public class Query
    {
        public Person GetPerson()
        {
            return new Person("Luke Skywalker");
        }
    }

    public class Person
    {
        public Person(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
