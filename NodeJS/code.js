const udid = require('uuid-random');

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
        result.correctIndex.push(correntIndex[i]);
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
        answer += splitChar[data.correctIndex[i]];
    }

    return answer;
}

/*for(let i = 0; i < 1; i++)
{
    let getUdid = udid()+udid()+udid();
    let hashData = GenerateHashFromWord(getUdid, "@");
    let answer = GetWordFromHash(hashData);
    
    console.log(getUdid);
    console.log(hashData);
    console.log(answer);
    console.log(getUdid == answer);

    var pass = getUdid === answer;

    if(pass == false)
    {
        console.log("fail at index : "+i);
        break;
    }
}*/



function RandomRange(min, max) {
    return Math.floor(Math.random() * (max - min) ) + min;
}

function RandomChar(){
    let allChar = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
    let charList = allChar.split('');
    let randomIndex = RandomRange(0, charList.length);
    return charList[randomIndex];
}

function RandomPrefix()
{
    let allChar = "@#$%&";
    let charList = allChar.split('');
    let randomIndex = RandomRange(0, charList.length);
    return charList[randomIndex];
}

//=================== A5 =====================
var aa = [1,2,3,4]

var answer = 0;


console.time('loop');

for(var i = 0; i < aa.length; i++)
{
    var isEven = (i % 2) == 0;

    if(!isEven)
    {
        console.log(i + (": - : ")+ isEven);
        answer -= aa[i];
    }
    else
    {
        console.log(i + (": + : ")+isEven);
        answer += aa[i];
    }
}

console.timeEnd('loop');

let a = {
    name:"test",
    level:99
}

let b = {
    name:"test2",
    level:11
}
//============================================