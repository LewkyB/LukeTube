import { __decorate } from "tslib";
import { Component } from "@angular/core";
let SubredditListComponent = class SubredditListComponent {
    constructor(subreddit) {
        this.subreddit = subreddit;
    }
    ngOnInit() {
        this.subreddit.loadSubreddits().subscribe((result) => {
            // get the results
            this.subreddit.subreddits = result;
            // sort them alphabetically
            this.subreddit.subreddits.sort(function (a, b) {
                var textA = a.toUpperCase();
                var textB = b.toUpperCase();
                return textA < textB ? -1 : textA > textB ? 1 : 0;
            });
        });
    }
};
SubredditListComponent = __decorate([
    Component({
        selector: "app-subreddit-list",
        template: `
  <nav-bar></nav-bar>
  <div class="container">
    <mat-grid-list cols="12" rowHeight="3:1">
      <mat-grid-tile *ngFor="let subreddit of subreddit.subreddits">
        <a routerLink="/videos/{{ subreddit }}">
          {{ subreddit }}
        </a>
      </mat-grid-tile>
    </mat-grid-list>
</div>
  `,
        styles: [
            `
      .container {
          border: 1px solid;
          color: pink;
      }
      `
        ],
    })
], SubredditListComponent);
export { SubredditListComponent };
//# sourceMappingURL=subreddit-list.component.js.map