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
    roomOption:{}
    wsList: []
}
*/

var roomMap = new Map();

wss.on("connection", (ws)=>{

    Connect(ws);

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
                    status:status,
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
        else if(toJsonObj.eventName == "GetPlayerList"){

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

function Connect(ws){

    let uid = uuid.v1();
    let callbackMsg = {
        eventName:"Connect",
        data:uid
    }
    ws.send(JSON.stringify(callbackMsg));
}

function Boardcast(){
    for(let keyRoom of roomMap.keys()){
        let wsList = roomMap.get(keyRoom).wsList;

        for(let keyClient of wsList.keys()){

            let replicateData = wsList.get(keyClient).replicateData;

            if(replicateData != undefined && replicateData != "")
            {
                for(let keyOtherClient of wsList.keys()){

                    if(keyClient != keyOtherClient)
                    {
                        let callbackMsg = {
                            eventName:"ReplicateData",
                            data:replicateData
                        }
    
                        keyOtherClient.send(JSON.stringify(callbackMsg));
                    }
                }
            }

            console.log(replicateData);
        }
    }
}

function CreateRoom(ws, roomOption){

    let roomName = roomOption.roomName;
    let isFoundRoom = roomMap.has(roomName);

    let callbackMsg = {
        eventName:"CreateRoom",
        status:false,
    }

    if(isFoundRoom == true)
    {
        callbackMsg.status = false;
        ws.send(JSON.stringify(callbackMsg));
    }
    else
    {
        roomMap.set(roomName, {
            wsList: new Map(),
            roomOption:roomOption
        });

        roomMap.get(roomName).wsList.set(ws, {});

        callbackMsg.status = true;
        callbackMsg.roomOption = roomOption;
        ws.send(JSON.stringify(callbackMsg));
    }
}

function JoinRoom(ws, roomOption){
    
    let roomName = roomOption.roomName;
    let isFoundRoom = roomMap.has(roomName);

    let callbackMsg = {
        eventName:"JoinRoom",
        status:false,
    }

    if(isFoundRoom === false){
        callbackMsg.status = false;
    }else{
        let isFoundWsInRoom = roomMap.get(roomName).wsList.has(ws);  

        if(isFoundWsInRoom === true){
            callbackMsg.status = false;
        }else{
            roomMap.get(roomName).wsList.set(ws, {});

            callbackMsg.status = true;
            callbackMsg.roomOption = roomMap.get(roomName).roomOption;
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
    let callbackMsg = {
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

setInterval(Boardcast, 1000);