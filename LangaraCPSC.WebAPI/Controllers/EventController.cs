using KeyMan;
using Microsoft.AspNetCore.Mvc;

namespace LangaraCPSC.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventController
    {
        private readonly APIKeyManager _ApiKeyManager;

        private readonly EventManager _EventManager;
        
        [HttpGet("ListAll")]
        public async Task<string> Get([FromHeader] string apikey)
        {
            if (!this._ApiKeyManager.IsValid(apikey, new string[]{ "ExecRead" }))
                return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();  
         
            return await Task.Run(() => new HttpObject(HttpReturnType.Success, this._EventManager.GetEvents()).ToJson());
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
 