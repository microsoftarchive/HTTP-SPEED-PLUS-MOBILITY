/*
---------------------------------------
HTTPbis
Copyright Microsoft Open Technologies, Inc.
---------------------------------------
Microsoft Reference Source License.

This license governs use of the accompanying software. 
If you use the software, you accept this license. 
If you do not accept the license, do not use the software.

1. Definitions

The terms "reproduce," "reproduction," and "distribution" have the same meaning here 
as under U.S. copyright law.
"You" means the licensee of the software.
"Your company" means the company you worked for when you downloaded the software.
"Reference use" means use of the software within your company as a reference, in read only form, 
for the sole purposes of debugging your products, maintaining your products, 
or enhancing the interoperability of your products with the software, 
and specifically excludes the right to distribute the software outside of your company.
"Licensed patents" means any Licensor patent claims which read directly on the software 
as distributed by the Licensor under this license. 

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, the Licensor grants you a non-transferable, 
non-exclusive, worldwide, royalty-free copyright license to reproduce the software for reference use.
(B) Patent Grant- Subject to the terms of this license, the Licensor grants you a non-transferable, 
non-exclusive, worldwide, royalty-free patent license under licensed patents for reference use. 

3. Limitations
(A) No Trademark License- This license does not grant you any rights 
to use the Licensor’s name, logo, or trademarks.
(B) If you begin patent litigation against the Licensor over patents that you think may apply 
to the software (including a cross-claim or counterclaim in a lawsuit), your license 
to the software ends automatically. 
(C) The software is licensed "as-is." You bear the risk of using it. 
The Licensor gives no express warranties, guarantees or conditions. 
You may have additional consumer rights under your local laws 
which this license cannot change. To the extent permitted under your local laws, 
the Licensor excludes the implied warranties of merchantability, 
fitness for a particular purpose and non-infringement. 

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


