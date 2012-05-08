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

var util = require('util')
  , events = require('events')
  , fs = require('fs')
  , url = require('url')
  , http = require('http');

function SimpleHttpServer(port, root) {  
  var self = this;

  self.server = http.createServer();
  self.server.listen(port);

  self.server.on('request', function (req, res) {
		var u = url.parse(req.url);
        path = __dirname + '/' + root + '/' + u.pathname; 
        console.log('request: ' + path);
        
		fs.readFile(path,
			function (err, data) {
			  if (err) {
				console.log('error: %s', err);
				res.writeHead(404, {'Content-Type': 'text/plain'});
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