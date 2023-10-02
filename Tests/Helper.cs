using Dapper;
using Npgsql;

namespace Tests;

public static class Helper
{
    public static readonly NpgsqlDataSource DataSource;
    
    public static void TriggerRebuild()
    {
        using var conn = DataSource.OpenConnection();
        try
        {
            conn.Execute(RebuildScript);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

     public static string RebuildScript = @"
DROP SCHEMA IF EXISTS /*schema*/ CASCADE;
CREATE SCHEMA /*schema*/;
create table if not exists /*schema.table*/
(
    /**/
);
 ";
    //TODO add script
}