var express = require('express');
const { Deta } = require('deta');

const deta = Deta();// No need to provide a key if running in a micro
const appCenterConfigDatabase = deta.Base('appCenterconfig');  // access your DB
const mobileConfigDatabase = deta.Base('mobileconfig');  // access your DB
const voucherConfigDatabase = deta.Base('voucherconfig');  // access your DB
const mobileLogsDatabase = deta.Base('mobilelogs');  // access your DB

var app = express();

app.use(express.json());

app.post('/configuration', async function(req,res)
{  
  // create the new config entry message
  const configEntry = req.body;
  configEntry.key = req.body.deviceIdentifier;

  // Write to the database
  const insertedConfigEntry = await mobileConfigDatabase.put(configEntry);
  
  // return a success (created)
  res.status(201).json(insertedConfigEntry); 
  
});

app.post('/voucherconfiguration', async function(req,res)
{  
  // create the new config entry message
  const configEntry = req.body;
  configEntry.key = req.body.deviceIdentifier;

  // Write to the database
  const insertedConfigEntry = await voucherConfigDatabase.put(configEntry);
  
  // return a success (created)
  res.status(201).json(insertedConfigEntry); 
});

app.post('/appcenterconfiguration', async function(req,res)
{  
  // create the new config entry message
  const configEntry = req.body;
  configEntry.key = req.body.applicationId;

  // Write to the database
  const insertedConfigEntry = await appCenterConfigDatabase.put(configEntry);
  
  // return a success (created)
  res.status(201).json(insertedConfigEntry);   
});

app.put('/configuration/:id', async function(req,res)
{  
  var config = await mobileConfigDatabase.get(req.params.id);

  const configEntry = req.body;

  var mergedConfig = {...config, ...configEntry};
  mergedConfig.key = req.params.id;

  // Write to the database
  await mobileConfigDatabase.put(mergedConfig);
  
  // return a success (created)
  res.status(200).send();     
});

app.put('/voucherconfiguration/:id', async function(req,res)
{  
  var config = await voucherConfigDatabase.get(req.params.id);

  const configEntry = req.body;

  var mergedConfig = {...config, ...configEntry};
  mergedConfig.key = req.params.id;

  // Write to the database
  await voucherConfigDatabase.put(mergedConfig);
  
  // return a success (created)
  res.status(200).send(); 
});

app.put('/appcenterconfiguration/:id', async function(req,res)
{  
  var config = await appCenterConfigDatabase.get(req.params.id);

  const configEntry = req.body;

  var mergedConfig = {...config, ...configEntry};
  mergedConfig.key = req.params.id;

  // Write to the database
  await appCenterConfigDatabase.put(mergedConfig);
  
  // return a success (created)
  res.status(200).send();     
});

app.get('/configuration/:id',
    async function(req, res)
    {               
        var config = await mobileConfigDatabase.get(req.params.id);
        var appCenterconfig = await appCenterConfigDatabase.get("transactionMobilePOS");

        config.appCenterconfig = appCenterconfig;

        // send records as a response        
        res.send(JSON.stringify(config));
    });

app.get('/voucherconfiguration/:id',
    async function(req, res)
    {               
        var config = await voucherConfigDatabase.get(req.params.id);
      
        // send records as a response        
        res.send(JSON.stringify(config));
    });    

app.get('/appcenterconfiguration/:id',
    async function(req, res)
    {               
        var config = await appCenterConfigDatabase.get(req.params.id);
      
        // send records as a response        
        res.send(JSON.stringify(config));
    });

function getLogLevel(logLevel)
{
  switch(logLevel)
  {
    case "Trace":
      return 5;
    case "Debug":
      return 4;
    case "Info":
      return 3;
    case "Warn":
      return 2;
    case "Error":
      return 1;
    case "Fatal":
      return 0;
  }
}

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
const port = 1337;
app.listen(port, () => console.log(`Hello world app listening on port ${port}!`));