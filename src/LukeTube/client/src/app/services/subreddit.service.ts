import { Injectable } from "@angular/core";

@Injectable()
export class Subreddit {
  loadComments(subreddit: string) {
    return fetch(`http://localhost:82/api/pushshift/subreddit/${subreddit}`, {
      method: "GET",
    }).then((response) => {
      if (!response.ok)
        throw new Error(`HTTP error! Status: ${response.status}`);

      return response.json();
    });
  }

  loadSubreddits() {
    return fetch("http://localhost:82/api/pushshift/subreddit-names", {
      method: "GET",
    }).then((response) => {
      if (!response.ok)
        throw new Error(`HTTP error! Status: ${response.status}`);

      return response.json();
    });
  }

  loadCommentsPaged(subreddit: string, pageNumber: number) {
    return fetch(
      `http://localhost:82/api/pushshift/${subreddit}/paged-comments/${pageNumber}`,
      {
        method: "GET",
      }
    ).then((response) => {
      if (!response.ok)
        throw new Error(`HTTP error! Status: ${response.status}`);

      return response.json();
    });
  }

  loadSubredditsWithLinkCount() {
    return fetch(
      "http://localhost:82/api/pushshift/subreddit-names-with-link-count",
      {
        method: "GET",
      }
    ).then((response) => {
      if (!response.ok)
        throw new Error(`HTTP error! Status: ${response.status}`);

      return response.json();
    });
  }

  getTotalRedditComments() {
    return fetch(
      "http://localhost:82/api/pushshift/youtube-video-total-count",
      {
        method: "GET",
      }
    ).then((response) => {
      if (!response.ok)
        throw new Error(`HTTP error! Status: ${response.status}`);

      return response.json();
    });
  }
}
