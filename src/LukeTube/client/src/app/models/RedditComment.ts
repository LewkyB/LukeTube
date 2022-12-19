import { VideoModel } from "./VideoModel";

export class RedditComment {
  subreddit: string;
  YoutubeId: string;
  score: number;
  createdUTC: Date;
  retrievedUTC: Date;
  permalink: string;
  videoModel: VideoModel;
}
