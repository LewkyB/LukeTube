using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using YoutubeExplode.Common;

namespace YoutubeExplode.Videos;

/// <summary>
/// Metadata associated with a YouTube video.
/// </summary>
public class Video : IVideo
{
    /// <inheritdoc />
    public VideoId Id { get; set; }

    /// <inheritdoc />
    public string Url => $"https://www.youtube.com/watch?v={Id}";

    /// <inheritdoc />
    public string Title { get; set; }

    /// <inheritdoc />
    public Author Author { get; set; }

    /// <summary>
    /// Video upload date.
    /// </summary>
    public DateTimeOffset UploadDate { get; set; }

    /// <summary>
    /// Video description.
    /// </summary>
    public string Description { get; set; }

    /// <inheritdoc />
    public TimeSpan? Duration { get; set; }

    /// <inheritdoc />
    public IReadOnlyList<Thumbnail> Thumbnails { get; set; }

    /// <summary>
    /// Available search keywords for the video.
    /// </summary>
    public IReadOnlyList<string> Keywords { get; set; }

    /// <summary>
    /// Engagement statistics for the video.
    /// </summary>
    public Engagement Engagement { get; set; }

    /// <summary>
    /// Initializes an instance of <see cref="Video" />.
    /// </summary>
    public Video(
        VideoId id,
        string title,
        Author author,
        DateTimeOffset uploadDate,
        string description,
        TimeSpan? duration,
        IReadOnlyList<Thumbnail> thumbnails,
        IReadOnlyList<string> keywords,
        Engagement engagement)
    {
        Id = id;
        Title = title;
        Author = author;
        UploadDate = uploadDate;
        Description = description;
        Duration = duration;
        Thumbnails = thumbnails;
        Keywords = keywords;
        Engagement = engagement;
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => $"Video ({Title})";
}