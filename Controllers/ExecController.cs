using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LangaraCPSC.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ExecController : ControllerBase
{
    [HttpGet("ListAll")]
    public string Get()
    {
        return JsonConvert.SerializeObject(Services.ExecManagerInstance.GetExecs());        
    } 
    

    public ExecController()
    {
    }
}