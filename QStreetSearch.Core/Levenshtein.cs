using System;

namespace QStreetSearch.Search
{
    internal class Levenshtein
    {
        public static int Calculate(string first, string second)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));

            int m = first.Length;
            int n = second.Length;

            short[,] aux = new short[m + 1, n + 1];

            // Fill d[][] in bottom up manner 
            for (short i = 0; i <= m; i++)
            {
                for (short j = 0; j <= n; j++)
                {
                    // If first string is empty, only option is to 
                    // insert all characters of second string 
                    if (i == 0)
                        aux[i, j] = j; // Min. operations = j 

                    // If second string is empty, only option is to 
                    // remove all characters of second string 
                    else if (j == 0)
                        aux[i, j] = i; // Min. operations = i 

                    // If last characters are same, ignore last char 
                    // and recur for remaining string 
                    else if (first[i - 1] == second[j - 1])
                        aux[i, j] = aux[i - 1, j - 1];

                    // If the last character is different, consider all 
                    // possibilities and find the minimum 
                    else
                        aux[i, j] = (short) (Min(aux[i, j - 1], aux[i - 1, j], aux[i - 1, j - 1]) + 1); // Replace
                }
            }

            return aux[m, n];

        }

        private static short Min(short n1, short n2, short n3) => Math.Min(Math.Min(n1, n2), n3);

    }
}