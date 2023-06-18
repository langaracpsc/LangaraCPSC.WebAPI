using System.Collections;
using System.Collections.Generic;
using KeyMan;
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
        public async Task<string> GetProfile([FromRoute] string id, [FromHeader] string apiKey)
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
        public async Task<string> CreateExec([FromHeader] string request, [FromHeader] string apiKey)
        {
            return await Task.Run(() =>
            {
                Exec exec;

                Hashtable requestMap = JsonConvert.DeserializeObject<Hashtable>(Tools.DecodeFromBase64(request));

                APIKey key = Services.APIKeyManagerInstance.GetAPIKey(apiKey);

                if (key == null)
                    return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();

                Console.WriteLine(JsonConvert.SerializeObject(key));  
                
                if (key.HasPermission("ExecCreate"))
                {
                    if ((exec = Services.ExecManagerInstance.CreateExec((long)requestMap["studentid"],
                            new ExecName(requestMap["firstname"].ToString(), requestMap["lastname"].ToString()),
                            (ExecPosition)(long)requestMap["position"], new ExecTenure(DateTime.Now))) == null)
                        return new HttpError(HttpErrorType.InvalidParamatersError, "Failed to create exec.").ToJson();
                }
                else
                    return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();
                
                return new HttpObject(HttpReturnType.Success, exec.ToJson()).ToJson();
            });
        }

        [HttpPost("End")]
        public async Task<string> EndTenure([FromHeader] string request)
        {
            Hashtable requestMap = JsonConvert.DeserializeObject<Hashtable>(Tools.DecodeFromBase64(request));

            long id;
            
            return await Task.Run(() =>
            {
                if (!requestMap.ContainsKey("apikey"))
                    return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();
                
                if (!requestMap.ContainsKey("studentid"))                               
                    return new HttpError(HttpErrorType.InvalidParamatersError, "Invalid parameters.").ToJson();

                APIKey key;
                
                try
                {
                    if ((key = Services.APIKeyManagerInstance.GetAPIKey(requestMap["apikey"].ToString())) == null)
                        return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();
                    
                    if (key.HasPermission("ExecEnd"))
                        Services.ExecManagerInstance.EndTenure(id = (long)requestMap["studentid"]);
                    else
                        return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();
                }
                catch (Exception e)
                {
                    return new HttpError(HttpErrorType.Unknown, e.Message).ToJson();
                }
                
                return new HttpObject(HttpReturnType.Success, id).ToJson();
            });
        }
        
        public ExecController()
        {
        }
    }
}  
