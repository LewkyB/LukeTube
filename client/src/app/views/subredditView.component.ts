import { OnInit } from "@angular/core";
import { Component } from "@angular/core";
import { Subreddit } from "../services/subreddit.service";
import { FormControl } from "@angular/forms";
import { NEVER, Observable } from "rxjs";
import { map, startWith } from "rxjs/operators";

@Component({
  selector: "subreddit-links",
  templateUrl: "subredditView.component.html",
})
export default class subredditView implements OnInit {
  constructor(public subreddit: Subreddit) {}

  myControl = new FormControl("");

  subreddits!: string[];
  filteredSubreddits!: Observable<string[]>;

  ngOnInit(): void {
    this.subreddit.loadSubreddits().subscribe(() => {
      // do something
    });

    this.filteredSubreddits = this.myControl.valueChanges.pipe(
      startWith(""),
      map((value) => this._filter(value))
    );
  }

  private _filter(value: string): string[] {
    const filterValue = value.toLowerCase();

    return this.subreddits.filter((subreddit) =>
      subreddit.toLowerCase().includes(filterValue)
    );
  }
}
