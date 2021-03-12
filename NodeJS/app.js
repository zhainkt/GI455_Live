const app = require('express')();
const server = require('http').Server(app);
const websocket = require('ws');
const wss = new websocket.Server({server});
var udid = require('udid');
//const fs = require('fs');

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
    const propRef = db.collection('proposion').doc('TimeLimit');

    const propDoc = await propRef.get();
    if (propDoc.exists) {
        const propData = propDoc.data();

        fs.readFile('./student3372.csv', 'utf8', function(err, data) {
            let textLine = data.split(/\n/);
            //var result = textLine.replace(/\r/,'');
            for(let i = 0; i < textLine.length; i++)
            {
                let clearText = textLine[i].replace(/\r/,'');
                let splitData = clearText.split(',');
                let id = splitData[0];
                let name = splitData[1];
                let email = splitData[2];
                let startTime = propData.startTimeSec2;
                let endTime = propData.deadLineSec2;
        
                citiesRef.doc(id).set({
                    name: name,
                    email: email,
                    startTime:startTime,
                    endTime:endTime
                });
            }
        });
    }
}

//setData();


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

var wsList = [];

wss.on("connection", (ws)=>{
    
    console.log("client connected.");

    ws.close();

    /*wsList.push({
        ws:ws,
        timeRes:0
    });

    ws.on("message", (data)=>{
    
        const notSpam = CheckTimeStamp(ws);

        if(notSpam){
            console.log(data);
            EventOrder(ws,data);
        }else{
            
            let result = {
                eventName:"SpamDetection",
                message:"Server kick. Because your spam send message to server."
            }

            ws.send(JSON.stringify(result));
            ws.close();

            console.log("Spam Detected.");
        }
        
    });

    ws.on("close", ()=>{
        for(var i = 0 ; i < wsList.length; i++){
            if(wsList[i].ws == ws){
                wsList.splice(i,1);
            }
        }
    });*/
});

function CheckTimeStamp(ws){

    const timeStamp = admin.firestore.Timestamp.now();

    for(var i = 0; i < wsList.length; i++){
        if(wsList[i].ws == ws){
            let sec = timeStamp;
            let compareSec = sec - wsList[i].timeRes;
            wsList[i].timeRes = sec;
            if(compareSec >= 2){
                return true;
            }else{
                return false;
            }

            break;
        }
    }

    return false;
}

const LocalCheckTime = async()=>{
    const studentRef = db.collection('students');

    const getDoc = await studentRef.get();

    getDoc.docs.map((doc)=>{

        const data = doc.data();

        if(data.endTime == undefined || data.startTime == undefined)
        {
            console.log(data.name);
        }
        else
        {
            let timeLeft = data.endTime - data.startTime;

            console.log(data.name+" : " + timeLeft);
        }
    });
}

let EventOrder = (ws, data)=>{
    
    jsonEvent = JSON.parse(data);
    
    switch(jsonEvent.eventName)
    {
        //case "StartExam": //for test
        case "RequestToken" :
        {
            let toJsonObj = JSON.parse(data);

            RequestToken(toJsonObj.studentID,(resToken)=>{

                if(resToken.status === false){

                    ws.send(JSON.stringify(resToken));

                }else{
                    RequestPropLink(resToken.token, (resProp)=>{

                        if(resProp.status === false){
                            ws.send(JSON.stringify(resProp));
                        }else{
                            let result = {
                                eventName:jsonEvent.eventName,
                                token:resToken.token,
                                link:resProp.data
                            }

                            ws.send(JSON.stringify(result));
                        }    
                    });
                }
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
            let toJsonObj = JSON.parse(data);

            RequestExamInfo(toJsonObj.token, (result)=>{
                ws.send(JSON.stringify(result));
            });
            break;
        }
        case "SendAnswer":
        {
            let toJsonObj = JSON.parse(data);

            SendAnswer(toJsonObj.token, toJsonObj.answer, (result)=>{
                ws.send(JSON.stringify(result));
            });
            break;
        }

    }
}

const RequestToken = async (studentID ,callback)=>{
    let result = {
        eventName:"RequestToken",
        status: false,
        message: "",
    }

    if(studentID == undefined || studentID == ""){
        result.status = false;
        result.message = "Request fail your studentID is empty string.";
        callback(result);
    }else{
        const studentRef = db.collection('students').doc(studentID);
        const getDoc = await studentRef.get();

        if (!getDoc.exists) {

            result.status = false;
            result.message = "Can't found data from your student ID";
            callback(result);
        } else {
    
            let name = getDoc.data().name;
            let startTime = getDoc.data().startTime;
            let token = getDoc.data().token;
            const timeStamp = admin.firestore.Timestamp.now();
            const date = new Date(timeStamp * 1000);
            const dateFormat = date.toLocaleString("th-TH", {timeZoneName: "short"});

            if(timeStamp >= startTime)
            {
                if(token === undefined)
                {
                    let newToken = udid(name);
                    const tokenRef = db.collection('token');
                    const proposionRef = db.collection('proposion');
                    const propDoc = await proposionRef.doc('allprop').get();
                    const propData = propDoc.data();
                    const propArr = propData.data;
                    var randomProp = RandomCharFromList(propArr);
    
                    await tokenRef.doc(newToken).set({
                        unix:timeStamp,
                        dateTime:dateFormat,
                        prop:randomProp,
                        startTime:getDoc.data().startTime,
                        endTime:getDoc.data().endTime
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
            else
            {
                result.status = false;
                result.message = "Your test has not started.";
                callback(result);
            }
            
        }
    }
};

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

const RequestPropLink = async(token, callback)=>{
    let result = {
        eventName:"RequestToken",
        status: false,
        message: "",
    }

    if(token == undefined || token == ""){
        result.status = false;
        result.message = "Request fail your token is empty string.";
        callback(result);
    }else{
        const tokenRef = db.collection('token').doc(token);
        const tokenDoc = await tokenRef.get();

        if (!tokenDoc.exists) {
            result.status = false;
            result.message = "Token is not found.";
            callback(result);
        }else{

            const tokenData = tokenDoc.data();
            const proposionRef = db.collection('proposion').doc(tokenData.prop);
            const propDoc = await proposionRef.get();

            if(!propDoc.exists){
                result.status = false;
                result.message = "Proposition ["+tokenData.prop+"] is not found.";
                callback(result);
            }else{

                result.status = true;
                result.message = "success";
                result.data = propDoc.data().link;
                callback(result);
            }
        }
    }
};

const RequestExamInfo = async(token, callback)=>{
    let result = {
        eventName:"RequestExamInfo",
        status: false,
    }

    if(token == undefined || token == ""){
        result.status = false;
        result.message = "Request fail your token is empty string.";
        callback(result);
    }else{
        const tokenRef = db.collection('token').doc(token);
        const tokenDoc = await tokenRef.get();

        if (!tokenDoc.exists) {
            result.status = false;
            result.message = "Token is not found.";
            callback(result);
        }else{

            const tokenData = tokenDoc.data();
            const timeStamp = admin.firestore.Timestamp.now();

            if(tokenData.data === undefined && tokenData.answer === undefined)
            {
                if(tokenData.prop === "A0"){

                    let a0Data = GetA0();
                    tokenRef.update({
                        data:a0Data.data,
                        answer:a0Data.answer
                    });

                    result.status = true;
                    result.data = a0Data.data;
    
                    callback(result);

                }else if(tokenData.prop === "A1"){

                    let a1Data = GetA1();
                    tokenRef.update({
                        data:a1Data.data,
                        answer:a1Data.answer,
                    });

                    result.status = true;
                    result.data = a1Data.data;
    
                    callback(result);
    
                }else if(tokenData.prop === "A2"){
    
                    let a2Data = GetA2();
                    tokenRef.update({
                        data:a2Data.data,
                        answer:a2Data.answer,
                    });

                    result.status = true;
                    result.data = a2Data.data;
    
                    callback(result);

                }else if(tokenData.prop === "A3"){
    
                    let a3Data = GetA3();
                    tokenRef.update({
                        data:a3Data.data,
                        answer:a3Data.answer,
                    });

                    result.status = true;
                    result.data = a3Data.data;
    
                    callback(result);
                    
                }else if(tokenData.prop === "A4"){
    
                    let a4Data = GetA4();
                    tokenRef.update({
                        data:a4Data.data,
                        answer:a4Data.answer,
                    });

                    result.status = true;
                    result.data = a4Data.data;
    
                    callback(result);

                }else if(tokenData.prop === "A5"){

                    let a5Data = GetA5();
                    tokenRef.update({
                        data: a5Data.data,
                        answer: a5Data.answer
                    });
    
                    result.status = true;
                    result.data = a5Data.data;
    
                    callback(result);
                }

            }else{
                result.status = true;
                result.data = tokenData.data;
                callback(result);
            }
        }
    }
}

const SendAnswer = async(token, answer, callback)=>{

    let result = {
        eventName:"SendAnswer",
        status: false,
    }

    if(token == undefined || token == ""){
        result.status = false;
        result.message = "Request fail your token is empty string.";
        callback(result);
    }else{

        const tokenRef = db.collection('token').doc(token);
        const tokenDoc = await tokenRef.get();

        if (!tokenDoc.exists) {
            result.status = false;
            result.message = "Token is not found.";
            callback(result);
        }else{
            const tokenData = tokenDoc.data();
            const timeStamp = admin.firestore.Timestamp.now();

            

            if(tokenData.answer == undefined || tokenData.data == undefined || tokenData.endTime == undefined)
            {
                let addCount = 1;
                if(tokenData.count == undefined)
                {
                    tokenRef.update({
                        count:addCount
                    });
                }
                else
                {
                    addCount = tokenData.count + 1;

                    tokenRef.update({
                        count:addCount
                    })
                }

                console.log(tokenData.answer == undefined);
                console.log(tokenData.data == undefined);
                console.log(tokenData.endTime == undefined);

                console.log("SendAnswer : fail [1]");
                result.status = false;
                result.message = "Your answer is wrong " + addCount + " time.";
                callback(result);
            }
            else
            {
                if(tokenData.score != undefined)
                {
                    result.status = true;
                    result.message = "Your exam is finish. Total score is " + tokenData.score;
                    callback(result);
                }
                else
                {
                    console.log(timeStamp);
                    console.log(tokenData.endTime);
                    console.log("SendAnswer : currentTime = " + timeStamp +", endTime = " + tokenData.endTime);
                    if(timeStamp > tokenData.endTime)
                    {
                        tokenRef.update({
                            score:0
                        });

                        result.status = false;
                        result.message = "You submitted the exam too late.";
                        callback(result);
                    }
                    else
                    {
                        if(answer == undefined || answer == ""){
                            let addCount = 1;
                            if(tokenData.count == undefined)
                            {
                                tokenRef.update({
                                    count:addCount
                                });
                            }
                            else
                            {
                                addCount = tokenData.count + 1;
            
                                tokenRef.update({
                                    count:addCount
                                })
                            }
            
                            console.log("SendAnswer : fail [2]");
                            result.status = false;
                            result.message = "Your answer is wrong " + addCount + " time.";
                            callback(result);
            
                        }else{
                            
                            let addCount = 1;
                            if(tokenData.count == undefined)
                            {
                                tokenRef.update({
                                    count:addCount
                                });
                            }
                            else
                            {
                                addCount = tokenData.count + 1;
            
                                tokenRef.update({
                                    count:addCount
                                })
                            }
        
                            if(tokenData.answer === answer)
                            {
                                let score = 100 - (tokenData.count*20);
        
                                tokenRef.update({
                                    score:score
                                })
        
                                result.status = true;
                                result.message = "Your answer is correct. Total score is " + score;
                                callback(result);
                            }
                            else
                            {
                                console.log("SendAnswer : fail [3]");
                                result.status = false;
                                result.message = "Your answer is wrong " + addCount + " time.";
                                callback(result);
                            }
                        }
                    }
                }
            }
        }

    }
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

function GetA1(){
    let str = "";
    let lengthArr = 1000;

    for(let i = 0; i < lengthArr; i++)
    {
        if(i == lengthArr-1)
        {
            str += RandomCharAlphabet();
        }
        else
        {
            str += RandomCharAlphabet()+",";
        }
    }

    let splitStr = str.split(',');
    let answer = "";
    for(let i = splitStr.length-1; i >= 0; i--)
    {
        if(i == 0)
        {
            answer += splitStr[i];
        }
        else
        {
            answer += splitStr[i] +",";
        }
    }


    let result = {
        data:str,
        answer:answer
    }
    
    return result;
}

function GetA2(){
    let str = "";
    let lengthArr = 1000;

    for(let i = 0; i < lengthArr; i++)
    {
        if(i == lengthArr-1)
        {
            str += RandomCharAlphabet();
        }
        else
        {
            str += RandomCharAlphabet()+",";
        }
    }

    let splitStr = str.split(',');
    let answer = "";
    for(let i = 0 ; i < splitStr.length; i++)
    {
        let isEven = i % 2 == 0;
        if(!isEven)
        {
            if(answer == "")
            {
                answer += splitStr[i];
            }
            else
            {
                answer += ","+splitStr[i];
            }
            
        }
    }

    let result = {
        data:str,
        answer:answer
    }
    
    return result;
}

function GetA3()
{
    let arrInt = [];
    var str = "";
    let lengthArr = 1000;
    let answerSum = 0;

    for(let i = 0; i < lengthArr; i++)
    {
        let random = RandomRange(1, 999);
        arrInt.push(random);

        if(i == lengthArr-1)
        {
            str += ""+random.toString();
        }else{
            str += random.toString()+",";
        }

        answerSum += random;
    }

    let result = {
        data:str,
        answer:answerSum.toString()
    }
    
    return result;
}

function GetA4(){

    var str = "";
    let lengthArr = 1000;

    for(let i = 0; i < lengthArr; i++)
    {
        let randomChar = RandomChar("@#$%&abcdefghijklm@#$%&nopqrstuvwxyz01234567@#$%&89ABCDEFGHIJKLMNOPQRSTUVWX@#$%&YZ1234567890")
        if(str == "")
        {
            str += randomChar;
        }
        else
        {
            str += ","+randomChar;
        }
    }

    let split = str.split(',');
    let answer = "";

    for(let i = 0; i < split.length; i++)
    {
        let _str = split[i];

        if(_str != "@" && _str != "#" && _str != "$" &&
            _str != "%" && _str != "&")
        {
            if(answer == "")
            {
                answer += _str;
            }
            else
            {
                answer += ","+_str;
            }
        }
    }

    let result = {
        data:str,
        answer:answer
    }

    return result;
}

function GetA5(){

    let arrInt = [];
    var str = "";
    let lengthArr = 1000;

    for(let i = 0; i < lengthArr; i++)
    {
        let random = RandomRange(1, 999);
        arrInt.push(random);

        if(i == lengthArr-1)
        {
            str += ""+random.toString();
        }else{
            str += random.toString()+",";
        }
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
        data:str,
        answer:answerSum.toString()
    }

    return result;
}

function GetA0(){

    let arrInt = [];
    var str = "";
    let lengthArr = 1000;

    for(let i = 0; i < lengthArr; i++)
    {
        let random = RandomRange(1, 999);
        arrInt.push(random);

        if(i == lengthArr-1)
        {
            str += ""+random.toString();
        }else{
            str += random.toString()+",";
        }
    }

    let answerSum = 0;

    answerSum = arrInt[978];

    let result = {
        data:str,
        answer:answerSum.toString()
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