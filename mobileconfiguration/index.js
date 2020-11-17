var express = require('express');
var app = express();

app.get('/configuration/:id',
    function(req, res)
    {        
        var sql = require("mssql");

        // config for your database
        var config = {
            user: 'mobileconfig',
            password: 'Se9RqFy~FE-y',
            server: 'den1.mssql7.gear.host',
            database: 'mobileconfig'
        };

        // connect to your database
        sql.connect(config, function (err) {

          if (err) console.log(err);

          // create Request object
          var request = new sql.Request();

            var query =
            "select top 1 DeviceIdentifier, ClientId, ClientSecret, EstateManagement, TransactionProcessorACL from vwDeviceConfiguration where DeviceIdentifier = '" + req.params.id + "'";

          console.log(query);
          // query to the database and get the records
            request.query(query, function (err, recordset)
          {
            if (err)
              console.log(err);

              var record = recordset.recordsets[0][0];
              console.log(JSON.stringify(record));

            // send records as a response
            var result = {
              "deviceIdentifier": record["DeviceIdentifier"],
              "clientId": record["ClientId"],
              "clientSecret": record["ClientSecret"],
              "estateManagement": record["EstateManagement"],
              "transactionProcessorACL": record["TransactionProcessorACL"],
            }
            //console.log(JSON.stringify(result));
            res.send(JSON.stringify(result));
          });
        });
    });

module.exports = app;    