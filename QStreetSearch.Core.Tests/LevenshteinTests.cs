using Xunit;

namespace QStreetSearch.Search.Tests
{
    public class LevenshteinTests
    {
        [Theory]
        [InlineData("a", "a", 0)]
        [InlineData("a", "", 1)]
        [InlineData("", "a", 1)]
        [InlineData("", "aa", 2)]
        [InlineData("a", "aa", 1)]
        [InlineData("ab", "aa", 1)]
        [InlineData("abcd", "aa", 3)]
        [InlineData("mart", "karma", 5)]
        [InlineData("isle", "kisle", 1)]
        [InlineData("intention", "execution", 5)]
        public void FindsCorrectDistance(string first, string second, int expected)
        {
            var dist = Levenshtein.Calculate(first, second);

            Assert.Equal(expected, dist);

        }
    }
}
