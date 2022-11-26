namespace LukeTubeLib.Models.HackerNews;

public record HackerNewsSearchOptions
{
    /// <summary>
    /// String / Quoted String for phrases
    /// </summary>
    public string Query { get; init; }

    /// <summary>
    /// Comma-delimited base36 ids
    /// </summary>
    public string[] Ids { get; set; }

    /// <summary>
    /// Integer &lt;= 1000
    /// </summary>

    /// <summary>
    /// Response fields
    /// </summary>
    // public string[] Fields { get; set; }

    // public Sort Sort { get; set; }
    //
    // public SortType SortType { get; set; }

    // public Aggs[] Aggs { get; set; }

    // public string Author { get; set; }

    public List<string> Tags { get; set; }

    // numeric filters
    public int HitsPerPage { get; init; }
    public string After { get; set; }

    public string Before { get; set; }

    // public Frequency Frequency { get; set; }


    public virtual List<string> ToArgs()
    {
        var args = new List<string>();

        if (!string.IsNullOrEmpty(Query))
            args.Add($"query={Query}");

        if (Ids?.Length > 0)
            args.Add($"ids={string.Join(",", Ids)}");

        // args.Add($"size={(HitsPerPage <= 0 ? 1 : HitsPerPage > 1000 ? 1000 : HitsPerPage)}");

        // if (Fields?.Length > 0)
        //     args.Add($"fields={string.Join(",", Fields)}");

        // args.Add($"sort={Sort.ToString().ToLower()}");
        // args.Add($"sort_type={SortType.ToString().ToLower()}");

        // if (Aggs?.Length > 0)
        //     args.Add($"aggs={string.Join(",", Aggs.Select(agg => agg.ToString().ToLower()))}");

        // if (!string.IsNullOrEmpty(Author))
        //     args.Add($"author={Author}");

        // if (Tags.Count > 0)
        //     args.Add($"tags={string.Join(",", Tags)}");

        // TODO: is my chevron right for after and before?
        if (!string.IsNullOrEmpty(After) || !string.IsNullOrEmpty(Before))
            args.Add($"numericFilters=created_at_i>{Before},created_at_i>{After}");
        // if (Frequency != Frequency.None)
        //     args.Add($"frequency={Frequency.ToString().ToLower()}");

        return args;
    }
}

// public enum Sort
// {
//     Desc,
//     Asc
// }
//
// public enum SortType
// {
//     Created_Utc,
//     Score,
//     Num_Comments
// }
//
// public enum Aggs
// {
//     Author,
//     Link_Id,
//     Created_Utc,
//     Subreddit
// }
//
// public enum Frequency
// {
//     None,
//     Second,
//     Minute,
//     Hour,
//     Day
// }