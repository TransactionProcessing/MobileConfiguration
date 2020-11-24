var express = require('express');
const { Deta } = require('deta');
const deta = Deta('a0f24j3l_aWcCT1TxDYep2DzQpNwFdP832EzQ11cG'); // configure your Deta project
const mobileLogsDatabase = deta.Base('mobilelogs');  // access your DB
const mobileConfigDatabase = deta.Base('mobileconfig');  // access your DB

var app = express();

app.use(express.json());

app.post('/configuration', async function(req,res)
{
  // create the new config entry message
  var configEntry = {
    key: req.body.deviceIdentifier,
    deviceIdentifier: req.body.deviceIdentifier,
    clientId: req.body.clientId,
    clientSecret: req.body.clientSecret,
    securityServiceUri: req.body.securityServiceUri,
    estateManagementUri: req.body.estateManagementUri,
    transactionProcessorACLUri: req.body.transactionProcessorACLUri,
    logLevel: req.body.logLevel    
  };
  // Write to the database
  const insertedConfigEntry = await mobileConfigDatabase.put(configEntry);

  // return a success
  res.status(201).json(insertedConfigEntry);
});

app.get('/configuration/:id',
    async function(req, res)
    {               
        var config = await mobileConfigDatabase.get(req.params.id);
      
        // send records as a response
        var result = {
          "deviceIdentifier": config.deviceIdentifier,
          "clientId": config.clientId,
          "clientSecret": config.clientSecret,
          "securityService": config.securityServiceUri,
          "estateManagement": config.estateManagementUri,
          "transactionProcessorACL": config.transactionProcessorACLUri,
          "logLevel": config.logLevel
        }

        res.send(JSON.stringify(result));
    });

app.post('/logging/:id', async (req,res) =>
    {      
        // create the new log message
        var logEntry = {
            deviceIdentifier: req.params.id,
            id: req.body.id,
            message: req.body.message,
            dateTime: req.body.entryDateTime,
            logLevel: req.body.logLevel
        };
        // Write to the database
        const insertedLogEntry = await mobileLogsDatabase.put(logEntry); // put() will autogenerate a key for us
        
        // return a success
        res.status(201).json(insertedLogEntry);    
    });    

module.exports = app;    

// Note: these 2 lines needed for local debugging only
//const port = 1337;
//app.listen(port, () => console.log(`Hello world app listening on port ${port}!`));