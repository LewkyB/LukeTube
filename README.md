﻿# LukeTube

This is a containerized ASP.NET Core web application that uses the Pushshift API to provide youtube video links from comments written in specific subreddits. Uses [nginx-proxy](https://github.com/nginx-proxy/nginx-proxy) to automate nginx and https certifications. 

## Requirements
- git
- [docker compose](https://docs.docker.com/get-docker/)

## Instructions

1. `git clone https://github.com/LewkyB/IRCTube-ASPNETCore.git`
1. `cd IRCTube-ASPNETCore`
1. `mv .env.acme-companion.example .env.acme-companion`
1. In the new `.env.acme-companion` file, fill in your `DEFAULT_EMAIL`
1. `docker-compose up`

## Troubleshooting
If you fail with building on a Raspberry Pi 4 check out [this](https://github.com/dotnet/dotnet-docker/issues/3253#issuecomment-956378676) for a fix

# Credit
- [Pushshift](https://github.com/pushshift/api)
- [PsawSharp](https://github.com/zHaytam/PsawSharp)
- [nginx-proxy](https://github.com/nginx-proxy/nginx-proxy)
- [heckingoodboys](https://github.com/pawl/heckingoodboys/) (for nginx-proxy docker-compose configuration example)
