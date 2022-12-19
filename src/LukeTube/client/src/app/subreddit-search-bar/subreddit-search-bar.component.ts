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
  styleUrls: ["subreddit-search-bar.component.css"],
})
export class SubredditSearchBar implements OnInit {
  myControl!: UntypedFormControl;
  filteredSubreddits!: Observable<string[]>;
  subreddits: string[] = []

  constructor(public subreddit: Subreddit, private router: Router) {}

  ngOnInit(): void {
    // TODO: how can I select this by pressing the hotkey '/'
    this.myControl = new UntypedFormControl("");

    // this.subreddit.loadSubreddits().then((result: string[]) => {
    //   this.subreddits = result;
    // });
    this.subreddit.loadSubreddits().subscribe(result => {
      this.subreddits = result
    })

    this.filteredSubreddits = this.myControl.valueChanges.pipe(
      startWith(""),
      map((value: string) => this._filter(value))
    );
  }

  private _filter(value: string): string[] {
    return this.subreddits.filter(
      (subreddit: string) => subreddit.toLowerCase().indexOf(value.toLowerCase()) === 0
    );
  }

  navigateTo(value: any) {
    if (value) {
      this.router.navigate(["videos/" + value]);
    }

    return false;
  }
}
