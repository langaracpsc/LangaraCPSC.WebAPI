using LangaraCPSC.WebAPI.DbModels;
using Newtonsoft.Json;

namespace LangaraCPSC.WebAPI.Tests;

public class ExecManagerTest
{
    private readonly LCSDBContext DBContext;
    
    private readonly ExecManager Manager;
    
    [Fact]
    public void GetExecs()
    {
        Assert.True(this.Manager.GetExecs().Count >= 0);
    }
    
    [Fact]
    public void CreateAndEndExec()
    {
        Assert.NotNull(this.Manager.CreateExec(100000000, new ExecName("John", "Doe"), "john@doe.com", ExecPosition.President, new ExecTenure(new DateTime())));
        Assert.NotNull(this.Manager.GetExec(100000000));
         
        this.Manager.EndTenure(100000000);

        this.Manager.UpdateExec(new DbModels.Exec { Id = 100000000, Firstname = "Alice"});

        Assert.Equal("Alice", this.Manager.GetExec(100000000)?.Name.FirstName);

        Assert.NotNull(this.Manager.GetExec(100000000)?.Tenure.End);
        Assert.True(this.Manager.DeleteExec(100000000));
    }
    
    
    public ExecManagerTest(LCSDBContext dbContext)
    {
        this.DBContext = dbContext;
        this.Manager = new ExecManager(dbContext);
    }
} 

