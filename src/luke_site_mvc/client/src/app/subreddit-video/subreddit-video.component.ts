import { Component, OnInit } from '@angular/core';
import { Subreddit } from '../services/subreddit.service';

@Component({
  selector: 'app-subreddit-video',
  template: `
  <hr>
    <mat-grid-list cols="3" rowHeight="1:1">
      <mat-grid-tile *ngFor="let redditComment of subreddit.redditComments">
        <youtube-player class="video-container" videoId={{redditComment.youtubeLinkId}}></youtube-player>
        <mat-card>{{redditComment.score}}</mat-card>
      </mat-grid-tile>
    </mat-grid-list>
  `,
  styles: [
    `
    .video-container {
      position: relative;
      padding-top: 10%;
      padding-bottom: 56.25%; /* 16:9 */
      height: 0;
    }
    .video-container iframe {
      position: absolute;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
    }
    `,
  ]
})
export class SubredditVideoComponent implements OnInit {

  constructor(public subreddit: Subreddit) { }

  ngOnInit(): void {
    
    this.subreddit.loadComments("math").subscribe(result => {
      this.subreddit.redditComments = result
    console.log(this.subreddit.redditComments)
    })
    const tag = document.createElement('script');
      tag.src = 'https://www.youtube.com/iframe_api';
      document.body.appendChild(tag);
    // console.log(this.subreddit.redditComments)
  }

}
