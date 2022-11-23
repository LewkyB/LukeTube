import { Component, OnInit } from "@angular/core";
import { SubredditWithCount } from "../models/SubredditWithCount";
import { Subreddit } from "../services/subreddit.service";

@Component({
  selector: "app-subreddit-list",
  templateUrl: "subreddit-list.component.html",
  styleUrls: ["subreddit-list.component.css"],
})
export class SubredditListComponent implements OnInit {
  subreddits!: SubredditWithCount[];

  constructor(public subreddit: Subreddit) {}

  ngOnInit(): void {
    this.subreddit.loadSubredditsWithLinkCount()
    .then((result: SubredditWithCount[]) => {
      this.subreddits = result;

      // sort them alphabetically
      this.subreddits.sort(function (a, b) {
        var textA = a.subreddit.toUpperCase();
        var textB = b.subreddit.toUpperCase();
        return textA < textB ? -1 : textA > textB ? 1 : 0;
      });
    });
  }
}
