const app = require('express')();
const server = require('http').Server(app);
const websocket = require('ws');
const wss = new websocket.Server({server});

const serverTickRate = 32;
const millInterval = (1/serverTickRate) * 1000;

class RoomData{
    constructor(roomID, roomName, wsList){
        this.roomID = roomID;
        this.roomName = roomName;
        this.wsList = wsList;
    }
}

class WebSocketData{
    constructor(ws, data){
        this.ws = ws;
        this.data = data;
    }
}

var roomDataList = [];

wss.on("connection", (ws)=>{

    console.log("client connected.");
    //wsList.push(ws);

    ws.on("message", (receive)=>{
        
        //console.log(ws + " : " +receive);
        Event(ws, receive);

    });

    ws.on("close", ()=>{
        console.log("client disconnected.");
        LeaveRoom(ws);
        //wsList = ArrayRemove(wsList, ws);
    });
});

server.listen(process.env.PORT || 8080, ()=>{
    console.log("Server start at port "+server.address().port);
});

function Event(ws, receive)
{
    var jsonObj = JSON.parse(receive);
    var event = jsonObj.Event;
    var data = jsonObj.Data;
    
    switch(event)
    {
        case "createRoom":
        {
            console.log("CreateRoom");
            CreateRoom(ws, data.roomName);
            break;
        }
        case "joinRoom":
        {
            console.log("JointRoom");
            JoinRoom(ws, data.roomName);
            break;
        }
        case "leaveRoom":
        {
            LeaveRoom(ws);
            break;
        }
        case "message":
        {
            UpdateData(ws, receive);
            break;
        }
        default:{}
    }
}

function CreateRoom(ws, roomName)
{
    var wsData = new WebSocketData(ws, "");
    var roomID = require('udid')('roomID');
    var roomData = new RoomData(roomID, roomName, [wsData]);
    var isExistRoom = false;

    for(var i = 0; i < roomDataList.length; i++)
    {
        if(roomDataList[i].roomName == roomName)
        {
            isExistRoom = true;
            break;
        }
    }

    var eventData = {
        Event:"createRoom",
        Msg:""
    }

    eventData.Msg = isExistRoom ? "fail" : "success";
    
    if(eventData.Msg == "success"){
        roomDataList.push(roomData);
        var lastIndex = roomDataList.length-1;
        console.log("RoomID : " + roomDataList[lastIndex].roomID);
        console.log("RoomName : " + roomDataList[lastIndex].roomName);
        console.log("Current Player : " + roomDataList[lastIndex].wsList.length);
    }

    ws.send(JSON.stringify(eventData)); 
}

function JoinRoom(ws, roomName)
{
    var indexRoom = -1;
    for(let i = 0; i < roomDataList.length; i++)
    {
        if(roomDataList[i].roomName == roomName)
        {
            indexRoom = i;
            break;
        }
    }

    let eventData = {
        Event:"joinRoom",
        Msg:""
    }

    if(indexRoom == -1)
    {
        console.log("Room : " + roomName + " is not found.");
        eventData.Msg = "not exist";
    }
    else
    {
        var isFoundWsInRoom = false; 
        for(var i = 0; i < roomDataList[indexRoom].wsList.length; i++)
        {
            if(roomDataList[indexRoom].wsList[i] == ws)
            {
                isFoundWsInRoom = true;
                break
            }
        }

        if(isFoundWsInRoom)
        {
            eventData.Msg = "already in room";
        }
        else
        {
            let wsData = new WebSocketData(ws, "");
            roomDataList[indexRoom].wsList.push(wsData);
            eventData.Msg = "success";
        }
    }

    ws.send(JSON.stringify(eventData));
}

function UpdateData(ws, data)
{
    console.log("UpdateData");
    for(let i = 0; i < roomDataList.length; i++)
    {
        for(let j = 0; j < roomDataList[i].wsList.length; j++)
        {
            if(ws == roomDataList[i].wsList[j].ws)
            {
                console.log("send from client :" + data);
                roomDataList[i].wsList[j].data = data;
                break;
            }
        }
    }
}

function LeaveRoom(ws)
{
    for(var i = 0; i < roomDataList.length; i++)
    {
        for(var j = 0; j < roomDataList[i].wsList.length; j++)
        {
            if(ws == roomDataList[i].wsList[j].ws)
            {
                roomDataList[i].wsList.splice(j,1);

                if(roomDataList[i].wsList.length <= 0)
                {
                    roomDataList.splice(i, 1);
                }

                var eventData = {
                    Event:"joinRoom",
                    Msg:""
                }

                console.log("client leave room");

                ws.send(JSON.stringify(eventData));
                break;
            }
        }
    }
}

function ArrayRemove(arr, value)
{
    return arr.filter((element)=>{
        return element != value;
    });
}

function Boardcast()
{
    //console.log("Current Room : "+roomDataList.length);

    for(var i = 0; i < roomDataList.length; i++)
    {
        for(var j = 0; j < roomDataList[i].wsList.length; j++)
        {
            for(var k = 0; k < roomDataList[i].wsList.length; k++)
            {
                var ws = roomDataList[i].wsList[k].ws;
                var data = roomDataList[i].wsList[j].data;
                
                if(data != "")
                {
                    console.log("send to client");
                    ws.send(data);
                }
            }
            roomDataList[i].wsList[j].data = "";
        }
    }
}

setInterval(()=>{
    Boardcast();
}, millInterval);

