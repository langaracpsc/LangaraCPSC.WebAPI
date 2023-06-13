using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LangaraCPSC.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ExecController : ControllerBase
{
    [HttpGet("ListAll")]
    public async Task<string> Get()
    {
        return await Task.Run(() => {
            return JsonConvert.SerializeObject(Services.ExecManagerInstance.GetExecs());
        });        
    }

    [HttpGet("Profile/{id}")]
    public async Task<string> GetProfile([FromRoute]string id)
    {
        return await Task.Run(() => {
            return id;
        });
    }

    public ExecController()
    {
    }
}