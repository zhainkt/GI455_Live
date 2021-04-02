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

var roomMap = new Map();

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

        let toJsonObj = JSON.parse(data);

        if(toJsonObj.eventName == "CreateRoom")
        {
            CreateRoom(ws, toJsonObj.roomOption);
        }
        else if(toJsonObj.eventName == "JoinRoom")
        {
            JoinRoom(ws, toJsonObj.roomOption);
        }
        else if(toJsonObj.eventName == "LeaveRoom")//LeaveRoom
        {
            LeaveRoom(ws, (status)=>{

                let callbackMsg = {
                    eventName:"LeaveRoom",
                    status:true
                }

                if(status){
                    ws.send(JSON.stringify(callbackMsg));
                }else{
                    callbackMsg.status = false;
                    var toJsonStr = JSON.stringify(callbackMsg);
                    ws.send(toJsonStr);
                }

                for(let roomKey of roomMap.keys()){
                    if(roomMap.get(roomKey).wsList.size <= 0)
                    {
                        roomMap.delete(roomKey);
                        break;
                    }
                }
            });
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
            ReplicateData(ws, toJsonObj.roomName, toJsonObj.data);
        }
        else if(toJsonObj.eventName == "OnceData"){
            OnceData(ws, toJsonObj.roomName, toJsonObj.data);
        }
        else if(toJsonObj.eventName == "RequestUIDObject"){
            RequestUIDObject(ws);
        }
    });

    ws.on("close", ()=>{
        LeaveRoom(ws, (status)=>{

            for(let roomKey of roomMap.keys()){
                if(roomMap.get(roomKey).wsList.size <= 0)
                {
                    roomMap.delete(roomKey);
                    break;
                }
            }
        });
    });
});

function CreateRoom(ws, roomOption)
{
    let roomName = roomOption.roomName;
    let isFoundRoom = roomMap.has(roomName);

    if(isFoundRoom == true)
    {
        let callbackMsg = {
            eventName:"CreateRoom",
            status:false,
        }

        let toJsonStr = JSON.stringify(callbackMsg);
        ws.send(toJsonStr);
    }
    else
    {
        roomMap.set(roomName, {
            roomOption: roomOption,
            wsList: new Map(),
        });

        roomMap.get(roomName).wsList.set(ws, {});

        let callbackMsg = {
            eventName:"CreateRoom",
            status:true,
            data: JSON.stringify(roomOption)
        }
        ws.send(JSON.stringify(callbackMsg));

        console.log(roomMap.get(roomName).wsList.has(ws));
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

        if(roomMap.get(roomKey).wsList.has(ws))
        {
            callback(roomMap.get(roomKey).wsList.delete(ws));
            return;
        }
    }

    callback(false);
    return;
}

function RequestUIDObject(ws)
{
    let uid = uuid.v1();
    var callbackMsg = {
        eventName:"RequestUIDObject",
        data:uid
    }
    ws.send(JSON.stringify(callbackMsg));
}

function ReplicateData(ws, roomName, data)
{   
    roomMap.get(roomName).wsList.set(ws, {
        replicateData:data
    });
}

function OnceData(ws, roomName, data)
{
    roomMap.get(roomName).wsList.set(ws, {
        onceData:data
    });
}

function Boardcast()
{
    for(let keyRoom of roomMap.keys()){

        let wsList = roomMap.get(keyRoom).wsList;

        for(let keyClient of wsList.keys()){
            for(let keyOtherClient of wsList.keys())
            {
                var ws = keyOtherClient;
                var replicateData = wsList.get(keyClient).replicateData;
                var onceData = wsList.get(keyClient).onceData;

                if(replicateData != undefined && replicateData != "")
                {
                    let callbackMsg = {
                        eventName:"ReplicateData",
                        data:replicateData
                    }
                    ws.send(JSON.stringify(callbackMsg));
                }

                if(onceData != undefined && onceData != "")
                {
                    var callbackMsg = {
                        eventName:"OnceData",
                        onceData:onceData
                    }
                    ws.send(JSON.stringify(callbackMsg));
                }

                //console.log(replicateData);
            }

            if(wsList.get(keyClient).onceData != "")
            {
                wsList.get(keyClient).onceData = "";
            }
        }
    }
}

const serverTickRate = 64;
const millInterval = (1/serverTickRate) * 1000;
setInterval(()=>{
    Boardcast();
}, millInterval);

