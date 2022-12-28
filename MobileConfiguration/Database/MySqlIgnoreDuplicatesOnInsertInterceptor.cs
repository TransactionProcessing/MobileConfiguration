namespace MobileConfiguration.Database;

using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

public class MySqlIgnoreDuplicatesOnInsertInterceptor : DbCommandInterceptor
{
    public override ValueTask<InterceptionResult<Int32>> NonQueryExecutingAsync(DbCommand command,
                                                                                CommandEventData eventData,
                                                                                InterceptionResult<Int32> result,
                                                                                CancellationToken cancellationToken = new CancellationToken()) {
        command.CommandText = this.SetIgnoreDuplicates(command.CommandText);
        return new ValueTask<InterceptionResult<Int32>>(result);
    }

    public override ValueTask<InterceptionResult<Object>> ScalarExecutingAsync(DbCommand command,
                                                                               CommandEventData eventData,
                                                                               InterceptionResult<Object> result,
                                                                               CancellationToken cancellationToken = new CancellationToken()) {
        command.CommandText = this.SetIgnoreDuplicates(command.CommandText);
        return new ValueTask<InterceptionResult<Object>>(result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,
                                                                                     CommandEventData eventData,
                                                                                     InterceptionResult<DbDataReader> result,
                                                                                     CancellationToken cancellationToken = new CancellationToken()) {
        command.CommandText = this.SetIgnoreDuplicates(command.CommandText);

        return new ValueTask<InterceptionResult<DbDataReader>>(result);
    }

    public override InterceptionResult<Object> ScalarExecuting(DbCommand command,
                                                               CommandEventData eventData,
                                                               InterceptionResult<Object> result) {
        command.CommandText = this.SetIgnoreDuplicates(command.CommandText);
        return result;
    }

    public override InterceptionResult<Int32> NonQueryExecuting(DbCommand command,
                                                                CommandEventData eventData,
                                                                InterceptionResult<Int32> result) {
        command.CommandText = this.SetIgnoreDuplicates(command.CommandText);
        return result;
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command,
                                                                     CommandEventData eventData,
                                                                     InterceptionResult<DbDataReader> result) {
        command.CommandText = this.SetIgnoreDuplicates(command.CommandText);
        return result;
    }

    private String SetIgnoreDuplicates(String commandText) {
        if (this.IsCommandInsert(commandText)) {
            if (ConfigurationGenericContext.IsDuplicateInsertsIgnored(this.GetTableName(commandText))) {
                // Swap the insert to ignore duplicates
                return commandText.Replace("INSERT INTO", "INSERT IGNORE INTO");
            }
        }

        return commandText;
    }

    private Boolean IsCommandInsert(String commandText) => commandText.Contains("INSERT INTO", StringComparison.InvariantCultureIgnoreCase);

    private String GetTableName(String commandText) {
        // Extract table and check if we are ignoring duplicates
        Int32 tablenameEnd = commandText.IndexOf("(");
        Int32 tablenameStart = 11;
        String tableName = commandText.Substring(tablenameStart, tablenameEnd - tablenameStart);
        tableName = tableName.Replace("`", "");
        return tableName;
    }
}