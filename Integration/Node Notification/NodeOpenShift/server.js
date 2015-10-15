// var express  = require('express');
// var app    = express();
// var server = require('http').createServer(app);
// var io     = require('socket.io').listen(server);

var app = require('express')();
var server = require('http').Server(app);
var io = require('socket.io').listen(server);

// var app=require("http").createServer();
// var io=require("socket.io")(app);
////app.listen(8000);
	
	
////Setup express middleware
// app.use(express.static('public'));
// app.use(connect.logger());
// app.use("/", express.static(__dirname+"/public"));

// Start the server
var port = process.env.OPENSHIFT_NODEJS_PORT || 8080  
, ip = process.env.OPENSHIFT_NODEJS_IP || "127.0.0.1";
server.listen(port, ip);
server.timeout = 10000;


app.get("/post", function (req, res) {
    res.sendfile(__dirname + "/MyPage.html");
	
});


app.post('/post_endpoint', function (req, res) {
 
res.writeHead(200, {"Content-Type": "text/plain"});
			
					// req.on('data', function(chunk) {
					// io.sockets.emit("message",req.body);
					// res.write("hi");
					// console.log('message received: ' + "Hi");
					// });
					//console.log('message received: ' + "Hi");
					
					
					// console.log(req.body);
					// io.sockets.emit("message",req.body);			

				req.on('data', function(chunk) {
					
			    io.sockets.emit("message",chunk.toString());
				console.log("Received body data:");
				console.log(chunk.toString());
				});
				req.on('end', function() {
				// empty 200 OK response for now
				//res.writeHead(200, "OK", {'Content-Type': 'text/html'});
				res.end();
				});
													
						
					//req.end();
			        //res.write("Hello");
					//res.end();
			
});



io.on('connection', function (socket) {
  socket.emit('message', { hello: 'world' });
  socket.on('my other event', function (data) {
    console.log(data);
  });
});



// io.on("connection",function(socket){
	
	// socket.emit("alert","hello from server :)");
	// socket.on("click",function(data){
		
		// console.log(data);
	// });	
	
	// socket.on("disconnect",function(){
		
		// console.log("Byee")
	
	// });
	
	
	
// });



// var port = process.env.OPENSHIFT_NODEJS_PORT || 8080  
// , ip = process.env.OPENSHIFT_NODEJS_IP || "127.0.0.1";
// app.listen(port, ip);

	// io.on('connection', function(client) {  
    // console.log('Client connected...');

    // client.on('join', function(data) {
        // console.log(data);
		// client.emit('messages', 'Hello from server');
    // });
// });

// app.get('/', function (req, res) {
  // res.writeHeader(200, {"Content-Type": "text/plain"});
			// res.write("Hello");
			// res.end();
			
// });





// app.get("/post", function (req, res) {
    // res.sendfile(__dirname + "/MyPage.html");
// });



// app.post('/post_endpoint', function (req, res) {
 
// res.writeHead(200, {"Content-Type": "text/plain"});
			
					//// req.on('response', function(chunk) {
					//// io.emit('message',"Hi");
					//// res.write("hi");
					//// console.log('message received: ' + "Hi");
					//// });
					
						// io.on('connection', function(socket){
					  // socket.emit('message', { some: 'data' });
					  // console.log('message received: ' + "Hi");
					// });
										
					
					
					
				

					
					////req.end();
			        ////res.write("Hello");
					// res.end();


			
// });



// app.post('/post_endpoint', function (req, res) {
 
// res.writeHeader(200, {"Content-Type": "text/plain"});
		
			
			// res.write("Hello");
			// res.end();


			
// });
