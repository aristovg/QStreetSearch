using Xunit;

namespace QStreetSearch.Search.Tests
{
    public class WordSetTests
    {
        private readonly WordSet<string> _wordSet;

        public WordSetTests()
        {
            _wordSet = new WordSet<string>(new[] {"kyiv", "kharkiv", "odessa", "krakiv"}, x => x);
        }

        [Theory]
        [InlineData("kyiv", "kyiv")]
        [InlineData("iykv", "kyiv")]
        [InlineData("vkyi", "kyiv")]
        [InlineData("viyk", "kyiv")]
        [InlineData("odessa", "odessa")]
        [InlineData("ssaode", "odessa")]
        public void ShouldFindAnagram(string anagram, string expected)
        {
            bool result = _wordSet.TryFindExact(anagram, out var words);

            Assert.True(result);
            Assert.Single(words);
            Assert.Equal(expected, words[0]);
        }

        [Fact]
        public void ShouldReturnOrderedWordsForEmptyString()
        {
            var result = _wordSet.FindByDistance("");

            Assert.Equal(4, result.Count);
            Assert.Equal("kyiv", result[0].Item);
            Assert.Equal("kharkiv", result[3].Item);
        }

        [Fact]
        public void ShouldReturnOrderedWords()
        {
            var result = _wordSet.FindByDistance("kraiv");

            Assert.Equal(4, result.Count);
            Assert.Equal("krakiv", result[0].Item);
            Assert.Equal("kharkiv", result[1].Item);
            Assert.Equal("kyiv", result[2].Item);
            Assert.Equal("odessa", result[3].Item);
        }
    }
}