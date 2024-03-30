using System.ComponentModel;
using Google.Apis.Calendar.v3.Data;
using LangaraCPSC.WebAPI.DbModels;

namespace LangaraCPSC.WebAPI.Tests;

public class EventManagerTest
{
    private readonly EventManager Manager;
    
    [Fact]
    public void GetAllEvents()
    {
        Assert.True(this.Manager.GetEvents().Count >= 0);     
    }
    
    [InlineData(2024, 5)]
    [Theory]
    public void GetMaxEvents(int year, int max)
    {
        List<Event> events = this.Manager.GetMaxEvents(year, max);

        Assert.True(events.Count >= 0 && events.Count <= max);     

        events = this.Manager.GetMaxEvents(year + 10, max);

        Assert.True(events.Count < 1);
    }

    public EventManagerTest()
    {
        this.Manager = new EventManager();
    }
} 
