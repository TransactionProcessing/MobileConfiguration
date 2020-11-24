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
        var messageArray = req.body.messages;
        var logEntries = [];
        
        messageArray.forEach(message => {
          var logEntry = {
            deviceIdentifier: req.params.id,
            id: message.Id,
            message: message.Message,
            dateTime: message.EntryDateTime,
            logLevel: message.LogLevel
          };  
          logEntries.push(logEntry)
        });
        
        // Write to the database
        await mobileLogsDatabase.putMany(logEntries);
        
        // return a success
        res.status(201).send();    
    });
    
app.delete('/logging', async (req,res)=>
{
  var logs = await mobileLogsDatabase.fetch();
  
  for await (const subArray of logs) // each subArray is up to the buffer length, 10
    subArray.forEach(l => {
       mobileLogsDatabase.delete(l.key);
    });

  res.status(200).send();
});    

module.exports = app;    

// Note: these 2 lines needed for local debugging only
//const port = 1337;
//app.listen(port, () => console.log(`Hello world app listening on port ${port}!`));