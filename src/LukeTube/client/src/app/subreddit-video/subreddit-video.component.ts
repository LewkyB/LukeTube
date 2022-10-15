import { Component, OnInit } from "@angular/core";
import { Subreddit } from "../services/subreddit.service";
import { PageEvent } from "@angular/material/paginator";
import { ActivatedRoute } from "@angular/router";
import { RedditComment } from "../models/RedditComment";
import { Observable } from "rxjs/internal/Observable";

@Component({
  selector: 'app-subreddit-video',
  templateUrl: `./subreddit-video.component.html`,
  styleUrls: ['./subreddit-video.component.css']
})
export class SubredditVideoComponent implements OnInit {
  length: number = 0;
  pageSize: number = 4; // default items per page
  pageSizeOptions: number[] = [2, 4, 8, 16];
  pageEvent?: PageEvent;
  sliceLow: number = 0;
  sliceHigh: number = 0;

  subredditName?: string;

  redditComments: RedditComment[] = [];

  // toggles
  sortScoreToggled?: boolean = false;
  sortDateToggled?: boolean = false;

  constructor(public subreddit: Subreddit, private route: ActivatedRoute) {}

  ngOnInit(): void {
    // necessary for youtube-player to work
    const tag = document.createElement("script");
    tag.src = "https://www.youtube.com/iframe_api";
    document.body.appendChild(tag);

    // get subreddit name to show in top left
    this.subredditName = this.route.snapshot.params["subredditName"];

    // kick off the paginator
    this.loadPage(undefined);
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

  // This method will be called whenever we switch a page, add or delete item, and init at the begining
  loadPage(event?: PageEvent) {
    this.subreddit
      .loadComments(this.route.snapshot.params["subredditName"]) // load the comments from the subreddit in the route
      .subscribe((projects) => {
        this.redditComments = projects; // assign data to variable on template

        // set default sorting - currently by score descending
        this.redditComments.sort((a, b) =>
          a.score > b.score ? -1 : a.score < b.score ? 1 : 0
        );

        this.length = projects.length;

        if (event) {
          this.sliceLow = event.pageIndex * event.pageSize;
          this.sliceHigh = this.sliceLow + event.pageSize;
        } else {
          this.sliceLow = 0;
          this.sliceHigh = this.pageSize;
        }
      });

    return event;
  }
}
