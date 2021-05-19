// Node.js sample
var request = require('request');
var crypto = require('crypto');

var key = 'CwQ7vZcFcPbAsi1dN3au8E';
var secret = 'q5uPgau2w2Q4o8Ks+r+CVeXxiv72DJ4W1HGzxZKrEkU=';

//var timestamp = Date.now().toString();
var timestamp = '1621441727238'

console.log(timestamp);
var method = 'GET';
var path = '/v1/me/getbalance';
var body = '';

var text = timestamp + method + path + body;
console.log(text);
var sign = crypto.createHmac('sha256', secret).update(text).digest('hex');
console.log(sign);

var options = {
    url: 'https://api.bitflyer.com' + path,
    method: method,
    body: body,
    headers: {
        'ACCESS-KEY': key,
        'ACCESS-TIMESTAMP': timestamp,
        'ACCESS-SIGN': sign,
        'Content-Type': 'application/json'
    }
};
request(options, function (err, response, payload) {
    console.log(payload);
});