var mongo = require('mongodb').MongoClient;
var url = "mongodb://localhost:27017/";

mongo.connect(url, {useUnifiedTopology: true} , (err,db)=>{
    if(err) throw err;
    var database = "gi455";
    var selectDB = db.db(database);

    //================ Create Collection ==================
    /*selectDB.createCollection("students", (err,res)=>{
        if(err)throw err;
    });*/

    //================ Insert Data ========================
    /*var newData = {
        id:34125123,
        name:"phawit"
    }
    selectDB.collection("students").insertOne(newData, (err, res)=>{
        if(err) throw err;
        db.close();
    });*/

    //================ Find Data ==========================
    /*var query = { name:/^p/ };
    selectDB.collection("students").find(query).toArray((err, result)=>{
        if(err) throw err;
        console.log(result);
        db.close();
    });*/

    //================= Update Data ========================
    /*var query = { id: 123456789 };
    var updateData = { $set: {money:500} };
    selectDB.collection("students").updateOne(query, updateData, (err, result)=>{
        if(err) throw err;
        console.log(result.result.nModified);
        db.close();
    });*/

    //================== Example Add money ==================
    /*var query = { id: 123456789 };
    var addedMoney = 200;
    var collection = selectDB.collection("students");
    collection.findOne(query, (err,result)=>{
        if(err) throw err;

        if(result != null)
        {
            var currentMoney = result.money;
            currentMoney = currentMoney + addedMoney;

            var updateData = {
                $set: { money:currentMoney }
            }

            collection.updateOne(query,updateData,(err, result)=>{
                if(err) throw err;

                console.log(result.result.nModified);

                db.close();
            });
        }
    });*/

});