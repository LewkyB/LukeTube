﻿import http from 'k6/http'

export let options = {
    insecureSkipTLSVerify: true,
    noConnectionReuse: false,
    vus: 1,
    duration: '10s'
};

export default () => {
    http.get('http://localhost:82/api/pushshift/subreddit/aviation')
}
