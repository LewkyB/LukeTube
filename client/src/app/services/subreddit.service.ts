import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { map } from "rxjs/operators";

import { RedditComment } from "../shared/RedditComment";

@Injectable()
export class Subreddit {

    constructor(private http: HttpClient) {

    }

    public redditComments: RedditComment[] = [];
    public subreddits: string[] = [];

    loadComments(): Observable<void> {
        return this.http.get<[]>("api/space")
            .pipe(map(data => {
                this.redditComments = data;
                return;
            }));
    }
    
    loadSubreddits() {
    // loadSubreddits(): Observable<void> {
        return this.http.get<string[]>("api/subredditnames")
            // .pipe(map(data => {
            //     this.subreddits = data;
            //     return;
            // }));
    }
}