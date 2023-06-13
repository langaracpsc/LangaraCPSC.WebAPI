using System.Net;
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
            return new HttpObject(HttpReturnType.Success, JsonConvert.SerializeObject(Services.ExecManagerInstance.GetExecs())).ToJson();
        });        
    }

    [HttpGet("Profile/{id}")]
    public async Task<string> GetProfile([FromRoute]string id)
    {
        return await Task.Run(() =>
        {
            ExecProfile profile = Services.ExecProfileManagerInstance.GetProfileById(id);

            if ((profile = Services.ExecProfileManagerInstance.GetProfileById(id)) == null)
                return new HttpObject(HttpReturnType.Error, $"Exec profile with {id} not found.").ToJson();
            
            return new HttpObject(HttpReturnType.Success, profile.ToJson()).ToJson();
        });
    }

    public ExecController()
    {
    }
}