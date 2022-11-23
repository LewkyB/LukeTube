using System.Net.Http.Json;
using LukeTubeLib.Models.Pushshift;
using Xunit;

// using OpenQA.Selenium;
// using OpenQA.Selenium.Chrome;
// using OpenQA.Selenium.Support.UI;

namespace LukeTube.Tests.IntegrationTests;

public static class LukeTubeTest
{
    public sealed class Api : IClassFixture<LukeTubeContainer>
    {
        private readonly LukeTubeContainer _lukeTubeContainer;

        public Api(LukeTubeContainer lukeTubeContainer)
        {
            _lukeTubeContainer = lukeTubeContainer;
            _lukeTubeContainer.SetBackendBaseAddress();
        }

        [Fact]
        [Trait("Category", nameof(Api))]
        public async Task Get_SubredditNames_ReturnsSubredditList()
        {
            const string path = "http://localhost:82/api/pushshift/subreddit-names";

            // TODO: anyway to get rid of this delay
            // PushshiftBackgroundService needs time to gather data
            await Task.Delay(TimeSpan.FromSeconds(20));

            var response = await _lukeTubeContainer.GetFromJsonAsync<IReadOnlyList<string>>(path)
                .ConfigureAwait(false);

            Assert.True(response.Count > 0);
        }

        [Fact]
        [Trait("Category", nameof(Api))]
        public async Task Get_All_Reddit_Comments()
        {
            const string path = "http://localhost:82/api/pushshift/get-all-reddit-comments";

            // TODO: anyway to get rid of this delay
            // PushshiftBackgroundService needs time to gather data
            await Task.Delay(TimeSpan.FromSeconds(20));

            var response = await _lukeTubeContainer.GetFromJsonAsync<IReadOnlyList<RedditComment>>(path)
                .ConfigureAwait(false);

            Assert.True(response.Count > 0);
        }
    }

    // public sealed class Web : IClassFixture<WeatherForecastContainer>
    // {
    //   private static readonly ChromeOptions ChromeOptions = new();
    //
    //   private readonly WeatherForecastContainer _weatherForecastContainer;
    //
    //   static Web()
    //   {
    //     ChromeOptions.AddArgument("headless");
    //     ChromeOptions.AddArgument("ignore-certificate-errors");
    //   }
    //
    //   public Web(WeatherForecastContainer weatherForecastContainer)
    //   {
    //     _weatherForecastContainer = weatherForecastContainer;
    //     _weatherForecastContainer.SetBaseAddress();
    //   }
    //
    //   [Fact]
    //   [Trait("Category", nameof(Web))]
    //   public void Get_WeatherForecast_ReturnsSevenDays()
    //   {
    //     // Given
    //     string ScreenshotFileName() => $"{nameof(Get_WeatherForecast_ReturnsSevenDays)}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.png";
    //
    //     using var chrome = new ChromeDriver(ChromeOptions);
    //
    //     // When
    //     chrome.Navigate().GoToUrl(_weatherForecastContainer.BaseAddress);
    //
    //     chrome.GetScreenshot().SaveAsFile(Path.Combine(CommonDirectoryPath.GetSolutionDirectory().DirectoryPath, ScreenshotFileName()));
    //
    //     chrome.FindElement(By.TagName("fluent-button")).Click();
    //
    //     var wait = new WebDriverWait(chrome, TimeSpan.FromSeconds(10));
    //     wait.Until(webDriver => 1.Equals(webDriver.FindElements(By.TagName("span")).Count));
    //
    //     chrome.GetScreenshot().SaveAsFile(Path.Combine(CommonDirectoryPath.GetSolutionDirectory().DirectoryPath, ScreenshotFileName()));
    //
    //     // Then
    //     Assert.Equal(7, int.Parse(chrome.FindElement(By.TagName("span")).Text, NumberStyles.Integer, CultureInfo.InvariantCulture));
    //   }
    // }
}