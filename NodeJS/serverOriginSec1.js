const app = require('express')();
const server = require('http').Server(app);
const websocket = require('ws');
const wss = new websocket.Server({server});
const sqlite = require('sqlite3').verbose();
const uuid = require('uuid');


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
    roomName:""
    wsList: []
}
*/

var roomMap = new Map();

wss.on("connection", (ws)=>{
    
    ClientConnect(ws);
    console.log("client connected.");

    ws.on("message", (data)=>{
        //console.log("send from client :"+ data);
        var toJsonObj = {
            eventName:""
        }
        toJsonObj = JSON.parse(data);
        //===============================================

        if(toJsonObj.eventName == "CreateRoom")//CreateRoom
        {
            CreateRoom(ws, toJsonObj.roomOption);
        }
        else if(toJsonObj.eventName == "JoinRoom")//JoinRoom
        {
            JoinRoom(ws, toJsonObj.roomOption);
        }
        else if(toJsonObj.eventName == "LeaveRoom")//LeaveRoom
        {
            LeaveRoom(ws, (status, roomKey)=>{

                let callbackMsg = {
                    eventName:"LeaveRoom",
                    status:true
                }

                if(status === false){
                    callbackMsg.status = false;
                }

                ws.send(JSON.stringify(callbackMsg));

                if(roomMap.get(roomKey).wsList.size <= 0){
                    roomMap.delete(roomKey);
                }
            });
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
        else if(toJsonObj.eventName == "RequestUIDObject"){
            RequestUIDObject(ws);
        }
        else if(toJsonObj.eventName == "ReplicateData"){
            ReplicateData(ws, toJsonObj.roomName, toJsonObj.data);
        }
    });

    ws.on("close", ()=>{
        
        LeaveRoom(ws, (status, roomKey)=>{

            if(status === true){
                if(roomMap.get(roomKey).wsList.size <= 0){
                    roomMap.delete(roomKey);
                }
            }
        });
    });
});

function ClientConnect(ws){
    ws.uid = uuid.v1();

    let callbackMsg = {
        eventName:"Connect",
        data:ws.uid
    }

    ws.send(JSON.stringify(callbackMsg));
}

function Boardcast()
{
    for(let keyRoom of roomMap.keys())
    {
        let wsList = roomMap.get(keyRoom).wsList;

        for(let keyClient of wsList.keys())
        {
            for(let keyOtherClient of wsList.keys())
            {
                let otherWs = keyOtherClient;
                let replicateData = wsList.get(keyClient).replicateData;

                if(replicateData != undefined && replicateData != "")
                {
                    if(keyClient != keyOtherClient)
                    {
                        let callbackMsg = {
                            eventName:"ReplicateData",
                            data:replicateData
                        }
    
                        otherWs.send(JSON.stringify(callbackMsg));
                    }
                }

                console.log(replicateData);
            }
        }
    }
}

function CreateRoom(ws, roomOption){

    var isFoundRoom = roomMap.has(roomOption.roomName); 

    if(isFoundRoom === true)
    {
        var callbackMsg = {
            eventName:"CreateRoom",
            status:false,
        }
        var toJsonStr = JSON.stringify(callbackMsg);
        ws.send(toJsonStr);
    }
    else
    {
        let roomName = roomOption.roomName;
        roomMap.set(roomName, {
            roomOption:roomOption,
            wsList: new Map()
        });

        roomMap.get(roomName).wsList.set(ws, {});

        var callbackMsg = {
            eventName:"CreateRoom",
            status:true,
            data: JSON.stringify(roomOption)
        }
        var toJsonStr = JSON.stringify(callbackMsg);
        ws.send(toJsonStr);
    }
}

function JoinRoom(ws, roomOption){

    let roomName = roomOption.roomName;
    let isFoundRoom = roomMap.has(roomName);

    let callbackMsg = {
        eventName:"JoinRoom",
        status:false,
    }

    if(isFoundRoom === false)
    {
        callbackMsg.status = false;
    }
    else
    {
        let isFoundClientInRoom = roomMap.get(roomName).wsList.has(ws); 

        if(isFoundClientInRoom === true)
        {
            callbackMsg.status = false;
        }
        else
        {
            roomMap.get(roomName).wsList.set(ws, {});
            callbackMsg.status = true;
            callbackMsg.data = JSON.stringify(roomMap.get(roomName).roomOption);
        }
    }
    ws.send(JSON.stringify(callbackMsg));
}

function LeaveRoom(ws, callback){
    for(let roomKey of roomMap.keys()){
        if(roomMap.get(roomKey).wsList.has(ws)){
            callback(roomMap.get(roomKey).wsList.delete(ws), roomKey);
            return;
        }
    }

    callback(false, "");
    return;
}

function RequestUIDObject(ws){

    let uid = uuid.v1();
    var callbackMsg = {
        eventName:"RequestUIDObject",
        data:uid
    }

    ws.send(JSON.stringify(callbackMsg));
}

function ReplicateData(ws, roomName, data){

    roomMap.get(roomName).wsList.set(ws, {
        replicateData:data
    });
}

setInterval(Boardcast, 100);