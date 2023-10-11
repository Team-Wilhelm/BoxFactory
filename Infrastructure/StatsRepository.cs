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
            : "production";
    }

    public async Task<StatsDto> GetStats()
    {
        var statsSql = @$"
    SELECT
        EXTRACT(MONTH FROM o.created_at) AS Month,
        SUM(bol.quantity) AS Boxes
    FROM {_databaseSchema}.orders o 
    INNER JOIN {_databaseSchema}.box_order_link bol ON o.order_id = bol.order_id 
    WHERE o.created_at > NOW() - INTERVAL '12 months'
    GROUP BY EXTRACT(MONTH FROM o.created_at)
    ORDER BY EXTRACT(MONTH FROM o.created_at);
";

        var statsList = await _dbConnection.QueryAsync(statsSql);

        return new StatsDto { OrdersPerMonth = statsList.ToDictionary(x => (int)x.Month, x => (int)x.Boxes) };

    }
}