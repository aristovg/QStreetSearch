using System;
using Xunit;

namespace QStreetSearch.Search.Tests
{
    public class SimpleContainsSearchTests
    {
        private readonly SimpleContainsSearch<string> _search = new SimpleContainsSearch<string>(new[]
            {
                "constable", "constitution", "balcony", "bacon", "surprise"
            },
            new[]
            {
                new ComparisonKeySelector<string>("Default", x => x)
            });

        [Theory]
        [InlineData("con", 4)]
        [InlineData("Con", 4)]
        [InlineData("CON", 4)]
        [InlineData("constable", 1)]
        [InlineData("CONStable", 1)]
        [InlineData("CONSTABLE", 1)]
        [InlineData("i", 2)]
        [InlineData("I", 2)]
        [InlineData("", 5)]
        public void ShouldFindMatch(string term, int count)
        {
            var results = _search.FindByContainsSequence(term);

            Assert.Equal(count, results.Count);
        }

        [Fact]
        public void ShouldThrowOnNullKey()
        {
            Assert.Throws<ArgumentNullException>(() => _search.FindByContainsSequence(null));
        }
    }
}