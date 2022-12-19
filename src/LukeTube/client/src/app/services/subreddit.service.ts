import { Injectable } from "@angular/core";
import { Observable } from "rxjs/internal/Observable";
import { SubredditWithCount } from "../models/SubredditWithCount";

@Injectable()
export class Subreddit {
  async loadComments(subreddit: string) {
    return fetch(`http://localhost:82/api/pushshift/subreddit/${subreddit}`, {
      method: "GET",
    }).then((response) => {
        if (!response.ok)
        throw new Error(`HTTP error! Status: ${response.status}`);

        return response.json();
      });
  }

  loadSubreddits(): Observable<string[]> {
    // loadSubreddits() {
    //   return fetch("http://localhost:82/api/pushshift/subreddit-names", {
    //     method: "GET",
    //   }).then((response) => {
    //     if (!response.ok)
    //       throw new Error(`HTTP error! Status: ${response.status}`);

    //     return response.json();
    //   });
    return new Observable<string[]>(observer => {
      fetch("http://localhost:82/api/pushshift/subreddit-names", {
        method: "GET",
      }).then(response => response.json())
        .then(data => {
          observer.next(data);
          // observer.complete();
        })
    });
  }

  async loadCommentsPaged(subredditName: string, pageNumber: number) {
    return fetch(
      `http://localhost:82/api/pushshift/${subredditName}/paged-comments/${pageNumber}`,
      {
        method: "GET",
      }
    ).then((response) => {
        if (!response.ok)
        throw new Error(`HTTP error! Status: ${response.status}`);

        return response.json();
      });
  }

  loadSubredditsWithLinkCount(): Observable<SubredditWithCount[]> {
    // return fetch(
    //   "http://localhost:82/api/pushshift/subreddit-names-with-link-count",
    //   {
    //     method: "GET",
    //   }
    // ).then((response) => {
    //     if (!response.ok)
    //     throw new Error(`HTTP error! Status: ${response.status}`);
    //
    //     return response.json();
    //   });
    return new Observable<SubredditWithCount[]> (observer => {
      fetch("http://localhost:82/api/pushshift/subreddit-names-with-link-count", {
        method: "GET",
      }).then(response => response.json())
        .then(data => {
          observer.next(data);
          // observer.complete();
        });
    });
  }

  async getTotalRedditComments(): Promise<number> {
    return fetch(
      "http://localhost:82/api/pushshift/youtube-video-total-count", {
        method: "GET",
      }
    ).then((response) => {
        if (!response.ok)
        throw new Error(`HTTP error! Status: ${response.status}`);

        return response.json();
      });
  }
}
