using System.Collections.Generic;
using System.Linq;

namespace QStreetSearch.Parser
{
    internal class KnownStreetTypes
    {
        public static KnownStreetTypes Ua = new KnownStreetTypes("вулиця", "проспект", "провулок", "міст", "площа", "узвіз", "бульвар", "набережна", "шосе", "проїзд", "дорога", "алея");
        public static KnownStreetTypes Ru = new KnownStreetTypes("улица", "проспект", "переулок", "мост", "площадь", "спуск", "бульвар", "набережная", "шоссе", "проезд", "дорога", "аллея");

        private readonly List<string> _knownTypes; 

        private KnownStreetTypes(params string[] types)
        {
            _knownTypes = types.ToList();
        }

        public bool Contains(string s) => _knownTypes.Contains(s);
    }
}