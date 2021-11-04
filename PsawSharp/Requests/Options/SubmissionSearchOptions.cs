using System.Collections.Generic;

namespace PsawSharp.Requests.Options
{
    public class SubmissionSearchOptions : SearchOptions
    {

        #region Properties

        public string QueryNot { get; set; }

        public string Title { get; set; }

        public string TitleNot { get; set; }

        public string Selftext { get; set; }

        public string SelftextNot { get; set; }

        /// <summary>
        /// Integer or &gt; x or &lt; x, (i.e. score=>100 or score=&lt;25)
        /// </summary>
        public string Score { get; set; }

        /// <summary>
        /// Integer or &gt; x or &lt; x, (i.e. num_comments=>100)
        /// </summary>
        public string NumComments { get; set; }

        public bool? Over18 { get; set; }

        public bool? IsVideo { get; set; }

        public bool? Locked { get; set; }

        public bool? Stickied { get; set; }

        public bool? Spoiler { get; set; }

        public bool? ContestMode { get; set; }

        #endregion

        #region Public Methods

        public override List<string> ToArgs()
        {
            var args = base.ToArgs();

            if (!string.IsNullOrEmpty(QueryNot))
                args.Add($"q:not={QueryNot}");

            if (!string.IsNullOrEmpty(Title))
                args.Add($"title={Title}");

            if (!string.IsNullOrEmpty(TitleNot))
                args.Add($"title:not={TitleNot}");

            if (!string.IsNullOrEmpty(Selftext))
                args.Add($"selftext={Selftext}");

            if (!string.IsNullOrEmpty(SelftextNot))
                args.Add($"selftext:not={SelftextNot}");

            if (!string.IsNullOrEmpty(Score))
                args.Add($"score={Score}");

            if (!string.IsNullOrEmpty(NumComments))
                args.Add($"num_comments={NumComments}");

            if (Over18.HasValue)
                args.Add($"over_18={Over18.ToString().ToLower()}");

            if (IsVideo.HasValue)
                args.Add($"is_video={IsVideo.ToString().ToLower()}");

            if (Locked.HasValue)
                args.Add($"locked={Locked.ToString().ToLower()}");

            if (Stickied.HasValue)
                args.Add($"stickied={Stickied.ToString().ToLower()}");

            if (Spoiler.HasValue)
                args.Add($"spoiler={Spoiler.ToString().ToLower()}");

            if (ContestMode.HasValue)
                args.Add($"contest_mode={ContestMode.ToString().ToLower()}");

            return args;
        }

        #endregion

    }
}
