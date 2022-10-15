import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { map } from "rxjs/operators";

import { RedditComment } from "../models/RedditComment";

@Injectable()
export class Subreddit {

    constructor(private http: HttpClient) {

    }

    public redditComments: RedditComment[] = [];
    public subreddits: string[] = [];

    loadComments(subreddit: string) {
    // loadComments(subreddit: string): Observable<void> {
        var subredditUrl = "http://localhost:82/api/reddit/subreddit/" + subreddit;
        return this.http.get<RedditComment[]>(subredditUrl)
            // .pipe(map(data => {
            //     this.redditComments = data;
            //     return;
            // }));
    }

    loadSubreddits() {
    // loadSubreddits(): Observable<void> {
        return this.http.get<string[]>("http://localhost:82/api/reddit/subreddit-names")
            // .pipe(map(data => {
            //     this.subreddits = data;
            //     return;
            // }));
    }
}