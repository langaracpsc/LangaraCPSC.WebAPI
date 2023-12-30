using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
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

        public string? Image { get; set; }
    }

    public class EventManager
    {
        private readonly ServiceAccountCredential Credential;

        private readonly CalendarService _CalendarService;

        private readonly DriveService _DriveService;

        private readonly string CalendarID;

        private string GetFileBase64(string? fileId)
        {
            string base64 = null;
    
            if (fileId == null)
                return null;
            
            MemoryStream stream = new MemoryStream();

            // this._DriveService.Files.Export(fileId, "application/vnd.google-apps.file").;
            Console.WriteLine($"Stream size: {stream.Length}");
            
            this._DriveService.Files.Get(fileId).Download(stream); 
            return Convert.ToBase64String(stream.ToArray());
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
                    Image = ((item.Attachments == null) ? null : this.GetFileBase64(item.Attachments.FirstOrDefault()?.FileId))
                }).ToList();
        }

        public EventManager(string calendarID, string keyFile)
        {
            this.CalendarID = calendarID;
            
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