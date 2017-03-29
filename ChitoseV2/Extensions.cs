using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;

namespace ChitoseV2
{
    internal static class Extensions
    {
        public static readonly Random rng = new Random();

        public static string CleanFileName(string filename)
        {
            return Path.GetInvalidFileNameChars().Aggregate(filename, (current, c) => current.Replace(c.ToString(), string.Empty));
        }

        public static T Random<T>(this T[] array)
        {
            return array[rng.Next(array.Length)];
        }

        public static string ReadString(this Stream stream)
        {
            return new StreamReader(stream).ReadToEnd();
        }

        public static string ToTitleCase(this string text)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text);
        }

        public static string DownloadFile(string url)
        {
            string FilePath = Chitose.TempDirectory + "Temp" + ".png"; 

            using (WebClient downloadclient = new WebClient())
            {
                downloadclient.DownloadFile(new Uri(url), FilePath);
            }

            return FilePath; 
        }
    }
}