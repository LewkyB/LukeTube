import { Component } from "@angular/core";

@Component({
  selector: "link-page",
  template: `
    <nav-bar></nav-bar>
    <router-outlet></router-outlet>
  `,
  styles: [],
})
export class AppComponent {
  title = "client";
}
