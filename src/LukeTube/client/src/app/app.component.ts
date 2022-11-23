import { Component } from "@angular/core";
import { IYoutubeThumbnailDetail } from "./models-yt/youtube-common.model";
import { IYoutubeSearchItem, IYoutubeSearchId, IYoutubeSearchSnippet, IYoutubeSearchThumbnail } from './models-yt/youtube-search-list.model'

@Component({
  selector: "app-root",
  templateUrl: "app.component.html",
  styleUrls: ["app.component.css"]
})
export class AppComponent {
  title = "LukeTube";

  public searchItem!: IYoutubeSearchItem;
  public searchId!: IYoutubeSearchId;
  public snippet!: IYoutubeSearchSnippet;
  public thumbnail!: IYoutubeSearchThumbnail;
  public thumbnailDetail!: IYoutubeThumbnailDetail;
  
  ngOnInit(): void {
    this.thumbnailDetail.url = "https://i.ytimg.com/vi/9yjZpBq1XBE/hqdefault.jpg?sqp=-oaymwEbCKgBEF5IVfKriqkDDggBFQAAiEIYAXABwAEG&rs=AOn4CLB4JupylgpEp9M1kBMJQcV4PogSDw"
    this.thumbnailDetail.height = 300;
    this.thumbnailDetail.width = 400;
    this.thumbnail.default = this.thumbnailDetail;
    this.searchId.videoId = "2aTULNg5F1E"
    this.searchItem.id = this.searchId;
    this.snippet.thumbnails = this.thumbnail
    this.searchItem.snippet = this.snippet;
  }
}

