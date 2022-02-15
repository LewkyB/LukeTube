import { Component } from "@angular/core";

@Component({
  selector: "link-page",
  template: `
  <nav-bar></nav-bar>
  <app-subreddit-list></app-subreddit-list> 
  `,
})
export class AppComponent {
  title = "client";
}
