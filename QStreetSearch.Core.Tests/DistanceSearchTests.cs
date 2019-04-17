using System;
using Xunit;

namespace QStreetSearch.Search.Tests
{
    public class DistanceSearchTests
    {
        private readonly DistanceSearch<string> _anagramDistanceSearch;

        public DistanceSearchTests()
        {
            _anagramDistanceSearch = new DistanceSearch<string>(new[] { "kyiv", "kharkiv", "odessa", "krakiv" },
                new ComparisonKeySelector<string>("Default", x => x));
        }


        [Fact]
        public void ShouldReturnOrderedWordsForEmptyString()
        {
            var result = _anagramDistanceSearch.FindByDistance("");

            Assert.Equal(4, result.Count);
            Assert.Equal("kyiv", result[0].Item);
            Assert.Equal("kharkiv", result[3].Item);
        }

        
        [Theory]
        [InlineData("KRAK")]
        [InlineData("krak")]
        [InlineData("kRaK")]
        public void ShouldReturnOrderedWordsForAnyCase(string input)
        {
            var result = _anagramDistanceSearch.FindByDistance(input);

            Assert.Equal(4, result.Count);
            Assert.Equal("krakiv", result[0].Item);
            Assert.Equal("kyiv", result[1].Item);
            Assert.Equal("kharkiv", result[2].Item);
            Assert.Equal("odessa", result[3].Item);
        }

        [Fact]
        public void ShouldThrowOnNullKey()
        {
            Assert.Throws<ArgumentNullException>(() => _anagramDistanceSearch.FindByDistance(null));
        }
    }
}