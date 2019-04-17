using Xunit;

namespace QStreetSearch.Search.Tests
{
    public class WordSetTests
    {
        private readonly AnagramDistanceSearch<string> _anagramDistanceSearch;

        public WordSetTests()
        {
            _anagramDistanceSearch = new AnagramDistanceSearch<string>(new[] {"kyiv", "kharkiv", "odessa", "krakiv"}, new ComparisonKeySelector<string>("Default", x => x));
        }

        [Theory]
        [InlineData("kyiv", "kyiv")]
        [InlineData("iykv", "kyiv")]
        [InlineData("vkyi", "kyiv")]
        [InlineData("viyk", "kyiv")]
        [InlineData("odessa", "odessa")]
        [InlineData("ssaode", "odessa")]
        public void ShouldFindExactAnagramWithZeroDistance(string anagram, string expected)
        {
            var result = _anagramDistanceSearch.FindByDistance(anagram);

            Assert.Equal(0, result[0].Distance);
            Assert.Equal(expected, result[0].Item);
        }

        [Fact]
        public void ShouldReturnOrderedWordsForEmptyString()
        {
            var result = _anagramDistanceSearch.FindByDistance("");

            Assert.Equal(4, result.Count);
            Assert.Equal("kyiv", result[0].Item);
            Assert.Equal("kharkiv", result[3].Item);
        }

        [Fact]
        public void ShouldReturnOrderedWords()
        {
            var result = _anagramDistanceSearch.FindByDistance("kraiv");

            Assert.Equal(4, result.Count);
            Assert.Equal("krakiv", result[0].Item);
            Assert.Equal("kharkiv", result[1].Item);
            Assert.Equal("kyiv", result[2].Item);
            Assert.Equal("odessa", result[3].Item);
        }
    }
}