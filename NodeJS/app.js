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
        for(var i = 0; i < wsList.length; i++)
        {
            if(wsList[i] == ws)
            {
                wsList.splice(i, 1);
                break;
            }
        }
    });
});

server.listen(process.env.PORT || 8080, ()=>{
    console.log("Server start at port "+server.address().port);
});

function Boardcast(data)
{
    for(var i = 0; i < wsList.length; i++)
    {
        wsList[i].send(data);
    }
}

