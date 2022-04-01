import { OnInit } from "@angular/core";
import { Component } from "@angular/core";
import { Subreddit } from "../services/subreddit.service";
import { FormControl } from "@angular/forms";
import { Observable, of } from "rxjs";
import { map, startWith } from "rxjs/operators";
import { Router } from "@angular/router";

@Component({
  selector: "subreddit-search-bar",
  templateUrl: "subreddit-search-bar.component.html",
})
export class SubredditSearchBar implements OnInit {

  myControl!: FormControl;
  filteredSubreddits!: Observable<string[]>;

  constructor(public subreddit: Subreddit, private router: Router) {}

  ngOnInit(): void {

    this.myControl = new FormControl("")
    
    this.subreddit.loadSubreddits().subscribe(result => {
      // this.subreddit.subreddits = result;
    });

    this.filteredSubreddits = this.myControl.valueChanges.pipe(
      startWith(''),
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
        this.router.navigate(["/videos/" + value]);
        console.log(value)
    }
    return false;
}

}
