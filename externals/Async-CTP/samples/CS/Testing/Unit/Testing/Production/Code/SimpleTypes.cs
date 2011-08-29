using System.Threading.Tasks;

public struct SnapshotExtrema
{
    public StockSnapshot MinDelta;
    public StockSnapshot MaxDelta;
}

public struct StockSnapshot
{
    public string Symbol;
    public double Percent;
}

public interface IStockSnapshotProvider
{
    Task<StockSnapshot> GetLatestSnapshotAsync();
}