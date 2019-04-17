using System;
using Xunit;

namespace QStreetSearch.Search.Tests
{
    public class PatternSearchTests
    {
        private readonly PatternSearch<string> _search = new PatternSearch<string>(new[]
            {
                "constable", "constitution", "balcony", "bacon", "surprise"
            },
            new[]
            {
                new ComparisonKeySelector<string>("Default", x => x)
            });

        [Theory]
        [InlineData("con*", 4)]
        [InlineData("Con*", 4)]
        [InlineData("CON*", 4)]
        [InlineData("*con*", 4)]
        [InlineData("*CON*", 4)]
        [InlineData("*Con*", 4)]
        [InlineData("constabl?", 1)]
        [InlineData("Constabl?", 1)]
        [InlineData("coNSTABL?", 1)]
        [InlineData("constable", 1)]
        [InlineData("CONStable", 1)]
        [InlineData("CONSTABLE", 1)]
        [InlineData("*i*", 2)]
        [InlineData("*I*", 2)]
        [InlineData("*E", 2)]
        [InlineData("*e", 2)]
        [InlineData("", 5)]
        public void ShouldFindMatch(string term, int count)
        {
            var results = _search.FindByPattern(term);

            Assert.Equal(count, results.Count);
        }

        [Fact]
        public void ShouldThrowOnNullKey()
        {
            Assert.Throws<ArgumentNullException>(() => _search.FindByPattern(null));
        }
    }
}