import { Component, OnInit } from '@angular/core';
import { Subreddit } from '../services/subreddit.service';

@Component({
  selector: 'app-subreddit-video',
  template: `
  <hr>
    <mat-grid-list cols="3" rowHeight="1:1">
      <mat-grid-tile *ngFor="let redditComment of subreddit.redditComments">
        <youtube-player videoId={{redditComment.youtubeLinkId}}></youtube-player>
        <mat-card>{{redditComment.score}}</mat-card>
      </mat-grid-tile>
    </mat-grid-list>
  `,
  styles: [
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
