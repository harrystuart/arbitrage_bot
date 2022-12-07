using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arbitrage
{
    public static class StringDistance
    {
        /// <summary>
        /// Compute the distance between two strings.
        /// </summary>
        public static int LevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }

        public static int LengthUnbiasedLevenshteinDistance(string s, string t)
        {
            int levenshteinDist = LevenshteinDistance(s, t);
            return levenshteinDist - Math.Abs(s.Length - t.Length);
        }

        public static bool TryFindLongestCommonSubstringSlice(string str1, string str2, out int start, out int end)
        {
            int[,] num = new int[str1.Length, str2.Length];
            int maxlen = 0;
            int lastSubsBegin = 0;
            StringBuilder subStrBuilder = new StringBuilder();

            for (int i = 0; i < str1.Length; i++)
            {
                for (int j = 0; j < str2.Length; j++)
                {
                    if (str1[i] != str2[j])
                    {
                        num[i, j] = 0;
                    }
                    else
                    {
                        if ((i == 0) || (j == 0))
                            num[i, j] = 1;
                        else
                            num[i, j] = 1 + num[i - 1, j - 1];

                        if (num[i, j] > maxlen)
                        {
                            maxlen = num[i, j];

                            int thisSubsBegin = i - num[i, j] + 1;

                            if (lastSubsBegin == thisSubsBegin)
                            {
                                subStrBuilder.Append(str1[i]);
                            }
                            else
                            {
                                lastSubsBegin = thisSubsBegin;
                                subStrBuilder.Length = 0;
                                subStrBuilder.Append(str1.Substring(lastSubsBegin, (i + 1) - lastSubsBegin));
                            }
                        }
                    }
                }
            }

            if (maxlen > 0)
            {
                // Inclusive
                start = lastSubsBegin;
                end = lastSubsBegin + maxlen - 1;
                return true;
            }
            else
            {
                start = 0;
                end = 0;
                return false;
            }
        }

        public static double NumberOfSubstringMovementsWithLengthPenalty(string s, string t)
        {
            s = s.Trim().ToLower();
            t = t.Trim().ToLower();

            int sLen = s.Length;
            int tLen = t.Length;

            string shortestString;
            string longestString;

            if (sLen > tLen)
            {
                shortestString = t;
                longestString = s;
            }
            else
            {
                shortestString = s;
                longestString = t;
            }

            int shortestStringLen = shortestString.Length;

            List<string> substrings = new List<string>();

            while (shortestString.Length > 0)
            {
                int start;
                int end;

                if (TryFindLongestCommonSubstringSlice(shortestString, longestString, out start, out end))
                {
                    substrings.Add(shortestString.Substring(start, end - start + 1));
                    shortestString = shortestString.Remove(start, end - start + 1).Trim();
                }
                else
                {
                    substrings.Add(shortestString);
                    break;
                }
            }

            if (substrings.Count == 1)
            {
                return 0;
            }

            return substrings.Count / (float)shortestStringLen;
        }
    }
}
