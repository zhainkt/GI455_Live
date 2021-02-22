const sqlite3 = require('sqlite3').verbose();

let db = new sqlite3.Database('./sql/test.db', sqlite3.OPEN_READWRITE, (err)=>{
    if(err) throw err;

    console.log('Connected to the test database.');

    //================= Select ==================
    /*db.all("SELECT * FROM PlayerData WHERE PlayerID=1111111", function(err, rows) {

        if(err) throw err;

        console.log(rows);
    });*/

    //================= Insert ===================
    /*db.all("INSERT INTO PlayerData (PlayerID, PlayerName, Level, Money, Rank) VALUES ('1111151', 'unicorn', '1', '0', '0')"
            , function(err, rows) {

        if(err) throw err;

        console.log(rows);
    });*/

    //================= Update ===================
    /*db.all("UPDATE PlayerData SET Money = '500' WHERE PlayerID='1111151'"
            , function(err, rows) {

        if(err) throw err;

        console.log(rows);
    });*/

    //================= Add money ================
    /*db.all("SELECT * FROM PlayerData WHERE PlayerID=1111151"
            , function(err, rows) {

        if(err) throw err;
        
        var currentMoney = rows[0].Money;
        currentMoney = currentMoney + 200;
        
        db.all("UPDATE PlayerData SET Money = '"+currentMoney+"' WHERE PlayerID='1111151'"
            , function(err, rows) {
            if(err) throw err;
        });
    });*/

    db.close((err) => {
        if (err) {
          return console.error(err.message);
        }
        console.log('Close the database connection.');
    });
});