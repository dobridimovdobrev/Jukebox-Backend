namespace Jukebox_Backend.DbHelpers
{
    public class SongTitleHelper
    {
        // clear song title 
        public static string Clean(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return string.Empty;

            // remove these symbols from song titles
            var cleaned = title
                .Replace("\"", "")
                .Replace("'", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("/", " ")
                .Replace("  ", " ")
                .Trim();

            return cleaned;
        }

        // normalize song title for better duplicate detection
        public static string Normalize(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return string.Empty;

            var normalized = Clean(title).ToLowerInvariant();

            // remove some noise words
            var noise = new[] { "remastered", "remaster", "live", "version", "edit", "remix", "mono", "stereo" };
            foreach (var word in noise)
            {
                normalized = normalized.Replace(word, "");
            }

            // collapse multiple spaces into one
            while (normalized.Contains("  "))
                normalized = normalized.Replace("  ", " ");

            return normalized.Trim();
        }

        // true if two song title are like duplicating
        public static bool IsDuplicate(string title1, string title2)
        {
            var n1 = Normalize(title1);
            var n2 = Normalize(title2);

            // when titles are equal
            if (n1 == n2) return true;

            // when one title contains the other 
            if (n1.Contains(n2) || n2.Contains(n1)) return true;

            // when words that are equal in the song titles
            var words1 = n1.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var words2 = n2.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var commonWords = words1.Intersect(words2).Count();
            var minWords = Math.Min(words1.Length, words2.Length);

            // the condition of song title to ignore duplicates
            if (minWords >= 2 && commonWords >= Math.Ceiling(minWords * 0.7))
                return true;

            return false;
        }
    }
}
