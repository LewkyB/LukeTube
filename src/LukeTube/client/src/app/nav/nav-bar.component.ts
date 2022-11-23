import { Component } from "@angular/core";
import { Subreddit } from "../services/subreddit.service";

@Component({
  selector: "nav-bar",
  templateUrl: "./nav-bar.component.html",
  styleUrls: ["nav-bar.component.css"],
})
export class NavBarComponent {
  constructor(public subreddit: Subreddit) {}
  
  youtubeVideoCount: number = 0;

  
  ngOnInit(): void {
    this.subreddit.getTotalRedditComments().then((response: number) => {
      this.youtubeVideoCount = response;
    })
  }
}
