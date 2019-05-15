using System.Collections.Generic;
using System.Linq;

namespace QStreetSearch.Location
{
    public class DistanceToObject
    {
        public float Min { get; }
        public float Max { get; }
        public float Avg { get; }
        public IReadOnlyCollection<float> Distances { get; }

        public bool IsEmpty => !Distances.Any();

        public DistanceToObject(IReadOnlyCollection<float> distances)
        {
            Min = distances.Min();
            Max = distances.Max();
            Avg = distances.Average();
            Distances = distances;
        }
    }
}