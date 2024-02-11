using LangaraCPSC.WebAPI.DbModels;

namespace LangaraCPSC.WebAPI.Tests;

public class UnitTest1
{
    private readonly LCSDBContext DBContext;
    
    private readonly ExecManager Manager;
    
    [Fact]
    public void Test1()
    {
    }

    public UnitTest1(LCSDBContext dbContext)
    {
        this.DBContext = dbContext;
        this.Manager = new ExecManager(dbContext);
    }
} 
