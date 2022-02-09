import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';

import { AppComponent } from './app.component';
import { Subreddit } from './services/subreddit.service';
import SubredditView from './views/subredditView.component';
import { YouTubePlayerModule } from '@angular/youtube-player';

@NgModule({
  declarations: [
        AppComponent,
        SubredditView,
  ],
  imports: [
      BrowserModule,
      HttpClientModule,
      YouTubePlayerModule,
  ],
    providers: [
        Subreddit
    ],
  bootstrap: [AppComponent]
})
export class AppModule { }
