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
        public async Task<ActionResult<string>> GetMaxEvents([FromHeader] string apikey, [FromRoute] int year, [FromRoute] int max)
        {
            if (!this._ApiKeyManager.IsValid(apikey, new string[]{ "ExecRead" }))
                return Forbid(new HttpError(HttpErrorType.Forbidden, "Forbidden").ToJson());  

            return await Task.Run(() => Ok(new HttpObject(HttpReturnType.Success, this._EventManager.GetMaxEvents(year, max)).ToJson()));
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
 