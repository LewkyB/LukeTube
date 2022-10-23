import { __decorate } from "tslib";
import { Component } from "@angular/core";
let SubredditVideoComponent = class SubredditVideoComponent {
    constructor(subreddit, route) {
        this.subreddit = subreddit;
        this.route = route;
        this.length = 0;
        this.pageSize = 4; // default items per page
        this.pageSizeOptions = [2, 4, 8, 16];
        this.sliceLow = 0;
        this.sliceHigh = 0;
        this.redditComments = [];
        // toggles
        this.sortScoreToggled = false;
        this.sortDateToggled = false;
    }
    ngOnInit() {
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
            this.redditComments.sort((a, b) => a.createdUTC.toString() > b.createdUTC.toString() ? -1 : a.createdUTC.toString() < b.createdUTC.toString() ? 1 : 0);
        }
        else {
            this.sortDateToggled = false;
            // sort ascending
            this.redditComments.sort((a, b) => a.createdUTC.toString() < b.createdUTC.toString() ? -1 : a.createdUTC.toString() > b.createdUTC.toString() ? 1 : 0);
        }
    }
    sortScore() {
        if (!this.sortScoreToggled) {
            this.sortScoreToggled = true;
            // sort ascending
            this.redditComments.sort((a, b) => a.score < b.score ? -1 : a.score > b.score ? 1 : 0);
        }
        else {
            this.sortScoreToggled = false;
            // sort descending
            this.redditComments.sort((a, b) => a.score > b.score ? -1 : a.score < b.score ? 1 : 0);
        }
    }
    // This method will be called whenever we switch a page, add or delete item, and init at the begining
    loadPage(event) {
        this.subreddit
            .loadComments(this.route.snapshot.params["subredditName"]) // load the comments from the subreddit in the route
            .subscribe((projects) => {
            this.redditComments = projects; // assign data to variable on template
            // set default sorting - currently by score descending
            this.redditComments.sort((a, b) => a.score > b.score ? -1 : a.score < b.score ? 1 : 0);
            this.length = projects.length;
            if (event) {
                this.sliceLow = event.pageIndex * event.pageSize;
                this.sliceHigh = this.sliceLow + event.pageSize;
            }
            else {
                this.sliceLow = 0;
                this.sliceHigh = this.pageSize;
            }
        });
        return event;
    }
};
SubredditVideoComponent = __decorate([
    Component({
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

    <div>
      <mat-toolbar>
        <mat-toolbar-row>
          <a routerLink="">{{ subredditName }}</a>
          <span class="spacer"></span>
          <mat-button-toggle (click)="sortDate()">Date</mat-button-toggle>
          <mat-button-toggle (click)="sortScore()">Score</mat-button-toggle>
            <!-- <button (click)="getType()">type</button> -->

          <span class="spacer"></span>
          <subreddit-search-bar></subreddit-search-bar>
          <span class="spacer"></span>
          <mat-paginator
            [length]="length"
            [pageSize]="pageSize"
            [pageSizeOptions]="pageSizeOptions"
            (page)="pageEvent = loadPage($event)"
          >
          </mat-paginator>
        </mat-toolbar-row>
      </mat-toolbar>
    </div>

    <div class="videos">
      <div
        *ngFor="let redditComment of redditComments | slice: sliceLow:sliceHigh"
      >
        <mat-card>
          <youtube-player
            videoId="{{ redditComment.youtubeLinkId }}"
          ></youtube-player>
          <mat-card-actions align="end">
            <a
              mat-stroked-button
              href="https://www.reddit.com{{ redditComment.permalink }}"
              target="_blank"
              >PERMALINK</a
            >
            <button mat-stroked-button>{{ redditComment.createdUTC.toString().substring(0,10) }} | {{ redditComment.createdUTC.toString().substring(11,19) }}</button>
            <button mat-stroked-button>Score: {{ redditComment.score }}</button>
            <!-- <button mat-stroked-button>{{ redditComment.createdUTC.toString().substring(0,10) }}</button>
            <button mat-stroked-button>{{ redditComment.createdUTC.toString().substring(11,19) }}</button> -->
          </mat-card-actions>
        </mat-card>
      </div>
    </div>
  `,
        styles: [
            `
      .videos {
        display: grid;
        /* grid-template-columns: repeat(2, 1fr); */
        grid-template-columns: 1fr 1fr;
        /* grid-gap: 5px; */
        grid-column-gap: 5px;
        grid-row-gap: 10px;
        justify-items: center;
      }
      .item {
        
      }
      .info {
        display: grid;
        grid-template-columns: 1fr 1fr 1fr;
      }

      .spacer {
        flex: 1 1 auto;
      }
      .center {
        align-items: center;
      }
    `,
        ],
    })
], SubredditVideoComponent);
export { SubredditVideoComponent };
//# sourceMappingURL=subreddit-video.component.js.map