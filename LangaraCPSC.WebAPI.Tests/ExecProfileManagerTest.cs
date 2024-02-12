using LangaraCPSC.WebAPI.DbModels;
using Newtonsoft.Json;
namespace LangaraCPSC.WebAPI.Tests;

public class ExecProfileManagerTest
{
    private readonly LCSDBContext DBContext;
    
    private readonly ExecProfileManager Manager;

    [InlineData(100000000)]
    [Theory]
    public void CRUDProfile(long id)
    {
        ExecProfile? profile;

        Assert.NotNull(this.Manager.CreateProfile(id, id.ToString(), "FooBar"));
        Assert.NotNull(this.Manager.GetProfileById(id));
       
        Assert.True(this.Manager.UpdateProfile(id, "110110110", "BarFoo"));
        Assert.True(this.Manager.DeleteProfileWithID(id));
    }
    
    public ExecProfileManagerTest(LCSDBContext dbContext)
    { 
        this.DBContext = dbContext;
        this.Manager = new ExecProfileManager(new ExecImageManager(dbContext), dbContext);
    }
} 


