import { __decorate } from "tslib";
import { Injectable } from "@angular/core";
let Subreddit = class Subreddit {
    constructor(http) {
        this.http = http;
        this.redditComments = [];
        this.subreddits = [];
    }
    loadComments(subreddit) {
        // loadComments(subreddit: string): Observable<void> {
        //var subredditUrl = "http://luketube.net:82/api/" + subreddit;
        var subredditUrl = "http://localhost:82/api/" + subreddit;
        //var subredditUrl = "/api/" + subreddit;
        return this.http.get(subredditUrl);
        // .pipe(map(data => {
        //     this.redditComments = data;
        //     return;
        // }));
    }
    loadSubreddits() {
        // loadSubreddits(): Observable<void> {
        //return this.http.get<string[]>("http://luketube.net:82/api/subredditnames")
        return this.http.get("http://localhost:82/api/subredditnames");
        //return this.http.get<string[]>("/api/subredditnames")
        // .pipe(map(data => {
        //     this.subreddits = data;
        //     return;
        // }));
    }
};
Subreddit = __decorate([
    Injectable()
], Subreddit);
export { Subreddit };
//# sourceMappingURL=subreddit.service.js.map