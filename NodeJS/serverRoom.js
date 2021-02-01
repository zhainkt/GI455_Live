const app = require('express')();
const server = require('http').Server(app);
const { isRegExp } = require('util');
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

let roomDataList = [];

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
})

function Event(ws, receive)
{
    let jsonObj = JSON.parse(receive);
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
            break;
        }
        case "leaveRoom":
        {
            LeaveRoom(ws);
            break;
        }
        default:{}
    }
}

function CreateRoom(ws, roomName)
{
    let wsData = new WebSocketData(ws, "");
    let roomID = require('udid')('roomID');
    let roomData = new RoomData(roomID, roomName, [wsData]);
    let isExistRoom = false;

    for(let i = 0; i < roomDataList.length; i++)
    {
        if(roomDataList[i].roomName == roomName)
        {
            isExistRoom = true;
            break;
        }
    }

    let eventData = {
        event:"createRoom",
        msg:""
    }

    eventData.msg = isExistRoom ? "fail" : "success";
    
    if(eventData.msg == "success"){
        roomDataList.push(roomData);
        let lastIndex = roomDataList.length-1;
        console.log("RoomID : " + roomDataList[lastIndex].roomID);
        console.log("RoomName : " + roomDataList[lastIndex].roomName);
        console.log("Current Player : " + roomDataList[lastIndex].wsList.length);
    }

    ws.send(JSON.stringify(eventData)); 
}

function LeaveRoom(ws)
{
    for(let i = 0; i < roomDataList.length; i++)
    {
        for(let j = 0; j < roomDataList[i].wsList.length; j++)
        {
            if(ws == roomDataList[i].wsList[j].ws)
            {
                roomDataList[i].wsList.splice(j,1);

                if(roomDataList[i].wsList.length <= 0)
                {
                    roomDataList.splice(i, 1);
                }

                console.log("client leave room");
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
    console.log("Current Room : "+roomDataList.length);

    for(let i = 0; i < roomDataList.length; i++)
    {
        for(let j = 0; j < roomDataList[i].wsList.length; j++)
        {
            let ws = roomDataList[i].wsList[j].ws;
            let data = roomDataList[i].wsList[j].data;
            roomDataList[i].wsList[j].data = "";
            
            if(data != "")
                ws.send(data);
        }
    }
}

setInterval(()=>{
    Boardcast();
}, 1000);

