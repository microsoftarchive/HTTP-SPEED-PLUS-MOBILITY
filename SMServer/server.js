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


var fs = require('fs');

var rootFolder = 'root';
var port = (process.env.port || 8080);
var WebSocketServer = require('ws').Server
  , SimpleHttpServer = require('./SimpleHTTPServer')
  , httpServer = new SimpleHttpServer(port, rootFolder)
  , wss = new WebSocketServer({ host: '0.0.0.0', server: httpServer })
  , Error = require('ws/lib/SMError');
 
console.log('Server started at port: ' + port);

wss.on('connection', function(session) {
	console.log('Session opened:');
	console.log(session.request.headers);
	console.log('------------------------------------');
	
    session.on('streamOpened', function(stream) {
        console.log('Stream opened: id=' + stream.streamId);
        console.log('Headers: ');
        console.log(stream.headers);       

        var path = stream.headers[':path'].trim('/');

        if (path == 'index') {        	
        	console.log('Client requested listing of files');        	
        	fs.readdir(__dirname + '/' + rootFolder + '/', 
        		function(error, files){
        			if (error) {
        				console.log('error: %s', error);
        				stream.error({statusCode : Error.INTERNAL_ERROR});
        			} else {
						var result = '';
        				files.forEach(function(elem){        					
        					var check = function(elem){
        						return (elem.indexOf('.txt') > -1 || elem.indexOf('.html') > -1 || elem.indexOf('.htm') > -1);
        					}
        					try{
        						var arrayFiles = fs.readdirSync(__dirname + "/" + rootFolder + "/" + elem +'/');
        						arrayFiles.forEach(function(nextFile){
        							if (check(nextFile)){
        								result += '/' + elem + '/' + nextFile + '\r\n';
        							}
        						});
        					} catch(err){
        						if (check(elem)){
        							result += '/' + elem + '\r\n';
        						}
        					}
        				});        				
        				console.log('Sent ' + result.toString());
        				stream.send(result, true);
        			}
        		}
        	);
        }
        else {
			console.log('Client requested file ' + path);
			fs.readFile(__dirname + '/' + rootFolder +'/' + path,
				function (err, data) {
					if (err) {
						console.log('error: %s', err);
						stream.error({statusCode : Error.INTERNAL_ERROR, message:err});
					} else {
						stream.send(data, true);
						console.log('Sent ' + path);
					}
				}
			);
		}
        stream.on('headers', function(headers){
        	console.log('Headers frame come');
         	console.log(headers);       	
		});
		
        stream.on('error', function(e){
        	console.log('Stream error: code ' + e.statusCode + ', message ' + e.message);    	
		});
    });
	
	session.on('error', function(e) {
		console.log('Error:');
		if (e.streamId) console.log('	Stream id:' + e.streamId);
		if (e.statusCode) console.log('	Status code:' + e.statusCode);
		if (e.error) console.log('	Status code:' + e.error);
	});
	
	session.on('streamClosed', function(stream) {
		console.log('Stream closed:' + stream.streamId);
	});	
	
	session.on('close', function(code, message) {
		console.log('Session closed: code ' + code + ', message ' + (message || "''"));
	});

	session.on('error', function(error) {
		console.log('Session error:');
		console.log(error);
		console.log('------------------------------------');
	});	
});


