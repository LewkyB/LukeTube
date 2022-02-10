import { NgModule } from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { HttpClientModule } from "@angular/common/http";

import { AppComponent } from "./app.component";
import { Subreddit } from "./services/subreddit.service";
import SubredditView from "./views/subredditView.component";
import { YouTubePlayerModule } from "@angular/youtube-player";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { MatAutocompleteModule } from "@angular/material/autocomplete";
import { ReactiveFormsModule } from "@angular/forms";
import { MatFormFieldModule } from "@angular/material/form-field";

@NgModule({
  declarations: [AppComponent, SubredditView],
  imports: [
    BrowserModule,
    HttpClientModule,
    YouTubePlayerModule,
    BrowserAnimationsModule,
    MatAutocompleteModule,
    ReactiveFormsModule,
    MatFormFieldModule,
  ],
  providers: [Subreddit],
  bootstrap: [AppComponent],
})
export class AppModule {}
