namespace LukeTube.Models.Pushshift
{
    public class SearchOptions
    {

        /// <summary>
        /// String / Quoted String for phrases
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Comma-delimited base36 ids
        /// </summary>
        public string[] Ids { get; set; }

        /// <summary>
        /// Integer &lt;= 1000
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Response fields
        /// </summary>
        public string[] Fields { get; set; }

        public Sort Sort { get; set; }

        public SortType SortType { get; set; }

        public Aggs[] Aggs { get; set; }

        public string Author { get; set; }

        public string Subreddit { get; set; }

        /// <summary>
        /// Epoch value or Integer + "s,m,h,d" (i.e. 30d for 30 days)
        /// </summary>
        public string After { get; set; }

        /// <summary>
        /// Epoch value or Integer + "s,m,h,d" (i.e. 30d for 30 days)
        /// </summary>
        public string Before { get; set; }

        public Frequency Frequency { get; set; }


        public virtual List<string> ToArgs()
        {
            var args = new List<string>();

            if (!string.IsNullOrEmpty(Query))
                args.Add($"q={Query}");

            if (Ids?.Length > 0)
                args.Add($"ids={string.Join(",", Ids)}");

            args.Add($"size={(Size <= 0 ? 1 : Size > 1000 ? 1000 : Size)}");

            if (Fields?.Length > 0)
                args.Add($"fields={string.Join(",", Fields)}");

            args.Add($"sort={Sort.ToString().ToLower()}");
            args.Add($"sort_type={SortType.ToString().ToLower()}");

            if (Aggs?.Length > 0)
                args.Add($"aggs={string.Join(",", Aggs.Select(agg => agg.ToString().ToLower()))}");

            if (!string.IsNullOrEmpty(Author))
                args.Add($"author={Author}");

            if (!string.IsNullOrEmpty(Subreddit))
                args.Add($"subreddit={Subreddit}");

            if (!string.IsNullOrEmpty(After))
                args.Add($"after={After}");

            if (!string.IsNullOrEmpty(Before))
                args.Add($"before={Before}");

            if (Frequency != Frequency.None)
                args.Add($"frequency={Frequency.ToString().ToLower()}");

            return args;
        }
    }

    public enum Sort
    {
        Desc,
        Asc
    }

    public enum SortType
    {
        Created_Utc,
        Score,
        Num_Comments
    }

    public enum Aggs
    {
        Author,
        Link_Id,
        Created_Utc,
        Subreddit
    }

    public enum Frequency
    {
        None,
        Second,
        Minute,
        Hour,
        Day
    }
}
