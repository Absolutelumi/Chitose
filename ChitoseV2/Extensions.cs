using System;
using System.IO;
using System.Linq;

namespace ChitoseV2
{
    internal static class Extensions
    {
        public static readonly Random rng = new Random();

        public static T Random<T>(this T[] array)
        {
            return array[Extensions.rng.Next(array.Length)];
        }

        public static string CleanFileName(string filename)
        {
            return Path.GetInvalidFileNameChars().Aggregate(filename, (current, c) => current.Replace(c.ToString(), string.Empty));
        }

        public static string ReadString(this Stream stream)
        {
            return new StreamReader(stream).ReadToEnd();
        }
    }
}