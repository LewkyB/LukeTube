namespace LukeTubeLib.Models.HackerNews;

public record HackerNewsSearchOptions
{
    public string Query { get; init; }
    public string[] Ids { get; set; }
    public string Tags { get; set; }
    public int HitsPerPage { get; init; }
    public string After { get; set; }
    public string Before { get; set; }
    public int Page { get; set; }

    public virtual List<string> ToArgs()
    {
        var args = new List<string>();

        if (!string.IsNullOrEmpty(Query))
            args.Add($"query={Query}");

        if (Ids?.Length > 0)
            args.Add($"ids={string.Join(",", Ids)}");

        if (!string.IsNullOrEmpty(Tags))
            args.Add($"tags={Tags}");

        // TODO: is my chevron right for after and before?
        if (!string.IsNullOrEmpty(After) || !string.IsNullOrEmpty(Before))
            args.Add($"numericFilters=created_at_i>{Before},created_at_i<{After}");

        if (Page >= 1)
            args.Add($"page={Page}");

        return args;
    }
}