import { Component, OnInit } from "@angular/core";
import { Subreddit } from "../services/subreddit.service";
import { ActivatedRoute } from "@angular/router";
import { RedditComment } from "../models/RedditComment";

@Component({
  selector: "app-subreddit-video",
  templateUrl: `./subreddit-video.component.html`,
  styleUrls: ["./subreddit-video.component.css"],
})
export class SubredditVideoComponent implements OnInit {
  throttle = 0;
  distance = 2;
  page: number = 0;

  subredditName: string = "";

  redditComments: RedditComment[] = [];

  // toggles
  sortScoreToggled: boolean = false;
  sortDateToggled: boolean = false;

  constructor(public subreddit: Subreddit, private route: ActivatedRoute) {}

  ngOnInit(): void {
    // necessary for youtube-player to work
    const tag = document.createElement("script");
    tag.src = "https://www.youtube.com/iframe_api";
    document.body.appendChild(tag);

    // get subreddit name to show in top left
    this.subredditName = this.route.snapshot.params["subredditName"];

    this.subreddit
      .loadCommentsPaged(this.subredditName, this.page) // load the comments from the subreddit in the route
      .then((result: RedditComment[]) => {
        this.redditComments.push(...result);
        this.page += 1;
      });
  }

  sortDate() {
    if (!this.sortDateToggled) {
      this.sortDateToggled = true;

      // sort descending
      this.redditComments.sort((a, b) =>
        a.createdUTC.toString() > b.createdUTC.toString()
          ? -1
          : a.createdUTC.toString() < b.createdUTC.toString()
          ? 1
          : 0
      );
    } else {
      this.sortDateToggled = false;
      // sort ascending
      this.redditComments.sort((a, b) =>
        a.createdUTC.toString() < b.createdUTC.toString()
          ? -1
          : a.createdUTC.toString() > b.createdUTC.toString()
          ? 1
          : 0
      );
    }
  }

  sortScore() {
    if (!this.sortScoreToggled) {
      this.sortScoreToggled = true;
      // sort ascending
      this.redditComments.sort((a, b) =>
        a.score < b.score ? -1 : a.score > b.score ? 1 : 0
      );
    } else {
      this.sortScoreToggled = false;
      // sort descending
      this.redditComments.sort((a, b) =>
        a.score > b.score ? -1 : a.score < b.score ? 1 : 0
      );
    }
  }

  onScroll() {
    this.subreddit
      .loadCommentsPaged(this.route.snapshot.params["subredditName"], this.page)
      .then((result: RedditComment[]) => {
        this.redditComments.push(...result);
        this.page += 1;
      });
  }
}
