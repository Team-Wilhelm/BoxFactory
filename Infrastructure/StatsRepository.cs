using System.Data;
using Dapper;
using Models.DTOs;

namespace Infrastructure;

public class StatsRepository
{
    private readonly IDbConnection _dbConnection;
    private readonly string _databaseSchema;

    public StatsRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
        _databaseSchema = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
            ? "testing"
            : "Production";
    }

    public async Task<Dictionary<int,int>> GetStats()
    {
        var statsSql = @$"
    SELECT
        EXTRACT(MONTH FROM o.created_at)::integer AS Month,
        SUM(bol.quantity)::integer AS Boxes
    FROM {_databaseSchema}.orders o 
    INNER JOIN {_databaseSchema}.box_order_link bol ON o.order_id = bol.order_id 
    WHERE o.created_at > NOW() - INTERVAL '12 months'
    GROUP BY EXTRACT(MONTH FROM o.created_at)
    ORDER BY EXTRACT(MONTH FROM o.created_at);
";
        var statsList = (await _dbConnection.QueryAsync(statsSql)).ToList();
        
        // The frontend expects the month to be 0-indexed, so we subtract 1 from the month
        // Also, we need to convert the IEnumerable<dynamic> to a Dictionary<int, int>
        // and put 0 for months that have no orders
        var statsDictionary = new Dictionary<int, int>();
        for (int i = 0; i < 12; i++)
        {
            statsDictionary.Add(i, 0);
        }
        
        for (int i = 0; i < statsList.Count; i++)
        {
            var statsListMonth = statsList[i].month - 1;
            statsDictionary[statsListMonth] = statsList[i].boxes;
        }

        return statsDictionary;
    }
}