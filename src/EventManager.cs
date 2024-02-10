using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.VisualBasic.CompilerServices;
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

    public class ICalUtils
    {
        public static string GenerateICalFilename(Event e, string outputDir, EventRRule? rrule = null)
        {
            CalendarEvent cEvent = new CalendarEvent
            {
                Summary = e.Title ?? "Unknown",
                Description = e.Description ?? "No description",  
                Start = new CalDateTime(e.Start), 
                End = new CalDateTime(e.End),
                Location = e.Location,
                RecurrenceRules = (rrule != null) ? new [] { rrule.ToIcalRecurrencePattern() } : null
            };
            
            string path = $"{outputDir}/{cEvent.Summary.Replace(' ', '_').Replace('/', '-')}.ics";
            
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
        
        public static string GenerateICalFilename(Google.Apis.Calendar.v3.Data.Event e, string outputDir, EventRRule? rrule = null)
        {
            CalendarEvent cEvent = new CalendarEvent
            {
                Summary = e.Summary ?? "Unknown",
                Description = e.Description ?? "No description",  
                Start = new CalDateTime(DateTime.Parse((e.Start != null && e.Start?.DateTimeRaw != null) ? e.Start.DateTimeRaw : new DateTime().ToLongDateString())), 
                End = (e.End != null && e.End.DateTimeRaw != null) ? new CalDateTime(e.End.DateTimeRaw) : null,
                Location = e.Location,
                RecurrenceRules = (rrule != null) ? new [] { rrule.ToIcalRecurrencePattern() } : null
            };
            
            string path = $"{outputDir}/{cEvent.Summary.Replace(' ', '_').Replace('/', '-')}.ics";
            
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


        public static FileStream GetIcalFileStream(string fileName, string outputDir)
        {
            return new FileStream($"{outputDir}/{fileName}", FileMode.Open);
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

    public class EventRRule
    {
        public string Frequency { get; set; }

        public string WeekStart { get; set; }

        public DateTime Until { get; set; }

        public string[] ByDay { get; set; }

        private static Dictionary<string, DayOfWeek> ByDayMap = new Dictionary<string, DayOfWeek>()
        {
            { "SU", DayOfWeek.Friday },
            { "MO", DayOfWeek.Monday }, 
            { "TU", DayOfWeek.Tuesday },
            { "WE", DayOfWeek.Wednesday },
            { "TH", DayOfWeek.Thursday },
            { "FR", DayOfWeek.Friday },
            { "SA", DayOfWeek.Saturday }
        };

        private static Dictionary<string, int> FrequencyMap = new Dictionary<string, int>()
        {
            { "DAILY", 1 },
            { "WEEKLY", 7 },
            { "MONTHLY", 30 },
            { "YEARLY", 365 }
        };

        private static int GetDaysByFrequency(string frequency, int year, int month = 1)
        {
            if (frequency != "MONTHLY")
                return FrequencyMap[frequency];
            
            if (month == 2)
                return ((year % 4) == 0) ? 29 : 28;

            return (month % 2 != 0) ? 30 : 31;
        }
        
        public static EventRRule FromRRuleString(string str)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            int index = str.IndexOf(':') + 1;
                
            foreach (string pair in str.Substring(index, str.Length - index).Split(';'))
            {
                string[] temp = pair.Split('=');
                dict.Add(temp[0], temp[1]);
            }
    
            return new EventRRule {
                Frequency = dict["FREQ"],
                WeekStart = dict.ContainsKey("WKST") ? dict["WKST"] : null,
                Until = dict.ContainsKey("UNTIL") ? DateTime.ParseExact(dict["UNTIL"], "yyyyMMddTHHmmssZ", null, System.Globalization.DateTimeStyles.AssumeUniversal) : DateTime.Now.AddMonths(1),
                ByDay = dict["BYDAY"].Split(',')
            };
        }

        public RecurrencePattern ToIcalRecurrencePattern()
        {
            return new RecurrencePattern()
            {
                Frequency  = (FrequencyType)FrequencyMap[this.Frequency], 
                ByDay = this.ByDay.Select(day  => new WeekDay(ByDayMap[day])).ToList(),
                FirstDayOfWeek = ByDayMap[this.WeekStart],
                Until = this.Until
            };
        }

        public List<Event> ToEvents(Google.Apis.Calendar.v3.Data.Event startEvent)
        {
            List<Event> events = new List<Event>();

            DateTime start = DateTime.Parse(startEvent.Start.DateTimeRaw);

            int days = 0;
            
            for (; start.CompareTo(this.Until) < 0; start = start.AddDays(days = GetDaysByFrequency(this.Frequency, start.Year, start.Month)))
                for (int x = 0;  x < this.ByDay.Length; x++)
                {
                    DateTime end = DateTime.Parse(startEvent.End.DateTimeRaw).AddDays(days), startWeekDay = start;

                    int weekDays = (x > 0) ? (int)EventRRule.ByDayMap[this.ByDay[x]] + 1 : 0;

                    startWeekDay = startWeekDay.AddDays(weekDays);
                    end = end.AddDays(days + weekDays);
                        
                    events.Add(new Event
                    {
                        Title = startEvent.Summary ?? "Unknown",
                        Start = new DateTimeOffset(startWeekDay, TimeZoneInfo.Local.GetUtcOffset(startWeekDay)).ToString(),
                        End = new DateTimeOffset(end, TimeZoneInfo.Local.GetUtcOffset(end)).ToString(),
                        Description = startEvent.Description ?? "No description.",
                        Location = startEvent.Location ?? "TBD",
                        Link = new LinkPair(startEvent.HtmlLink, null)
                    });
                }
 
            return events;
        }
    }

    public class EventManager
    {
        private readonly ServiceAccountCredential Credential;

        private readonly CalendarService _CalendarService;

        private readonly DriveService _DriveService;

        private readonly string CalendarID;

        public readonly string CachePath;

        private readonly string ImagePath;
        
        private Dictionary<string, string> FileCache;

        public string InviteLink
        {
            get => _InviteLink;
            private set { } 
        }

        private string _InviteLink;

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

            Console.WriteLine($"Fetching path {path}");

            if (!File.Exists(path))
            {
                MemoryStream stream = new MemoryStream();
            
                this._DriveService.Files.Get(fileId).Download(stream); 

                this.FileCache.Add(fileId, path);
                
                File.WriteAllBytes(path, stream.GetBuffer());
            }
            
            return path; 
        }

        public string GenerateCalendarInvite()
        {
            return this._InviteLink = $"https://calendar.google.com/calendar?cid={this.CalendarID}";
        }

        public List<Event> GetEvents()
        {
            EventAttachment? attachment;

            var items = this._CalendarService.Events.List(this.CalendarID).Execute().Items ??
                        new List<Google.Apis.Calendar.v3.Data.Event>();
            
            List<Event> events = new List<Event>();
            
           return items.Where(item => item != null && item.Summary != null).Select(item =>
           {
               if (item.Recurrence != null && item.Recurrence.Count > 0)
               {
                    EventRRule rrule;

                    Event e = (rrule = EventRRule.FromRRuleString(item.Recurrence[0]))
                        .ToEvents(item)
                        .Where(e => (DateTime.Parse(e.Start).CompareTo(DateTime.Now) >= 0))
                        .FirstOrDefault();

                    e.Link = new LinkPair($"{item.HtmlLink}&recur={item.Recurrence[0]}", ICalUtils.GenerateICalFilename(e, this.CachePath, rrule));
                    
                    return e;
               }

               return new Event
               {
                   Title = item.Summary ?? "Unknown",
                   Start = (item.Start != null) ? item.Start.DateTimeRaw : item.OriginalStartTime.DateTimeRaw,
                   End = (item.End != null) ? item.End.DateTimeRaw : null,
                   Description = item.Description ?? "No description.",
                   Location = item.Location ?? "TBD",
                   Image = ((item.Attachments == null || item.Attachments.Count < 1)
                       ? null
                       : this.FetchFile((item.Attachments.Count > 1) ? item.Attachments[0].FileId : null, "png")),
                   Link = new LinkPair(item.HtmlLink, ICalUtils.GenerateICalFilename(item, this.CachePath))
               };
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
            
            this.GenerateCalendarInvite();
        }
    }
}
 