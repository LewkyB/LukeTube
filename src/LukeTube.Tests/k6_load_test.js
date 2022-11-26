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
