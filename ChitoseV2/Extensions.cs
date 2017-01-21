using System.IO;
using System.Linq;

namespace ChitoseV2
{
    internal static class Extensions
    {
        public static string CleanFileName(string filename)
        {
            return Path.GetInvalidFileNameChars().Aggregate(filename, (current, c) => current.Replace(c.ToString(), string.Empty));
        }
    }
}