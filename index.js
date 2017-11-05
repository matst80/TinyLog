var net = require('net');
var fs = require('fs');

var express = require('express');  
var app = express();  
var server = require('http').createServer(app);  
var io = require('socket.io')(server);

app.use(express.static(__dirname + '/static'));  
app.get('/', function(req, res, next) {  
    res.sendFile(__dirname + '/index.html');
});

var mapFunc = function(cb) {
    var ret = [];
    for(var i in this) {
        var v = cb(this[i],i);
        if (v)
            ret.push(v);
        else if (v===false)
            return ret;
    }
    return ret;
}

Array.prototype.map = mapFunc;

function parseLog(data,send) {

    var ret = {
        sum:{
            //'0':0,
            '1':0,
            '2':0,
            '3':0,
            '4':0,
            '5':0,
            '6':0
        }
    };
    ret.log = data.split('##').map(function(e) {
            if (e.length<10)
                return;
            msgStart = e.indexOf('\n');
            var v = e.substr(0,msgStart);
            var msg = e.substring(msgStart+1);
            var parts = v.split('\t');
            ret.sum[parts[1]]++;
            var r = {
                date: parts[0],
                type: parts[1]-0,
                client: parts[2],
                data: msg
            };
            if (send) {
                io.sockets.emit('log',r);
            }
            return r;
        })
        
    console.log(ret);
    return ret;
}

server.listen(4200);

var handlers = {
    'file':function(opt) {
        console.log('init file');
        var t = this;
        this.file = opt;
        this.load = function(cb) {
            fs.readFile(t.file,{},function(err,fileData) {
                if (!err)
                    cb(parseLog(fileData.toString()));
            });
        }
        this.log = function(data,cb) {
            if (data&&data.length) {
                console.log('logging',t.file);
                fs.appendFile(t.file, data, function(err) {
                    if (err)
                        console.log('write error',err);
                    else
                        console.log('written do disk');
                });
                cb(parseLog(data,true));
            }
        }
    }
}

io.on('connection',function(socket) {
    var iohandler;
    socket.on('auth',function(data) {
        iohandler = authKeys[data];
        if (iohandler) { 
            socket.emit('history',iohandler.log);
        }
    })
})

var authKeys = {
    'wEJg1EDYiy': {
        type:'file',
        settings:'./appalizer.log'
    }
};
var keyLength = 10;

mapFunc.apply(authKeys,[function(val,key) {
    //console.log('init',val);
    var h = new handlers[val.type](val.settings);
    val.handler = h;
    if (h.load) {
        h.load(function(d) {
            val.sum = d.sum;
            val.log = d.log;
        });
    }
}]);

function addToLog(hdl,logdata) {
    console.log(hdl,logdata);
    for(var i in logdata.sum) {
        hdl.sum[i]+=logdata.sum[i];
    }
    logdata.log.map(function(v) {
        hdl.log.push(v);
    })
}

var server = net.createServer(function(socket) {
    console.log('client connected, auth');
    //socket.write("auth:");
    var authHandler;
    socket.on('data',function(dataBuff) {
        var data = dataBuff.toString();
        //console.log('got:',data);
        if (authHandler) {
            console.log('has handler:',authHandler);
            authHandler.handler.log(data,function(res) {
                addToLog(authHandler,res);
            });           
        }
        else
        {
            var keyData = authKeys[data.substr(0,keyLength)];
            console.log('found:',keyData);

            if (keyData) {
                authHandler = keyData;
                if (data.length>keyLength) {
                    var logdata = data.substring(keyLength);
                    authHandler.handler.log(logdata,function(res) {
                        addToLog(authHandler,res);
                    });           
                }
            }
        }
    });
    socket.on('end',function() {
        console.log('closed connection');
    })
});

server.listen(1337, '10.10.10.181');