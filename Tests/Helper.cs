using Dapper;
using Models;
using Npgsql;

namespace Tests;

public static class Helper
{
    public static readonly NpgsqlDataSource DataSource;
    
    public static BoxCreateDto CreateBoxCreateDto(float weight, string colour, string material, float price, float height, float length, float width)
    {
        return new BoxCreateDto()
        {
            Weight = weight,
            Colour = colour,
            Material = material,
            Price = price,
            Dimensions = new Dimensions()
            {
                Height = height,
                Length = length,
                Width = width
            }
        };
    }
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

    //TODO add script
     public static string RebuildScript = @"
DROP SCHEMA IF EXISTS /*schema*/ CASCADE;
CREATE SCHEMA /*schema*/;
create table if not exists /*schema.table*/
(
    /**/
);
 ";
    
     
}