const app = require('express')();
const server = require('http').Server(app);
const uuid = require('uuid');
const websocket = require('ws');
const wss = new websocket.Server({server});
const sqlite = require('sqlite3').verbose();


var database = new sqlite.Database('./database/chatDB.db', sqlite.OPEN_CREATE | sqlite.OPEN_READWRITE, (err)=>{
    if(err) throw err;

    console.log("Connected to database.");
});

server.listen(process.env.PORT || 8080, ()=>{
    console.log("Server start at port "+server.address().port);
});

//var wsList = [];
var roomList = [];
/*
{
    roomOption:{}
    wsList: []
}
*/

wss.on("connection", (ws)=>{
    
    console.log("client connected.");

    ws.id = uuid.v4();
    
    var callbackMsg = {
        eventName:"Connect",
        status:true,
        data: ws.id
    }

    ws.send(JSON.stringify(callbackMsg));

    ws.on("message", (data)=>{
        if(typeof data === 'string')
        {
            var toJsonObj = {
                eventName:""
            }
            toJsonObj = JSON.parse(data);
            //===============================================
    
            if(toJsonObj.eventName == "CreateRoom")//CreateRoom
            {
                /*
                {
                    eventName:"",
                    data:"",
                    roomOption:{}
                }
                */
    
                //============= Find room with roomName from Client =========
                var isFoundRoom = false;
                for(var i = 0; i < roomList.length; i++)
                {
                    if(roomList[i].roomOption.roomName == toJsonObj.roomOption.roomName)
                    {
                        isFoundRoom = true;
                        break;
                    }
                }
                //===========================================================
    
                if(isFoundRoom == true)// Found room
                {
                    //Can't create room because roomName is exist.
                    //========== Send callback message to Client ============
    
                    //ws.send("CreateRoomFail"); 
    
                    //I will change to json string like a client side. Please see below
                    var callbackMsg = {
                        eventName:"CreateRoom",
                        status:false,
                    }
                    var toJsonStr = JSON.stringify(callbackMsg);
                    ws.send(toJsonStr);
                    //=======================================================
    
                    console.log("client create room fail.");
                }
                else
                {
                    //============ Create room and Add to roomList ==========
                    var newRoom = {
                        roomOption: toJsonObj.roomOption,
                        wsList: []
                    }
    
                    newRoom.wsList.push({
                        ws:ws,
                    });
                    roomList.push(newRoom);
    
                    var callbackMsg = {
                        eventName:"CreateRoom",
                        status:true,
                        data: JSON.stringify(toJsonObj.roomOption)
                    }
                    var toJsonStr = JSON.stringify(callbackMsg);
                    ws.send(toJsonStr);
                    //=======================================================
                    console.log("client create room success.");
                }
            }
            else if(toJsonObj.eventName == "JoinRoom")//JoinRoom
            {
                //============= Home work ================
                // Implementation JoinRoom event when have request from client.
                var indexRoom = -1;
                var roomName = toJsonObj.roomOption.roomName;
                for(let i = 0; i < roomList.length; i++)
                {
                    if(roomList[i].roomOption.roomName == roomName)
                    {
                        indexRoom = i;
                        break;
                    }
                }
    
                let eventData = {
                    eventName:"JoinRoom",
                    status:false,
                }
    
                if(indexRoom == -1)
                {
                    console.log("Room : " + roomName + " is not found.");
                    eventData.status = false;
                }
                else
                {
                    var isFoundWsInRoom = false; 
                    for(var i = 0; i < roomList[indexRoom].wsList.length; i++)
                    {
                        if(roomList[indexRoom].wsList[i].ws == ws)
                        {
                            isFoundWsInRoom = true;
                            break
                        }
                    }
    
                    if(isFoundWsInRoom)
                    {
                        eventData.status = false;
                    }
                    else
                    {
                        roomList[indexRoom].wsList.push({
                            ws:ws,
                        });
                        eventData.status = true;
                        eventData.roomOption = JSON.stringify(roomList[indexRoom].roomOption);
                    }
                }
    
                ws.send(JSON.stringify(eventData));
                //================= Hint =================
                //roomList[i].wsList.push(ws);
    
                console.log("client request JoinRoom");
                //========================================
            }
            else if(toJsonObj.eventName == "LeaveRoom")//LeaveRoom
            {
                //============ Find client in room for remove client out of room ================
                var isLeaveSuccess = false;//Set false to default.
                for(var i = 0; i < roomList.length; i++)//Loop in roomList
                {
                    for(var j = 0; j < roomList[i].wsList.length; j++)//Loop in wsList in roomList
                    {
                        if(ws == roomList[i].wsList[j].ws)//If founded client.
                        {
                            roomList[i].wsList.splice(j, 1);//Remove at index one time. When found client.
    
                            if(roomList[i].wsList.length <= 0)//If no one left in room remove this room now.
                            {
                                roomList.splice(i, 1);//Remove at index one time. When room is no one left.
                            }
                            isLeaveSuccess = true;
                            break;
                        }
                    }
                }
                //===============================================================================
    
                if(isLeaveSuccess)
                {
                    //========== Send callback message to Client ============
    
                    //ws.send("LeaveRoomSuccess");
    
                    //I will change to json string like a client side. Please see below
                    var callbackMsg = {
                        eventName:"LeaveRoom",
                        status:true
                    }
                    var toJsonStr = JSON.stringify(callbackMsg);
                    ws.send(toJsonStr);
                    //=======================================================
    
                    console.log("leave room success");
                }
                else
                {
                    //========== Send callback message to Client ============
    
                    //ws.send("LeaveRoomFail");
    
                    //I will change to json string like a client side. Please see below
                    var callbackMsg = {
                        eventName:"LeaveRoom",
                        status:false
                    }
                    var toJsonStr = JSON.stringify(callbackMsg);
                    ws.send(toJsonStr);
                    //=======================================================
    
                    console.log("leave room fail");
                }
            }
            else if(toJsonObj.eventName == "SendMessage")//Send Message
            {
                Boardcast(ws, toJsonObj.data);
            }
            else if(toJsonObj.eventName == "Login"){
                
                var splitData = toJsonObj.data.split('#');
                var userID = splitData[0];
                var password = splitData[1];
                var sqlSelect = `SELECT * FROM UserData WHERE UserID='${userID}' AND Password='${password}'`;
    
                var callbackMsg = {
                    eventName:"Login",
                    data:""
                }
    
                database.all(sqlSelect, (err, rows)=>{
                    if(err)
                    {
                        callbackMsg.data = "fail";
                    }
                    else
                    {
                        if(rows.length > 0)
                        {
                            callbackMsg.data = rows[0].Name;
                        }
                        else
                        {
                            callbackMsg.data = "fail";
                        }
                        
                    }
    
                    var toJsonStr = JSON.stringify(callbackMsg);
                    ws.send(toJsonStr);
                });
    
            }
            else if(toJsonObj.eventName == "Register"){
                
                var splitData = toJsonObj.data.split('#');
                var userID = splitData[0];
                var password = splitData[1];
                var name = splitData[2];
                var sqlInsert = `INSERT INTO UserData (UserID, Password, Name) VALUES ('${userID}', '${password}', '${name}')`;
    
                var callbackMsg = {
                    eventName:"Register",
                    data:""
                }
    
                database.all(sqlInsert, (err, rows)=>{
                    if(err)
                    {
                        callbackMsg.data = "fail";
                    }
                    else
                    {
                        callbackMsg.data = "success";
                    }
    
                    
                    var toJsonStr = JSON.stringify(callbackMsg);
                    ws.send(toJsonStr);
                });
            }
            else if(toJsonObj.eventName == "GetPlayerList"){
    
            }
            else if(toJsonObj.eventName == "ReplicateData"){
                ReplicateData(ws, toJsonObj.data);
            }
            else if(toJsonObj.eventName == "OnceData"){
                OnceData(ws, toJsonObj.data);
            }
            else if(toJsonObj.eventName == "RequestUIDObject"){
                RequestUIDObject(ws);
            }
        }else{
            console.log(data);
            ReplicateData(ws, data);
        }
    });

    ws.on("close", ()=>{
        //============ Find client in room for remove client out of room ================
        for(var i = 0; i < roomList.length; i++)//Loop in roomList
        {
            for(var j = 0; j < roomList[i].wsList.length; j++)//Loop in wsList in roomList
            {
                if(ws == roomList[i].wsList[j].ws)//If founded client.
                {
                    console.log("client leave room");
                    roomList[i].wsList.splice(j, 1);//Remove at index one time. When found client.

                    if(roomList[i].wsList.length <= 0)//If no one left in room remove this room now.
                    {
                        roomList.splice(i, 1);//Remove at index one time. When room is no one left.
                    }
                    break;
                }
            }
        }
        console.log("client disconnected.");
        //===============================================================================
    });
});

function RequestUIDObject(ws)
{
    let uid = uuid.v1();
    var callbackMsg = {
        eventName:"RequestUIDObject",
        data:uid
    }
    ws.send(JSON.stringify(callbackMsg));
}

function ReplicateData(ws, data)
{   
    console.log(data);
    for(let i = 0; i < roomList.length; i++)
    {
        for(let j = 0; j < roomList[i].wsList.length; j++)
        {
            if(ws == roomList[i].wsList[j].ws)
            {
                roomList[i].wsList[j].replicateData = data;
                break;
            }
        }
    }
}

function OnceData(ws,data)
{
    for(let i = 0; i < roomList.length; i++)
    {
        for(let j = 0; j < roomList[i].wsList.length; j++)
        {
            if(ws == roomList[i].wsList[j].ws)
            {
                roomList[i].wsList[j].onceData = data;
                break;
            }
        }
    }
}

function Boardcast()
{
    for(var i = 0; i < roomList.length; i++)
    {
        for(var j = 0; j < roomList[i].wsList.length; j++)
        {
            for(var k = 0; k < roomList[i].wsList.length; k++)
            {
                var ws = roomList[i].wsList[k].ws;
                var replicateData = roomList[i].wsList[j].replicateData;
                var onceData = roomList[i].wsList[j].onceData;
                
                if(replicateData != undefined && replicateData != "")
                {
                    /*var callbackMsg = {
                        eventName:"ReplicateData",
                        replicateData:replicateData
                    }
                    ws.send(JSON.stringify(callbackMsg));*/
                    ws.send(replicateData);
                }

                if(onceData != undefined && onceData != "")
                {
                    var callbackMsg = {
                        eventName:"OnceData",
                        onceData:onceData
                    }
                    ws.send(JSON.stringify(callbackMsg));
                }
            }

            if(roomList[i].wsList[j].onceData != "")
            {
                roomList[i].wsList[j].onceData = "";
            }
        }
    }
}

const serverTickRate = 1;
const millInterval = (1/serverTickRate) * 1000;
setInterval(()=>{
    Boardcast();
}, millInterval);

