const app = require('express')();
const server = require('http').Server(app);
const websocket = require('ws');

const wss = new websocket.Server({server});

var wsList = [];

wss.on("connection", (ws)=>{

    console.log("client connected.");
    wsList.push(ws);
    
    ws.on("message", (data)=>{
        console.log("send from client :"+ data);
        Boardcast(data);
    });

    ws.on("close", ()=>{
        console.log("client disconnected.");
        wsList = ArrayRemove(wsList, ws);
    });
});

server.listen(process.env.PORT || 8080, ()=>{
    console.log("Server start at port "+server.address().port);
})

function ArrayRemove(arr, value)
{
    return arr.filter((element)=>{
        return element != value;
    });
}

function Boardcast(data)
{
    for(var i = 0; i < wsList.length; i++)
    {
        wsList[i].send(data);
    }
}

