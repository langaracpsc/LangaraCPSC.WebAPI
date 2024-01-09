using System.Reflection;
using System.Text.Json.Nodes;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Newtonsoft.Json;
using Calendar = Ical.Net.Calendar;

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

        private readonly string ImagePath;
        
        private Dictionary<string, string> FileCache;

        private string GetFileBase64(string? fileId)
        {
            if (fileId == null)
                return null;

            string? value = null;

            if (this.FileCache.TryGetValue(fileId, out value))
                return value;
            
            MemoryStream stream = new MemoryStream();
            
            this._DriveService.Files.Get(fileId).Download(stream); 
            
            string base64String = Convert.ToBase64String(stream.ToArray());

            this.FileCache.Add(fileId, base64String);

            return base64String; 
        }

        public string FetchFile(string? fileId, string extension)
        {
            if (fileId == null)
                return null;

            string? value = null;

            if (this.FileCache.TryGetValue(fileId, out value))
                return value;

            string path = $"{this.ImagePath}/{fileId}.{extension}";

            if (!File.Exists(path))
            {
                MemoryStream stream = new MemoryStream();
            
                this._DriveService.Files.Get(fileId).Download(stream); 

                this.FileCache.Add(fileId, path);
                
                File.WriteAllBytes(path, stream.GetBuffer());
            }
            
            return path; 
        }
        
        private string GenerateICalFilename(Google.Apis.Calendar.v3.Data.Event e)
        {
            CalendarEvent cEvent = new CalendarEvent
            {
                Summary = e.Summary ?? "Unknown",
                Description = e.Description ?? "No description",  
                Start = new CalDateTime(DateTime.Parse((e.Start != null) ? e.Start.DateTimeRaw : new DateTime().ToLongDateString())), 
                End = (e.End != null) ? new CalDateTime(e.End.DateTimeRaw) : null,
                Location = e.Location 
            };
            
            string path = $"{this.CachePath}/{cEvent.Summary.Replace(' ', '_').Replace('/', '-')}.ics";
            
            if (!File.Exists(path))
                using (StreamWriter writer = new StreamWriter(path))
                {
                    Calendar calendar = new Calendar();
                    
                    calendar.Version = "2.0";
                    calendar.Events.Add(cEvent);
                    
                    writer.Write(new CalendarSerializer().SerializeToString(calendar));
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
            EventAttachment? attachment;

            var items = this._CalendarService.Events.List(this.CalendarID).Execute().Items ??
                        new List<Google.Apis.Calendar.v3.Data.Event>();

            List<Event> events = new List<Event>();
            
           return items.Where(item => item != null && item.Summary != null).Select(item =>
               new Event
                {
                    Title = item.Summary ?? "Unknown",
                    Start = (item.Start != null) ? item.Start.DateTimeRaw : item.OriginalStartTime.DateTimeRaw,
                    End = (item.End != null) ? item.End.DateTimeRaw : null,
                    Description = item.Description ?? "No description.",
                    Location = item.Location ?? "TBD",
                    Image = ((item.Attachments == null || item.Attachments.Count < 1)
                        ? null
                        : this.FetchFile((item.Attachments.Count > 1) ? item.Attachments[0].FileId : null, "png")),
                    Link = new LinkPair(item.HtmlLink ?? null, this.GenerateICalFilename(item) ?? null)
                }).ToList();
        }
            
        public static JsonObject GetCalendarConfig()
        {
            JsonObject config = new JsonObject();
            
            config.Add("type", Environment.GetEnvironmentVariable("CAL_ACCOUNT_TYPE"));
            config.Add("project_id", Environment.GetEnvironmentVariable("CAL_PROJECT_ID"));
            config.Add("private_key_id", Environment.GetEnvironmentVariable("CAL_PRIVATE_KEY_ID"));
            config.Add("private_key", Environment.GetEnvironmentVariable("CAL_PRIVATE_KEY"));
            config.Add("client_email", Environment.GetEnvironmentVariable("CAL_CLIENT_EMAIL"));
            config.Add("client_id", Environment.GetEnvironmentVariable("CAL_CLIENT_ID"));
            config.Add("auth_uri", Environment.GetEnvironmentVariable("CAL_AUTH_URI"));
            config.Add("token_uri", Environment.GetEnvironmentVariable("CAL_TOKEN_URI"));
            config.Add("auth_provider_x509_cert_url", Environment.GetEnvironmentVariable("CAL_PROVIDER_X509_CERT_URL"));
            config.Add("client_x509_cert_url", Environment.GetEnvironmentVariable("CAL_CLIENT_X509_CERT_URL"));
            config.Add("universe_domain", Environment.GetEnvironmentVariable("CAL_UNIVERSE_DOMAIN"));
 
            return config;
        }
        
        public EventManager(string calendarID, string cachePath = "CalendarEvents", string imagePath = "Images")
        {
            this.CalendarID = calendarID;
            this.CachePath = cachePath;
            this.ImagePath = imagePath;
            
            this.FileCache = new Dictionary<string, string>();
            
            if (!Directory.Exists(this.CachePath))
                Directory.CreateDirectory(this.CachePath);
            
            // this.Credential = GoogleCredential.FromFile("keyfile_conf.json").CreateScoped(CalendarService.Scope.Calendar, DriveService.Scope.Drive, DriveService.Scope.DriveFile, DriveService.Scope.DriveReadonly).UnderlyingCredential as ServiceAccountCredential;
            
            this.Credential = GoogleCredential.FromJson(EventManager.GetCalendarConfig().ToJsonString()).CreateScoped(CalendarService.Scope.Calendar, DriveService.Scope.Drive, DriveService.Scope.DriveFile, DriveService.Scope.DriveReadonly).UnderlyingCredential as ServiceAccountCredential;

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
 