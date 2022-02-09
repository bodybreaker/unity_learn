// import { fromPath } from "pdf2pic";

var app = require('express')();
var server = require('http').Server(app);
var io = require('socket.io')(server);
var express = require('express');
var path = require('path');
var os = require("os");
var pdf2pic = require("pdf2pic")
const fs = require('fs');

// for Linux
//const { Poppler } = require("node-poppler");
//const poppler = new Poppler("./usr/bin");

const { Poppler } = require("node-poppler");
const poppler = new Poppler();

//  참고 - http://expressjs.com/en/resources/middleware/multer.html
// 파일 업로드
var multer = require('multer'); // express에 multer모듈 적용 (for 파일업로드)
const { resolve } = require('path');
let storage = multer.diskStorage({
    destination: function(req, file, callback){
		if(req.params.type=="video"){
			callback(null,"public/video/")
			console.log("비디오 저장")
		}else if(req.params.type == "pdf"){
			callback(null,"public/pdf/")
			console.log("pdf변환")
		}
    },
    filename: function(req, file, callback){
		const uniqueSuffix = Date.now() + '-' + Math.round(Math.random() * 1E9)
        callback(null, uniqueSuffix+"-"+file.originalname);
    }
});

// pdf -> 이미지 변환 
// https://github.com/yakovmeister/pdf2image
// gs9.52
// GraphicsMagick-1.3.35-Q16
// poppler - pdfinfo
// https://github.com/Fdawgs/node-poppler

var upload = multer({ storage: storage})

server.listen(3000);

// global variables for the server
var enemies = [];
var playerSpawnPoints = [];
var clients = [];


// app.use(express.static(path.join(__dirname,'public')));
app.use('/public', express.static('public'));

app.get('/', function(req, res) {
	res.send('hey you got back get "/"');
});


// // 영상 업로드
// app.post('/uploadVideo', upload.fields([{name:'destFile',maxCount:1},{name:'type',maxCount:1}]), function(req, res){
// 	console.log(req.files['destFile']);
// 	console.log(req.body.type);  
// 	res.json({
// 		'url':'http://'+hostname+":"+port+"/public/video/"+req.files['destFile'].originalname
// 	}); 
// });

// pdf/ppt 업로드
app.post('/upload/:type', upload.single('destFile'), function(req, res){
	
	// storage destination이 선 호출됨
	console.log(req.file); // 콘솔(터미널)을 통해서 req.file Object 내용 확인 가능.
	console.log(req.params.type);
	
	console.log('저장위치 >>' + req.file.path);

	if(req.params.type =="video"){
		res.json({
			'url':req.protocol+'://'+req.get('host')+"/"+req.file.destination+req.file.filename
		}); // object를 리턴함
		return;
	}else if (req.params.type=="pdf"){
		console.log("이미지로 변환")
		// pdf -> 이미지 리스트 전환

		poppler.pdfInfo(req.file.path,{printAsJson:true}).then(function(info,error){
			
			let defWidth = 841.8*2
			let defHeight = 595.2*2

			console.log("페이지 수>>  ",info.pages);
			console.log("페이지 사이즈>>  ",info.pageSize);

			// 페이지 사이즈 파싱을 위한 정규표현식 추가
			// 595.2 x 841.8 pts (A4)
			// 612 x 792 pts (letter) => ([1-9]*\.*[1-9]*) x ([1-9]*\.*[1-9]*)
			let regex = /([1-9]*\.*[1-9]*) x ([1-9]*\.*[1-9]*)/;
			let sizeArr = info.pageSize.match(regex);
			
			console.log(sizeArr);
			if(sizeArr.length!=3){
				// 페이지 사이즈 파싱 실패
				console.log("페이지 사이즈 파싱 실패");
				
			}else{
				// 페이지 사이즈 파싱 성공
				console.log("가로 >> ",sizeArr[1]," 세로 >> ",sizeArr[2]);
				defWidth = sizeArr[1]*2;
				defHeight = sizeArr[2]*2;
			}
			console.log("파일명 >> ",req.file.name)
			const options = {
				density: 100,
				saveFilename: req.file.filename,
				savePath: "public/image",
				format: "png",
				width: defWidth,
				height: defHeight
			};
			const storeAsImage = pdf2pic.fromPath(req.file.path, options);
			// 모든 페이지를 변환해야함 promise all
			let pageNumArr = []; 
		
			for (let i=0; i< parseInt(info.pages);i++){
				pageNumArr.push(i+1);
			}
			async function call(){
				list = await Promise.all(pageNumArr.map(pageNum=>storeAsImage(pageNum).then((resolve)=>{
					console.log("Page>> ",pageNum);
					//console.log(resolve.path);
					return resolve;
				})));
				//console.log(list);
				return list;
			}
			call().then(imgList=>{

				
				console.log(imgList)
				urlList = imgList.map(imgInfo => {
					return req.protocol+'://'+req.get('host')+"/"+imgInfo.path
				})
				res.json({
					'urls':urlList
				}); // obj
			});

			
			
		})


	}
});




io.on('connection', function(socket) {
	
	var currentPlayer = {};
	currentPlayer.name = 'unknown';

	// 디버깅용
	var onevent = socket.onevent;
	socket.onevent = function (packet) {
		var args = packet.data || [];
		onevent.call (this, packet);    // original call
		packet.data = ["*"].concat(args);
		onevent.call(this, packet);      // additional call to catch-all
	};
	
	socket.on('*',function(event, data) {
		//console.log('[Event received]' + event + ' - data:' + JSON.stringify(data));
	});
	///

	socket.on('player connect', function() {
		console.log(currentPlayer.name+' recv: player connect');
		for(var i =0; i<clients.length;i++) {
			var playerConnected = {
				name:clients[i].name,
				position:clients[i].position,
				rotation:clients[i].position,
				health:clients[i].health
			};
			// in your current game, we need to tell you about the other players.
			socket.emit('other player connected', playerConnected);
			console.log(currentPlayer.name+' emit: other player connected: '+JSON.stringify(playerConnected));
		}
	});

	socket.on('play', function(data) {
		console.log(currentPlayer.name+' recv: play: '+JSON.stringify(data));
		// if this is the first person to join the game init the enemies
		if(clients.length === 0) {
			numberOfEnemies = data.enemySpawnPoints.length;
			enemies = [];
			data.enemySpawnPoints.forEach(function(enemySpawnPoint) {
				var enemy = {
					name: guid(),
					position: enemySpawnPoint.position,
					rotation: enemySpawnPoint.rotation,
					health: 100
				};
				enemies.push(enemy);
			});
			playerSpawnPoints = [];
			data.playerSpawnPoints.forEach(function(_playerSpawnPoint) {
				var playerSpawnPoint = {
					position: _playerSpawnPoint.position,
					rotation: _playerSpawnPoint.rotation
				};
				playerSpawnPoints.push(playerSpawnPoint);
			});
		}

		var enemiesResponse = {
			enemies: enemies
		};
		// we always will send the enemies when the player joins
		console.log(currentPlayer.name+' emit: enemies: '+JSON.stringify(enemiesResponse));
		socket.emit('enemies', enemiesResponse);
		var randomSpawnPoint = playerSpawnPoints[Math.floor(Math.random() * playerSpawnPoints.length)];
		currentPlayer = {
			name:data.name,
			position: randomSpawnPoint.position,
			rotation: randomSpawnPoint.rotation,
			health: 100
		};
		clients.push(currentPlayer);
		// in your current game, tell you that you have joined
		console.log(currentPlayer.name+' emit: play: '+JSON.stringify(currentPlayer));
		socket.emit('play', currentPlayer);
		// in your current game, we need to tell the other players about you.
		socket.broadcast.emit('other player connected', currentPlayer);
	});

	socket.on('player move', function(data) {
		//console.log('recv: move: '+JSON.stringify(data));
		currentPlayer.position = data.position;
		socket.broadcast.emit('player move', currentPlayer);
	});

	socket.on('player turn', function(data) {
		//console.log('recv: turn: '+JSON.stringify(data));
		currentPlayer.rotation = data.rotation;
		socket.broadcast.emit('player turn', currentPlayer);
	});

	socket.on('player shoot', function() {
		console.log(currentPlayer.name+' recv: shoot');
		var data = {
			name: currentPlayer.name
		};
		console.log(currentPlayer.name+' bcst: shoot: '+JSON.stringify(data));
		socket.emit('player shoot', data);
		socket.broadcast.emit('player shoot', data);
	});

	socket.on('player shake', function() {
		console.log(currentPlayer.name+' recv: shake');
		var data = {
			name: currentPlayer.name
		};
		console.log(currentPlayer.name+' bcst: shake: '+JSON.stringify(data));
		socket.emit('player shake', data);
		socket.broadcast.emit('player shake', data);
	});

	socket.on('chat',function(data){
		console.log(currentPlayer.name+' recv: chat: '+JSON.stringify(data));
		var data ={
			name:currentPlayer.name,
			text:data.text
		}
		socket.emit('chat', data);
		socket.broadcast.emit('chat', data);
	});


	//video-url
	socket.on('video-url',function(data){
		console.log(currentPlayer.name+' recv: video-url: '+JSON.stringify(data));
		var data ={
			name:currentPlayer.name,
			url:data.url
		}
		socket.emit('video-play', data);
		socket.broadcast.emit('video-play', data);
	});

	socket.on('health', function(data) {
		console.log(currentPlayer.name+' recv: health: '+JSON.stringify(data));
		// only change the health once, we can do this by checking the originating player
		if(data.from === currentPlayer.name) {
			var indexDamaged = 0;
			if(!data.isEnemy) {
				clients = clients.map(function(client, index) {
					if(client.name === data.name) {
						indexDamaged = index;
						client.health -= data.healthChange;
					}
					return client;
				});
			} else {
				enemies = enemies.map(function(enemy, index) {
					if(enemy.name === data.name) {
						indexDamaged = index;
						enemy.health -= data.healthChange;
					}
					return enemy;
				});
			}

			var response = {
				name: (!data.isEnemy) ? clients[indexDamaged].name : enemies[indexDamaged].name,
				health: (!data.isEnemy) ? clients[indexDamaged].health : enemies[indexDamaged].health
			};
			console.log(currentPlayer.name+' bcst: health: '+JSON.stringify(response));
			socket.emit('health', response);
			socket.broadcast.emit('health', response);
		}
	});

	socket.on('disconnect', function() {
		console.log(currentPlayer.name+' recv: disconnect '+currentPlayer.name);
		socket.broadcast.emit('other player disconnected', currentPlayer);
		console.log(currentPlayer.name+' bcst: other player disconnected '+JSON.stringify(currentPlayer));
		for(var i=0; i<clients.length; i++) {
			if(clients[i].name === currentPlayer.name) {
				clients.splice(i,1);
			}
		}
	});

});

console.log('--- server is running ...');

function guid() {
	function s4() {
		return Math.floor((1+Math.random()) * 0x10000).toString(16).substring(1);
	}
	return s4() + s4() + '-' + s4() + '-' + s4() + '-' + s4() + '-' + s4() + s4() + s4();
}