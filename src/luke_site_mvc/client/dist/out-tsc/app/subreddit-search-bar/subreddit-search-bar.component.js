import { __decorate } from "tslib";
import { Component } from "@angular/core";
import { FormControl } from "@angular/forms";
import { map, startWith } from "rxjs/operators";
let SubredditSearchBar = class SubredditSearchBar {
    constructor(subreddit, router) {
        this.subreddit = subreddit;
        this.router = router;
    }
    ngOnInit() {
        // TODO: how can I select this by pressing the hotkey '/'
        this.myControl = new FormControl("");
        this.subreddit.loadSubreddits().subscribe((result) => {
            // this.subreddit.subreddits = result;
        });
        this.filteredSubreddits = this.myControl.valueChanges.pipe(startWith(""), map((value) => this._filter(value)));
    }
    _filter(value) {
        return this.subreddit.subreddits.filter((subreddit) => subreddit.toLowerCase().indexOf(value.toLowerCase()) === 0);
    }
    navigateTo(value) {
        if (value) {
            this.router.navigate(["videos/" + value]);
            console.log(value);
        }
        return false;
    }
};
SubredditSearchBar = __decorate([
    Component({
        selector: "subreddit-search-bar",
        templateUrl: "subreddit-search-bar.component.html",
        styles: [
            `
      .disable-scroll-bar {
        -ms-overflow-style: none; /* for Internet Explorer, Edge */
        scrollbar-width: none; /* for Firefox */
        overflow-y: scroll;
      }
    `,
        ],
    })
], SubredditSearchBar);
export { SubredditSearchBar };
//# sourceMappingURL=subreddit-search-bar.component.js.map