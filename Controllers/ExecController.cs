using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LangaraCPSC.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExecController : ControllerBase
    {
        [HttpGet("ListAll")]
        public async Task<string> Get()
        {
            return await Task.Run(() =>
            {
                return new HttpObject(HttpReturnType.Success,
                    JsonConvert.SerializeObject(Services.ExecManagerInstance.GetExecs())).ToJson();
            });
        }

        [HttpGet("Profile/{id}")]
        public async Task<string> GetProfile([FromRoute] string id)
        {
            return await Task.Run(() =>
            {
                ExecProfile profile = Services.ExecProfileManagerInstance.GetProfileById(id);

                if ((profile = Services.ExecProfileManagerInstance.GetProfileById(id)) == null)
                    return new HttpObject(HttpReturnType.Error, $"Exec profile with {id} not found.").ToJson();

                return new HttpObject(HttpReturnType.Success, profile.ToJson()).ToJson();
            });
        }

        [HttpPost("Create")]
        public async Task<string> CreateExec([FromHeader] string request)
        {
            return await Task.Run(() =>
            {
                Exec exec;

                Hashtable requestMap = JsonConvert.DeserializeObject<Hashtable>(Tools.DecodeFromBase64(request));

                if ((exec = Services.ExecManagerInstance.CreateExec((long)requestMap["StudentID"], new ExecName(requestMap["FirstName"].ToString(), requestMap["FirstName"].ToString()), (ExecPosition)(long)requestMap["Position"], new ExecTenure(DateTime.Now))) == null)
                    return new HttpObject(HttpReturnType.Error, "Failed to create exec.").ToJson();

                return new HttpObject(HttpReturnType.Success, exec.ToJson()).ToJson();
            });
        }

        public ExecController()
        {
        }
    }
} 