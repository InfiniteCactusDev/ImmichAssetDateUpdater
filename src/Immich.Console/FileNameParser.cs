using System.Text.RegularExpressions;

namespace Immich.Console
{
    public class FileNameParser(string[] patterns)
    {
        private readonly Regex[] _patterns = patterns?.Select(p => new Regex(p)).ToArray() ?? [];

        public List<ParseResult> Parse(string fileName)
        {
            var results = new List<ParseResult>();
            foreach (var pattern in _patterns)
            {
                var match = pattern.Match(fileName);
                if (!match.Success)
                    continue;

                var result = new ParseResult();
                if (match.Groups["year"].Success)
                    result.Year = int.Parse(match.Groups["year"].Value);
                if (match.Groups["month"].Success)
                    result.Month = int.Parse(match.Groups["month"].Value);
                if (match.Groups["day"].Success)
                    result.Day = int.Parse(match.Groups["day"].Value);
                if (match.Groups["hour"].Success)
                    result.Hour = int.Parse(match.Groups["hour"].Value);
                if (match.Groups["minute"].Success)
                    result.Minute = int.Parse(match.Groups["minute"].Value);
                if (match.Groups["second"].Success)
                    result.Second = int.Parse(match.Groups["second"].Value);

                results.Add(result);
            }

            return results;
        }
    }

    public class ParseResult
    {
        public int? Year { get; set; }
        public int? Month { get; set; }
        public int? Day { get; set; }
        public int? Hour { get; set; }
        public int? Minute { get; set; }
        public int? Second { get; set; }

        public int Score
        {
            get
            {
                var score = 0;
                if (!Year.HasValue)
                    return score;
                score++;
                if (!Month.HasValue)
                    return score;
                score++;
                if (!Day.HasValue)
                    return score;
                score++;
                if (!Hour.HasValue)
                    return score;
                score++;
                if (!Minute.HasValue)
                    return score;
                score++;
                if (!Second.HasValue)
                    return score;
                score++;
                return score;
            }
        }

        public DateTime ToDateTime()
        {
            return new DateTime(
                Year ?? 1900,
                Month ?? 1,
                Day ?? 1,
                Hour ?? 12,
                Minute ?? 0,
                Second ?? 0
            );
        }
    }
}