import { OnInit } from "@angular/core";
import { Component } from "@angular/core";
import { Subreddit } from "../services/subreddit.service";
import { UntypedFormControl } from "@angular/forms";
import { Observable, of } from "rxjs";
import { map, startWith } from "rxjs/operators";
import { Router } from "@angular/router";

@Component({
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
export class SubredditSearchBar implements OnInit {
  myControl!: UntypedFormControl;
  filteredSubreddits!: Observable<string[]>;

  constructor(public subreddit: Subreddit, private router: Router) {}

  ngOnInit(): void {
    // TODO: how can I select this by pressing the hotkey '/'
    this.myControl = new UntypedFormControl("");

    this.subreddit.loadSubreddits().subscribe((result) => {
      // this.subreddit.subreddits = result;
    });

    this.filteredSubreddits = this.myControl.valueChanges.pipe(
      startWith(""),
      map((value) => this._filter(value))
    );
  }

  private _filter(value: string): string[] {
    return this.subreddit.subreddits.filter(
      (subreddit) => subreddit.toLowerCase().indexOf(value.toLowerCase()) === 0
    );
  }
  navigateTo(value: any) {
    if (value) {
      this.router.navigate(["videos/" + value]);
      console.log(value);
    }
    return false;
  }
}
