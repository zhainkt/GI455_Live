const app = require('express')();
const server = require('http').Server(app);
const websocket = require('ws');

const wss = new websocket.Server({server});

var wsList = [];
var roomList = [];
/*
{
    roomName: ""
    wsList: []
}
*/

wss.on("connection", (ws)=>{
    
    //Lobby
    {
        console.log("client connected.");
        //Reception
        ws.on("message", (data)=>{
            console.log("send from client :"+ data);

            var toJsonObj = JSON.parse(data);

            if(toJsonObj.eventName == "CreateRoom")//CreateRoom
            {
                var isFoundRoom = false;

                for(var i = 0; i < roomList.length; i++)
                {
                    if(roomList[i].roomName == toJsonObj.roomName)
                    {
                        isFoundRoom = true;
                        break;
                    }
                }

                if(isFoundRoom == true)
                {
                    //Callback to client : create room fail
                    ws.send("CreateRoomFail");

                    console.log("client create room fail.");
                }
                else
                {
                    //Callback to client : create room success
                    var newRoom = {
                        roomName: toJsonObj.roomName,
                        wsList: []
                    }

                    newRoom.wsList.push(ws);
    
                    roomList.push(newRoom);

                    ws.send("CreateRoomSuccess");

                    console.log("client create room success.");
                }

                console.log("client request CreateRoom ["+toJsonObj.roomName+"]");
                
            }
            else if(toJsonObj.eventName == "JoinRoom")//JoinRoom
            {
                console.log("client request JoinRoom");
            }
            else if(toJsonObj.eventName == "LeaveRoom")
            {
                var isLeaveSuccess = false;

                for(var i = 0; i < roomList.length; i++)
                {
                    for(var j = 0; j < roomList[i].wsList.length; j++)
                    {
                        if(ws == roomList[i].wsList[j])
                        {
                            roomList[i].wsList.splice(j, 1);

                            if(roomList[i].wsList.length <= 0)
                            {
                                roomList.splice(i, 1);
                            }
                            isLeaveSuccess = true;
                            break;
                        }
                    }
                }

                if(isLeaveSuccess)
                {
                    ws.send("LeaveRoomSuccess");

                    console.log("leave room success");
                }
                else
                {
                    ws.send("LeaveRoomFail");

                    console.log("leave room fail");
                }
            }
        });
    }


    /*wsList.push(ws);
    
    ws.on("message", (data)=>{
        console.log("send from client :"+ data);
        Boardcast(data);
    });
    */
    ws.on("close", ()=>{
        console.log("client disconnected.");

        for(var i = 0; i < roomList.length; i++)
        {
            for(var j = 0; j < roomList[i].wsList.length; j++)
            {
                if(ws == roomList[i].wsList[j])
                {
                    roomList[i].wsList.splice(j, 1);

                    if(roomList[i].wsList.length <= 0)
                    {
                        roomList.splice(i, 1);
                    }

                    break;
                }
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

