﻿using Xunit;

namespace QStreetSearch.Parser.Tests
{
    public class StreetTests
    {
        [Theory]
        [InlineData("Крепостной переулок", "крепостной", "переулок")]
        [InlineData("Броварской проспект", "броварской", "проспект")]
        [InlineData("Цимбалов Яр улица", "цимбалов яр", "улица")]
        [InlineData("Юрия Гагарина проспект", "юрия гагарина", "проспект")]
        [InlineData("Надднепровское шоссе", "надднепровское", "шоссе")]
        [InlineData("Спортивная площадь", "спортивная", "площадь")]
        [InlineData("Патриарха Мстислава Скрипника улица", "патриарха мстислава скрипника", "улица")]
        [InlineData("Кольцевая дорога", "кольцевая", "дорога")]
        [InlineData("Военный проезд", "военный", "проезд")]
        [InlineData("Героев Небесной Сотни аллея", "героев небесной сотни", "аллея")]
        [InlineData("непонятная тропа", "непонятная тропа", null)]
        public void ParsesRuStreetName(string fullStreetName, string expectedName, string expectedType)
        {
            var localizedOsm = new LocalizedOsmGeoObject(Language.Ru, new StreetName(fullStreetName));

            var street  = new GeoObject(localizedOsm);
            
            Assert.Equal(expectedName, street.Name);
            Assert.Equal(expectedType, street.Type);
        }

        [Theory]
        [InlineData("Крепостной переулок", "крепостной", "переулок")]
        [InlineData("Броварской проспект", "броварской", "проспект")]
        [InlineData("Цимбалов Яр улица", "цимбалов яр", "улица")]
        [InlineData("Юрия Гагарина проспект", "юрия гагарина", "проспект")]
        [InlineData("Надднепровское шоссе", "надднепровское", "шоссе")]
        [InlineData("Спортивная площадь", "спортивная", "площадь")]
        [InlineData("Патриарха Мстислава Скрипника улица", "патриарха мстислава скрипника", "улица")]
        [InlineData("Кольцевая дорога", "кольцевая", "дорога")]
        [InlineData("Военный проезд", "военный", "проезд")]
        [InlineData("Героев Небесной Сотни аллея", "героев небесной сотни", "аллея")]
        [InlineData("непонятная тропа", "непонятная тропа", null)]
        public void ParsesRuOldStreetName(string fullStreetName, string expectedName, string expectedType)
        {
            var localizedOsm = new LocalizedOsmGeoObject(Language.Ru, new StreetName(fullStreetName, fullStreetName));

            var street = new GeoObject(localizedOsm);

            Assert.Equal(expectedName, street.OldName);
            Assert.Equal(expectedType, street.OldType);
        }
    }
}
