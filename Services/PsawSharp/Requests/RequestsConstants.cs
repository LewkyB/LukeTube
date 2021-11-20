namespace PsawSharp.Requests
{
    public static class RequestsConstants
    {

        public const string BaseAddress = "https://api.pushshift.io";
        public const string SearchRoute = "reddit/{0}/search";
        public const string CommentIdsRoute = "reddit/submission/comment_ids/{0}";
        public const int MaxRequestsPerMinute = 60;

    }
}
