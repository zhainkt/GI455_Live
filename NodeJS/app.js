const app = require('express')();
const server = require('http').Server(app);
const websocket = require('ws');
const wss = new websocket.Server({server});
var udid = require('udid');

const admin = require('firebase-admin');
//const serviceAcc = require('./gi455-305013-firebase-adminsdk-x33n5-c2af095e6a.json');
/*admin.initializeApp({
    //credential: admin.credential.applicationDefault()
    credential: admin.credential.cert(serviceAcc),
    databaseURL: "https://gi455chatserver-default-rtdb.firebaseio.com"
});*/
admin.initializeApp();

const db = admin.firestore();

const setData = async function() {
    const citiesRef = db.collection('students');

    fs.readFile('./student.csv', 'utf8', function(err, data) {
        let textLine = data.split(/\n/);
        //var result = textLine.replace(/\r/,'');
        for(let i = 0; i < textLine.length; i++)
        {
            let clearText = textLine[i].replace(/\r/,'');
            let splitData = clearText.split(',');
            let id = splitData[0];
            let name = splitData[1];
            let email = splitData[2];
    
            citiesRef.doc(id).set({
                name: name,
                email: email
            });
        }
        
    });
}

server.listen(process.env.PORT || 8080, ()=>{
    console.log("Server start at port "+server.address().port);
});

//var wsList = [];
var roomList = [];
/*
{
    roomName: ""
    wsList: []
}
*/

wss.on("connection", (ws)=>{
    
    //Lobby
    console.log("client connected.");
    //Reception
    ws.on("message", (data)=>{
        
        EventOrder(ws,data);
    });

    ws.on("close", ()=>{

    });
});

let EventOrder = (ws, data)=>{
    
    let toJsonObj = {
        eventName:"",
        data:{}
    }
    toJsonObj = JSON.parse(data);

    let _eventName = toJsonObj.eventName;
    let _data = toJsonObj.data;
    
    switch(_eventName)
    {
        case "RequestToken" :
        {
            RequestToken(_data.studentID,(result)=>{

                toJsonObj.data = result;

                ws.send(JSON.stringify(toJsonObj));
            });
            break;
        }
        case "GetStudentData":
        {
            GetStudentData(_data.studentID, (result)=>{
                toJsonObj.data = result;

                ws.send(JSON.stringify(toJsonObj));
            });
            break;
        }
    }
}

const RequestToken = async (studentID ,callback)=>{

    const studentRef = db.collection('students').doc(studentID);

    const getDoc = await studentRef.get();

    let result = {
        status: false,
        message: "",
        data:{}
    }

    if (!getDoc.exists) {

        result.status = false;
        result.message = "Can't found data from your student ID";
        callback(result);
    } else {

        let name = getDoc.data().name;
        let token = getDoc.data().token;
        if(token === undefined)
        {
            let newToken = udid(name);
            const tokenRef = db.collection('token');
            const timeStamp = admin.firestore.Timestamp.now();
            const date = new Date(timeStamp * 1000);
            const dateFormat = date.toLocaleString("th-TH", {timeZoneName: "short"});
            await tokenRef.doc(newToken).set({
                unix:timeStamp,
                dateTime:dateFormat
            }).then(studentRef.update({
                token:newToken
            }));

            token = newToken;
        }

        result.status = true;
        result.message = "success";
        result.data = token;
        callback(result);
    }

    //Example 
    /*RequestToken('test', (result)=>{
        console.log(result);
    });*/
}

const GetStudentData = async(studentID, callback)=>{

    const studentRef = db.collection('students').doc(studentID);

    const getDoc = await studentRef.get();

    let result = {
        status: false,
        message: "",
        data:{}
    }

    if (!getDoc.exists) {
        result.status = false;
        result.message = "Can't found data from your student ID";
        callback(result);
    } else {
        result.status = true;
        result.message = "success";
        result.data = getDoc.data();
        callback(result);
    }
};

function Boardcast(data)
{
    /*for(var i = 0; i < wsList.length; i++)
    {
        wsList[i].send(data);
    }*/
}

