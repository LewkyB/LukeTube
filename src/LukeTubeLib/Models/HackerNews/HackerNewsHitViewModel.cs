namespace LukeTubeLib.Models.HackerNews;

public class HackerNewsHitViewModel
{
    public string Author { get; set; }
    public string Url { get; set; }
    public int? Points { get; set; }
    public string YoutubeId { get; set; }
    public VideoViewModel VideoModel { get; set; }
}