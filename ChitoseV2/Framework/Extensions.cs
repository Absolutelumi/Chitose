using Discord;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace ChitoseV2
{
    internal static class Extensions
    {
        public static readonly Random rng = new Random();

        /// <summary>
        /// Calculate percentage similarity of two strings <param name="source">Source String to
        /// Compare with</param><param name="target">Targeted String to
        /// Compare</param><returns>Return Similarity between two strings from 0 to 1.0</returns>
        /// </summary>
        public static double CalculateSimilarity(string source, string target)
        {
            if ((source == null) || (target == null))
                return 0.0;
            if ((source.Length == 0) || (target.Length == 0))
                return 0.0;
            if (source == target)
                return 1.0;

            int stepsToSame = ComputeLevenshteinDistance(source, target);
            return 1.0 - ((double)stepsToSame / Math.Max(source.Length, target.Length));
        }

        public static string CleanFileName(string filename)
        {
            return Path.GetInvalidFileNameChars().Aggregate(filename, (current, c) => current.Replace(c.ToString(), string.Empty));
        }

        /// <summary>
        /// Returns the number of steps required to transform the source string into the target string.
        /// </summary>
        public static int ComputeLevenshteinDistance(string source, string target)
        {
            if ((source == null) || (target == null))
                return 0;
            if ((source.Length == 0) || (target.Length == 0))
                return 0;
            if (source == target)
                return source.Length;

            int sourceWordCount = source.Length;
            int targetWordCount = target.Length;

            // Step 1
            if (sourceWordCount == 0)
                return targetWordCount;
            if (targetWordCount == 0)
                return sourceWordCount;

            int[,] distance = new int[sourceWordCount + 1, targetWordCount + 1];

            // Step 2
            for (int i = 0; i <= sourceWordCount; i++)
            {
                distance[i, 0] = i;
            }
            for (int j = 0; j <= targetWordCount; j++)
            {
                distance[0, j] = j;
            }

            for (int i = 1; i <= sourceWordCount; i++)
            {
                for (int j = 1; j <= targetWordCount; j++)
                {
                    // Step 3
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                    // Step 4
                    distance[i, j] = Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1, distance[i - 1, j - 1] + cost);
                }
            }
            return distance[sourceWordCount, targetWordCount];
        }

        public static Stream GetHttpStream(Uri uri)
        {
            HttpWebRequest getRequest = WebRequest.CreateHttp(uri);
            getRequest.Method = "GET";
            WebResponse response = getRequest.GetResponse();
            return response.GetResponseStream();
        }

        public static string HtmlDecode(this string text)
        {
            return HttpUtility.HtmlDecode(text);
        }

        public static T Max<T>(params T[] values)
        {
            return values.Max();
        }

        public static T Min<T>(params T[] values)
        {
            return values.Min();
        }

        public static T Random<T>(this T[] array)
        {
            return array[rng.Next(array.Length)];
        }

        public static string ReadString(this Stream stream)
        {
            return new StreamReader(stream).ReadToEnd();
        }

        public static async Task SendFile(this Channel channel, Uri url)
        {
            await channel.SendFile(Path.GetFileName(url.LocalPath), GetHttpStream(url));
        }

        public static bool StartsWithVowelSound(this int number)
        {
            if (number <= 0)
                return false;
            while (number >= 1000)
            {
                number /= 1000;
            }
            return number.ToString()[0] == '8' || number == 11;
        }

        public static string ToTitleCase(this string text)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text);
        }

        public static string UrlEncode(this string text)
        {
            return HttpUtility.UrlEncode(text);
        }
    }
}