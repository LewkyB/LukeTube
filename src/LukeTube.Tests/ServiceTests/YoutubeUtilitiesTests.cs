using LukeTubeLib.Services;
using Xunit;

namespace LukeTube.Tests.ServiceTests;

public class YoutubeUtilitiesTests
{
    [Theory]
    [InlineData(@"https://www.youtube.com/watch?v=fe4Yf-0Wm4U", 11)]
    public void FindYoutubeIdTest_ShouldReturnCorrectSizeId(string youtubeLink, int expectedLength)
    {
        var result = YoutubeUtilities.FindYoutubeId(youtubeLink);
        Assert.True(result.Count is 1);
        Assert.Equal(expectedLength, result[0].Length);
    }

    [Theory]
    [InlineData(@"https://www.youtube.com/watch?v=fe4Yf-0Wm4U https://www.youtube.com/watch?v=fg6pf-0Wm4U", 2, 11)]
    public void FindYoutubeIdTest_ShouldReturnMultipleIds(string body, int expectedCount, int expectedLength)
    {
        var result = YoutubeUtilities.FindYoutubeId(body);
        Assert.Equal(expectedCount, result.Count);

        foreach (var res in result)
        {
            Assert.Equal(expectedLength, res.Length);
        }
    }

    [Fact]
    public void FindYoutubeIdTest_EmptyString()
    {
        var result = YoutubeUtilities.FindYoutubeId(string.Empty);
        Assert.Empty(result);
    }

    [Theory]
    [InlineData("youtube.com/watch?v=yIVRs6YSbOM", "yIVRs6YSbOM")]
    [InlineData("youtu.be/yIVRs6YSbOM", "yIVRs6YSbOM")]
    [InlineData("youtube.com/embed/yIVRs6YSbOM", "yIVRs6YSbOM")]
    [InlineData("youtube.com/shorts/sKL1vjP0tIo", "sKL1vjP0tIo")]
    public void Video_ID_can_be_parsed_from_a_URL_string(string videoUrl, string expectedVideoId)
    {
        var result = YoutubeUtilities.FindYoutubeId(videoUrl);
        Assert.True(result.Count is 1);
        Assert.Equal(expectedVideoId, result[0]);
    }

    [Theory]
    [InlineData("9bZkp7q19f0")]
    [InlineData("_kmeFXjjGfk")]
    [InlineData("AI7ULzgf8RU")]
    public void Video_ID_can_be_parsed_from_an_ID_string(string videoId)
    {
        var result = YoutubeUtilities.FindYoutubeId(videoId);
        Assert.True(result.Count is 1);
        Assert.Equal(videoId, result[0]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("pI2I2zqzeK")]
    [InlineData("pI2I2z zeKg")]
    [InlineData("youtube.com/xxx?v=pI2I2zqzeKg")]
    [InlineData("youtu.be/watch?v=xxx")]
    [InlineData("youtube.com/embed/")]
    public void Video_ID_cannot_be_parsed_from_an_invalid_string(string videoId)
    {
        var result = YoutubeUtilities.FindYoutubeId(videoId);

        Assert.True(result.Count is 0);
    }
}