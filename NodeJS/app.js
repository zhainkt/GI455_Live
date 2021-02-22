const app = require('express')();
const server = require('http').Server(app);
const websocket = require('ws');
const wss = new websocket.Server({server});
var udid = require('udid');

const admin = require('firebase-admin');
const serviceAcc = require('./gi455-305013-firebase-adminsdk-x33n5-c2af095e6a.json');
admin.initializeApp({
    //credential: admin.credential.applicationDefault()
    credential: admin.credential.cert(serviceAcc),
    databaseURL: "https://gi455chatserver-default-rtdb.firebaseio.com"
});
//admin.initializeApp();

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
        
        console.log(data);
        EventOrder(ws,data);
    });

    ws.on("close", ()=>{

    });
});

let EventOrder = (ws, data)=>{
    
    jsonEvent = JSON.parse(data);
    
    switch(jsonEvent.eventName)
    {
        case "RequestToken" :
        {
            let toJsonObj = JSON.parse(data);

            RequestToken(toJsonObj.studentID,(result)=>{

                ws.send(JSON.stringify(result));
            });
            break;
        }
        case "GetStudentData":
        {
            let toJsonObj = JSON.parse(data);

            GetStudentData(toJsonObj.studentID, (result)=>{

                ws.send(JSON.stringify(result));
            });
            break;
        }
        case "RequestExamInfo":
        {
            break;
        }
    }
}

const RequestToken = async (studentID ,callback)=>{

    const studentRef = db.collection('students').doc(studentID);

    const getDoc = await studentRef.get();

    let result = {
        eventName:"RequestToken",
        status: false,
        message: "",
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
            const prop = RandomCharFromList(["A1","A2","A3","A4","A5"]);
            await tokenRef.doc(newToken).set({
                unix:timeStamp,
                dateTime:dateFormat,
                prop:prop
            }).then(studentRef.update({
                token:newToken
            }));

            token = newToken;
        }

        result.status = true;
        result.message = "success";
        result.token = token;
        callback(result);
    }
}

const GetStudentData = async(studentID, callback)=>{

    console.log(studentID);
    
    let result = {
        eventName:"GetStudentData",
        status: false,
        message: "",
    }

    if(studentID == undefined || studentID == "")
    {
        result.status = false;
        result.message = "Can't found data from your student ID";
        callback(result);
    }
    else
    {
        const studentRef = db.collection('students').doc(studentID);

        const getDoc = await studentRef.get();
    
        if (!getDoc.exists) {
            result.status = false;
            result.message = "Can't found data from your student ID";
            callback(result);
        } else {

            var docData = getDoc.data();
            result.status = true;
            result.message = "success";
            result.studentName = docData.name;
            result.studentEmail = docData.email;
            callback(result);
        }
    }
};

const A5 = async(token, callback)=>{ 
    const tokenRef = db.collection('token').doc(token);

    const getDoc = await tokenRef.get();

    var result = {};

    if(!getDoc.exists){
        result.status = false;
    }else{
        var docData = getDoc.data();

        result.status = true;

        if(docData.data == undefined || docData.data === ""){
            
            let a5Data = GetA5();
            
            tokenRef.update({
                data: a5Data.data,
                answer: a5Data.answer
            });

            result.data = a5Data.data;
        }else{

            result.data = docData.data;
        }
    }

    callback(result);
}

function GenerateHashFromWord(_word, _prefix){
    let answer = _word;//"apple"
    let prefix = _prefix;//"@";
    let arrAnswer = answer.split('');
    let lengthPerChar = [];
    let correntIndex = [];

    for(let i = 0; i < arrAnswer.length; i++)
    {
        let range = RandomRange(20,51);
        correntIndex.push(RandomRange(0,range));
        lengthPerChar.push(range);
    }

    let hash = "";

    for(let i = 0; i < lengthPerChar.length; i++)
    {
        let text = "";
        for(let j = 0; j < lengthPerChar[i]; j++)
        {
            if(correntIndex[i] == j)
            {
                text += arrAnswer[i];
            }
            else
            {
                text += RandomChar();
            }
        }

        if(i == lengthPerChar.length - 1)
        {
            hash += text;
        }
        else
        {
            hash += text+prefix;
        }
    }

    let result = {
        hash:hash,
        prefix:prefix,
        correctIndex:[]
    }

    for(let i = 0; i < correntIndex.length; i++)
    {
        result.correctIndex.push(correntIndex);
    }

    return result;
}

function GetWordFromHash(data)
{
    let prefix = data.prefix;
    let splitWord = data.hash.split(prefix);

    let answer = "";
    let charList = [];

    for(let i = 0; i < splitWord.length; i++)
    {
        let splitChar = splitWord[i].split('');
        answer += splitChar[data["p"+(i+1)]];
    }

    return answer;
}

function GetA5(){

    let arrInt = [];
    let arrStr = [];
    let lengthArr = 50;

    for(let i = 0; i < lengthArr; i++)
    {
        let random = RandomRange(1, 999);
        arrInt.push(random);
        arrStr.push(random.toString());
    }

    let answerSum = 0;

    for(let i = 0; i < arrInt.length; i++)
    {
        let isEven = (i % 2) == 0;

        if(!isEven)
        {
            answerSum -= arrInt[i];
        }
        else
        {
            answerSum += arrInt[i];
        }
    }

    let result = {
        data:arrStr,
        answer:answerSum
    }

    return result;
}

function RandomRange(min, max) {
    return Math.floor(Math.random() * (max - min) ) + min;
}

function RandomCharFromList(charList){
    let randomIndex = RandomRange(0, charList.length);
    return charList[randomIndex];
}

function RandomChar(allstr){
    let charList = allstr.split('');
    return RandomCharFromList(charList);
}

function RandomCharAlphabet(){
    return RandomChar("abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890");
}

function RandomPrefix(){
    return RandomChar("@#$%&");
}