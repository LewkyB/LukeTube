using Microsoft.EntityFrameworkCore;

namespace LukeTubeLib.Models.HackerNews;

[PrimaryKey(nameof(HackerNewsHitId))]
public class HackerNewsHit
{
    public int HackerNewsHitId { get; set; }
    public string Author { get; set; }
    public string Url { get; set; }
    public int Points { get; set; }
    public string YoutubeId { get; set; }
    public VideoModel VideoModel { get; set; }
}