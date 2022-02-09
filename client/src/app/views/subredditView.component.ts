import { OnInit } from "@angular/core";
import { Component } from "@angular/core";
import { Subreddit } from "../services/subreddit.service";

@Component({
    selector: "subreddit-links",
    templateUrl: "subredditView.component.html",
})
export default class subredditView implements OnInit{

    constructor(public subreddit: Subreddit) {
    }

    ngOnInit(): void {
        this.subreddit.loadComments()
            .subscribe(() => {
                // do something
            });
    }
}