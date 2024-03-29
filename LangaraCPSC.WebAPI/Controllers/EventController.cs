using KeyMan;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace LangaraCPSC.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventController : ControllerBase
    {
        private readonly APIKeyManager _ApiKeyManager;

        private readonly EventManager _EventManager;
        
        [HttpGet("ListAll")]
        [OutputCache(Duration = 60)]
        public async Task<ActionResult<string>> Get([FromHeader] string apikey)
        {
            if (!this._ApiKeyManager.IsValid(apikey, new string[]{ "ExecRead" }))
                return Forbid(new HttpError(HttpErrorType.Forbidden, "Forbidden").ToJson());  
         
            return await Task.Run(() => Ok(new HttpObject(HttpReturnType.Success, this._EventManager.GetEvents()).ToJson()));
        }
        
        [HttpGet("{year}/{max}")]
        [OutputCache(Duration = 60)]
        public async Task<ActionResult<string>> GetEventsMax([FromHeader] string apikey, [FromRoute] int year, [FromRoute] int max)
        {
            if (!this._ApiKeyManager.IsValid(apikey, new string[]{ "ExecRead" }))
                return Forbid(new HttpError(HttpErrorType.Forbidden, "Forbidden").ToJson());  
            
            List<Event> events = this._EventManager.GetEvents().Where(e => e.Start != null && DateTime.Parse(e.Start).Year == year).ToList();  

            return await Task.Run(() => Ok(new HttpObject(HttpReturnType.Success, (events.Count > 0) ? events.Slice(0, events.Count >= max ? max : events.Count) : events).ToJson()));
        }
        
        [HttpGet("Calendar")]
        public async Task<ActionResult<string>> GetCalendarInviteLink()
        {
            return await Task.Run( () => this._EventManager.InviteLink); 
        }

        [HttpGet("ICal/{fileName}")]
        public async Task<IActionResult> GetIcal([FromRoute] string fileName)
        {
            return await Task.Run(() => new FileStreamResult(ICalUtils.GetIcalFileStream(fileName, this._EventManager.CachePath),
                    "application/octet-stream") { FileDownloadName = fileName } 
            );
        }
        
         public EventController(APIKeyManager keyManager, IEventManager eventManager)
         {
             this._ApiKeyManager = keyManager;
             this._EventManager = eventManager as EventManager;
         }
     }
 }
 