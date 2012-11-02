/*
Copyright 2012 Microsoft Open Technologies, Inc.

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.

-----------------End of License---------*/

var util = require('util')
  , events = require('events')
  , fs = require('fs')
  , url = require('url')
  , https = require('https')
  , http = require('http');

function SimpleHttpServer(port, root, options) {  
  var self = this;

  if (options != undefined)
  {
    self.server = https.createServer(options).listen(port, function() {
        console.log('Wss server started at port: ' + port);
    });
  }else {
    self.server = http.createServer();    
    self.server.listen(port);
  }

  self.server.on('upgrade', function(req, socket, upgradeHead) {
    handleUpgrade(req, socket, upgradeHead, function(client) {
      self.emit('connection', client);
    });
  });

  function handleUpgrade(req, socket, upgradeHead, cb) {
	  console.log('upgrade request to: ' + req.headers.upgrade);
	  
	  if (typeof req.headers.upgrade != 'undefined' && req.headers.upgrade.toLowerCase() == 'websocket') {
		return;
	  }	  
	  
	  if (typeof req.headers.upgrade === 'undefined' || req.headers.upgrade.toLowerCase() !== 'http/2.0') {
		abortConnection(socket, 400, 'Bad Request');
		return;
	  }

	  var u = url.parse(req.url);
      var str = u.path.replace(/\/+/g, '\\');
      path = __dirname + '\\' + root + str;
      console.log('request http2: ' + path);

      fs.readFile(path,
			function (err, data) {
			    if (err) {
			      console.log('error: %s', err);
			      abortConnection(socket, 404, 'Not Found');
			    } else {
				
				  console.log('Switching Protocols');
				  // Respond that we understand http 2.0
				  var response = [
					'HTTP/1.1 101 Switching Protocols',
					'Connection: Upgrade',
					'Upgrade: HTTP/2.0'
				  ];
				  socket.write(response.concat('', '').join('\r\n'));
				  socket.write('\r\n');
				  
				  // emulate http2 frame
				  socket.write('http2frame_start\r\n');	  
				  socket.write(data);
				  socket.write('http2frame_end');
				  
				  socket.end();	
			    }
			}
      );  
  }
  
  function abortConnection(socket, code, name) {
	try {
	  var response = [
		'HTTP/1.1 ' + code + ' ' + name,
		'Content-type: text/html'
	  ];
	  socket.write(response.concat('', '').join('\r\n'));
      socket.end();
	  console.log('Connection aborted! Code: ' + code);
	}
	catch (e) {}
  }  

  self.server.on('request', function (req, res) {
      var u = url.parse(req.url);
      var str = decodeURIComponent(u.path.replace(/\++/g, ' '));
      path = __dirname + '/' + root + '/' + str;
      console.log('request: ' + path);

      fs.readFile(path,
			function (err, data) {
			    if (err) {
			        console.log('error: %s', err);
			        res.writeHead(404, { 'Content-Type': 'text/plain' });
			        res.end('Not found');
			    } else {
			        console.log('Sent ' + path);
			        res.writeHead(200, {
			            'Server': 'SM Server'
					, 'Date': new Date().toUTCString()
					, 'Cache-Control': 'private, max-age=0, no-cache'
					, 'Pragma': 'no-cache'
					, 'X-Powered-By': 'node.js'
					, 'Content-Length': data.length
			        });
			        res.end(data);
			    }
			}
      );
  });
  return self.server;
}

util.inherits(SimpleHttpServer, events.EventEmitter);
module.exports = SimpleHttpServer;