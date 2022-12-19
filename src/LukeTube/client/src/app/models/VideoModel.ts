export class VideoModel {
  youtubeId: string;
  url: string;
  title: string;
  author: Author;
  uploadDate: Date;
  description: string;
  duration: Date;
  thumbnails: Thumbnail[];
  keywords: string[];
  engagement: Engagement;
}

export class Author {
  channelId: string;
  channelUrl: string;
  channelTitle: string;
}

export class Thumbnail {
  url: string;
  width: number;
  height: number;
  area: number;
}

export class Engagement {
  viewCount: number;
  likeCount: number;
  dislikeCount: number;
  averageRating: number;
}
