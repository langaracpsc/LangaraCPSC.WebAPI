using System.Runtime.CompilerServices;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;

namespace LangaraCPSC.WebAPI
{
    public class Event
    {
        public string Title { get; set; }

        public string Start { get; set; }

        public string End { get; set; }

        public string Description { get; set; }

        public string Location;
    }

    public class EventManager
    {
        private readonly ServiceAccountCredential Credential;

        private readonly CalendarService Service;

        private readonly string CalendarID;
        
        public List<Event> GetEvents()
        {
            return this.Service.Events.List(this.CalendarID).Execute().Items.Select(item => new Event
            {
                Title = item.Summary,
                Start = item.Start.DateTimeRaw,
                End  = item.End.DateTimeRaw, 
                Description = item.Description,
                Location = "T001" // ToDo: Change this to the actual extended property
            }).ToList();
        }

        public EventManager(string calendarID, string keyFile)
        {
            this.CalendarID = calendarID;
            
            using (FileStream stream = new FileStream(keyFile, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                this.Credential = GoogleCredential.FromStream(stream).CreateScoped(CalendarService.Scope.Calendar).UnderlyingCredential as ServiceAccountCredential;
            }

            this.Service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = this.Credential,
                ApplicationName = "LangaraCPSC.WebAPI"
            });
        }
    }
}