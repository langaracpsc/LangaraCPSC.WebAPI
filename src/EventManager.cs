using System.Reflection;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;

namespace LangaraCPSC.WebAPI
{
    public struct LinkPair
    {
        public string Google;

        public string Apple;

        public LinkPair(string google, string apple)
        {
            this.Google = google;
            this.Apple = apple;
        }
    }
    
    public class Event
    {
        public string Title { get; set; }

        public string Start { get; set; }

        public string End { get; set; }

        public string Description { get; set; }

        public string Location { get; set; }

        public string? Image { get; set; }

        public LinkPair Link { get; set; }
    }

    public class EventManager
    {
        private readonly ServiceAccountCredential Credential;

        private readonly CalendarService _CalendarService;

        private readonly DriveService _DriveService;

        private readonly string CalendarID;

        private readonly string CachePath;
        
        private string GetFileBase64(string? fileId)
        {
            if (fileId == null)
                return null;
            
            MemoryStream stream = new MemoryStream();

            this._DriveService.Files.Get(fileId).Download(stream); 
            
            return Convert.ToBase64String(stream.ToArray());
        }

        private string GenerateICalFilename(Google.Apis.Calendar.v3.Data.Event e)
        {
            CalendarEvent cEvent = new CalendarEvent
            {
                Summary = e.Summary,
                Description = e.Description,  
                Start = new CalDateTime(DateTime.Parse(e.Start.DateTimeRaw)), 
                End = new CalDateTime(DateTime.Parse(e.End.DateTimeRaw)),
                Location = e.Location
            };
            
            
            string path = $"{this.CachePath}/{e.Summary.Replace(' ', '_').Replace('/', '-')}.ics";
            
            if (!File.Exists(path))
                using (StreamWriter writer = new StreamWriter(path))
                {
                    Calendar calendar = new Calendar();

                    calendar.Version = "2.0";
                    
                        
                    calendar.Events.Add(cEvent);
                    
                    writer.Write(new CalendarSerializer().SerializeToString(cEvent));
                    writer.Flush();
                }
            
            return path;
        }

        public FileStream GetIcalFileStream(string fileName)
        {
            return new FileStream($"{this.CachePath}/{fileName}", FileMode.Open);
        }

        public List<Event> GetEvents()
        {
            return this._CalendarService.Events.List(this.CalendarID).Execute().Items.Select(item =>
                new Event 
                {
                    Title = item.Summary,
                    Start = item.Start.DateTimeRaw,
                    End = item.End.DateTimeRaw,
                    Description = item.Description,
                    Location = item.Location,
                    Image = ((item.Attachments == null) ? null : this.GetFileBase64(item.Attachments.FirstOrDefault()?.FileId)),
                    Link = new LinkPair(item.HtmlLink, this.GenerateICalFilename(item))
                }).ToList();
        }
        
        public EventManager(string calendarID, string keyFile, string cachePath = "CalendarEvents")
        {
            this.CalendarID = calendarID;
            this.CachePath = cachePath;

            if (!Directory.Exists(cachePath))
                Directory.CreateDirectory(cachePath);P:
            
            using (FileStream stream = new FileStream(keyFile, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                this.Credential = GoogleCredential.FromStream(stream).CreateScoped(CalendarService.Scope.Calendar, DriveService.Scope.Drive, DriveService.Scope.DriveFile, DriveService.Scope.DriveReadonly).UnderlyingCredential as ServiceAccountCredential;
            }

            this._CalendarService = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = this.Credential,
                ApplicationName = "LangaraCPSC.WebAPI"
            });

            this._DriveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = this.Credential,
                ApplicationName = "LangaraCPSC.WebAPI"
            });
        }
    }
}