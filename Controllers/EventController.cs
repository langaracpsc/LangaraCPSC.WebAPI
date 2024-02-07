using Microsoft.AspNetCore.Mvc;

namespace LangaraCPSC.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventController
    {
        [HttpGet("ListAll")]
        public async Task<string> Get([FromHeader] string apikey)
        {
            if (!Services.APIKeyManagerInstance.IsValid(apikey, new string[]{ "ExecRead" }))
                return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();  
         
            return await Task.Run(() => new HttpObject(HttpReturnType.Success, Services.EventManagerInstance.GetEvents()).ToJson());
        }

        [HttpGet("ICal/{fileName}")]
        public async Task<IActionResult> GetIcal([FromRoute] string fileName)
        {
            return await Task.Run(() => new FileStreamResult(ICalUtils.GetIcalFileStream(fileName, Services.EventManagerInstance.CachePath),
                    "application/octet-stream") { FileDownloadName = fileName } 
            );
        }
        
         public EventController()
         {
         }
     }
 }
 