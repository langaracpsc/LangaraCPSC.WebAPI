using LangaraCPSC.WebAPI.DbModels;

namespace LangaraCPSC.WebAPI.Tests;

public class ExecImageManagerTest
{
    private readonly LCSDBContext DBContext;
    
    private readonly ExecImageManager Manager;
    
    [InlineData("100000000.png")] 
    [Theory]
    public void CRUDImage(string path)
    {
        ExecImageBase64? image = null;
        
        Assert.True(this.Manager.AddExecImage(ExecImageBase64.LoadFromFile(path))); // Create
        Assert.True(this.Manager.ExecImageExists(100000000)); // Read 
        
        Assert.True(this.Manager.UpdateImage(100000000, "test.png")); // Update
        Assert.Throws<FileNotFoundException>(() => this.Manager.GetImageByID(100000000)); // throws because updated path doesn't exist
        
        Assert.True(this.Manager.DeleteImage(100000000)); // Delete
        Assert.Throws<Exception>(() => this.Manager.GetImageByID(100000000)); // Throws because image isn't supposed to be found after deletion
    }
    
    public ExecImageManagerTest(LCSDBContext dbContext)
    {
        this.DBContext = dbContext;
        this.Manager = new ExecImageManager(dbContext, "LangaraCPSC.WebAPI.Tests/bin/Debug/net8.0/");
    }
} 


