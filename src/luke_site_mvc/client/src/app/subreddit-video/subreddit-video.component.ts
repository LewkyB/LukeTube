import { Component, OnInit } from "@angular/core";
import { Subreddit } from "../services/subreddit.service";
import { PageEvent } from "@angular/material/paginator";
import { ActivatedRoute } from "@angular/router";
import { RedditComment } from "../shared/RedditComment";
import { Observable } from "rxjs/internal/Observable";

@Component({
  selector: "app-subreddit-video",
  template: `
    <!-- <hr>
    <mat-grid-list cols="3" rowHeight="1:1">
      <mat-grid-tile *ngFor="let redditComment of subreddit.redditComments">
        <youtube-player class="video-container" videoId={{redditComment.youtubeLinkId}}></youtube-player>
        <mat-card>{{redditComment.score}}</mat-card>
      </mat-grid-tile>
    </mat-grid-list>
    <hr /> -->
    <mat-paginator
      [length]="length"
      [pageSize]="pageSize"
      [pageSizeOptions]="pageSizeOptions"
      (page)="pageEvent = loadPage($event)"
    >
    </mat-paginator>

    <div class="videos">
      <div
        *ngFor="let redditComment of redditComments | slice: sliceLow:sliceHigh"
      >
        <youtube-player
          videoId="{{ redditComment.youtubeLinkId }}"
        ></youtube-player>
        <div>
          <button>
            <a href="https://www.reddit.com{{ redditComment.permalink }}"
              >PERMALINK</a
            >
          </button>
          <p>score: {{ redditComment.score }}</p>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      .videos {
        display: grid;
        grid-template-columns: repeat(2, 1fr);
        grid-gap: 5px;
        justify-items: center;
      }
    `,
  ],
})
export class SubredditVideoComponent implements OnInit {
  length: number = 0;
  pageSize: number = 4; // default items per page
  pageSizeOptions: number[] = [2, 4, 8, 16];
  pageEvent?: PageEvent;
  sliceLow: number = 0;
  sliceHigh: number = 0;

  redditComments: RedditComment[] = [];

  constructor(public subreddit: Subreddit, private route: ActivatedRoute) {}

  ngOnInit(): void {
    // necessary for youtube-player to work
    const tag = document.createElement("script");
    tag.src = "https://www.youtube.com/iframe_api";
    document.body.appendChild(tag);

    // kick off the paginator
    this.loadPage(undefined);
  }

  // This method will be called whenever we switch a page, add or delete item, and init at the begining
  loadPage(event?: PageEvent) {
    this.subreddit
      .loadComments(this.route.snapshot.params["subredditName"])
      .subscribe((projects) => {
        this.redditComments = projects; // assign data to variable on template

        // sort from highest score to lowest
        this.redditComments.sort((a, b) => a.score > b.score ? -1 : a.score < b.score ? 1 : 0);
        
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
