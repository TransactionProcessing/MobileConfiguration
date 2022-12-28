﻿namespace MobileConfiguration.Database;

using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.SqlClient;
using MySqlConnector;
using Shared.General;
using Shared.Repositories;

[ExcludeFromCodeCoverage]
public class ConfigurationReaderConnectionStringRepository : IConnectionStringConfigurationRepository
{
    #region Methods

    public async Task CreateConnectionString(String externalIdentifier,
                                             String connectionStringIdentifier,
                                             String connectionString,
                                             CancellationToken cancellationToken)
    {
        throw new NotImplementedException("This is only required to complete the interface");
    }

    public async Task DeleteConnectionStringConfiguration(String externalIdentifier,
                                                          String connectionStringIdentifier,
                                                          CancellationToken cancellationToken)
    {
        throw new NotImplementedException("This is only required to complete the interface");
    }

    public async Task<String> GetConnectionString(String externalIdentifier,
                                                  String connectionStringIdentifier,
                                                  CancellationToken cancellationToken)
    {
        String connectionString = string.Empty;
        String databaseName = string.Empty;

        String databaseEngine = ConfigurationReader.GetValue("AppSettings", "DatabaseEngine");

        databaseName = $"{connectionStringIdentifier}{externalIdentifier}";
        connectionString = ConfigurationReader.GetConnectionString(connectionStringIdentifier);

        DbConnectionStringBuilder builder = null;

        if (databaseEngine == "MySql")
        {
            builder = new MySqlConnectionStringBuilder(connectionString)
                      {
                          Database = databaseName
                      };
        }
        else
        {
            // Default to SQL Server
            builder = new SqlConnectionStringBuilder(connectionString)
                      {
                          InitialCatalog = databaseName
                      };
        }

        return builder.ToString();
    }

    #endregion
}