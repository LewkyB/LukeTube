using PsawSharp.Requests.Options;
using Xunit;

namespace PsawSharp.Tests
{
    public class SearchOptionsTests
    {

        [Fact]
        public void SearchOptionsToArgs1()
        {
            var options = new SearchOptions
            {
                Query = "games",
                Size = 2500
            };

            var args = options.ToArgs();
            Assert.Equal(4, args.Count);
            Assert.Equal("size=1000", args[1]);
        }

        [Fact]
        public void SearchOptionsToArgs2()
        {
            var options = new SubmissionSearchOptions
            {
                Query = "game",
                QueryNot = "video",
                Stickied = true,
                NumComments = ">100",
                Fields = new[] { "author", "title", "permalink" },
                Size = 2500
            };

            var args = options.ToArgs();
            Assert.Equal(8, args.Count);
            Assert.Equal("size=1000", args[1]);
            Assert.Equal($"fields={string.Join(",", options.Fields)}", args[2]);
        }

    }
}
