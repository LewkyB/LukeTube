/*
To run with docker:
    $ cat .\LukeTube.Tests\k6_load_test.js | docker run --net=host --rm -i grafana/k6 run -
 */

import http from 'k6/http'

export let options = {
    insecureSkipTLSVerify: true,
    noConnectionReuse: false,
    vus: 10,
    duration: '2000s'
};

export default () => {
    http.get('http://localhost:82/api/pushshift/subreddit/aviation')
}
