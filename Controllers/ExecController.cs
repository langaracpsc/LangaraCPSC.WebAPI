using System.Collections;
using System.Collections.Generic;
using KeyMan;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Newtonsoft.Json;
using Npgsql;

namespace LangaraCPSC.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExecController : ControllerBase
    {
        [HttpGet("ListAll")]
        public async Task<string> Get([FromHeader] string apikey)
        {
            APIKey key;

            if ((key = Services.APIKeyManagerInstance.GetAPIKey(apikey)) == null)
                return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();

            if (!key.HasPermission("ExecRead"))
                return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();

            return await Task.Run(() =>
            {
                return new HttpObject(HttpReturnType.Success,
                    JsonConvert.SerializeObject(Services.ExecManagerInstance.GetExecs())).ToJson();
            });
        }

        [HttpGet("Profile/{id}")]
        public async Task<string> GetProfile([FromRoute] long id, [FromHeader] string apikey)
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
        public async Task<string> CreateExec([FromHeader] string request, [FromHeader] string apikey)
        {
            return await Task.Run(() =>
            {
                Exec exec;

                Hashtable requestMap = JsonConvert.DeserializeObject<Hashtable>(Tools.DecodeFromBase64(request));

                APIKey key = Services.APIKeyManagerInstance.GetAPIKey(apikey);

                if (key == null)
                    return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();

                if (!key.HasPermission("ExecCreate"))
                    return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();

                try
                {
                    if ((exec = Services.ExecManagerInstance.CreateExec((long)requestMap["studentid"],
                            new ExecName(requestMap["firstname"].ToString(), requestMap["lastname"].ToString()),
                            requestMap["email"].ToString(),
                            (ExecPosition)(long)requestMap["position"], new ExecTenure(DateTime.Now))) == null)
                        return new HttpError(HttpErrorType.InvalidParamatersError, "Failed to create exec.").ToJson();
                }
                catch (Exception e)
                {
                    return new HttpObject(HttpReturnType.Error, e.Message).ToJson();
                }

                return new HttpObject(HttpReturnType.Success, exec.ToJson()).ToJson();
            });
        }

        [HttpPost("Profile/Create")]
        public async Task<string> CreateExecProfile([FromHeader] string request, [FromHeader] string apikey)
        {
            return await Task.Run(() =>
            {
                ExecProfile execProfile = null;

                Hashtable requestMap = JsonConvert.DeserializeObject<Hashtable>(Tools.DecodeFromBase64(request));

                APIKey key = Services.APIKeyManagerInstance.GetAPIKey(apikey);

                if (key == null)
                    return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();
                try
                {
                    if (key.HasPermission("ExecCreate"))
                    {
                        if ((execProfile = Services.ExecProfileManagerInstance.CreateProfile((long)requestMap["id"],requestMap["imageid"].ToString(), requestMap["description"].ToString())) == null)
                            return new HttpError(HttpErrorType.InvalidParamatersError, "Failed to create exec.").ToJson();
                    }
                    else
                        return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();

                }
                catch (Exception e)
                {
                    return new HttpError(HttpErrorType.Unknown, e.Message).ToJson();
                }

                return new HttpObject(HttpReturnType.Success, execProfile.ToJson()).ToJson();
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

        [HttpPost("Update")]
        public async Task<string> UpdateExec([FromHeader] string request, [FromHeader] string apikey)
        {
            APIKey key;

            Hashtable requestMap = JsonConvert.DeserializeObject<Hashtable>(Tools.DecodeFromBase64(request));

            return await Task.Run(() =>
            {
                Exec updatedExec = null;

                try
                {
                    if ((key = Services.APIKeyManagerInstance.GetAPIKey(apikey)) == null)
                        return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();

                    if (key.HasPermission("ExecUpdate"))
                        updatedExec = Services.ExecManagerInstance.UpdateExec(requestMap);
                }
                catch (Exception e)
                {
                    return new HttpError(HttpErrorType.Unknown, e.Message).ToJson();
                }

                return new HttpObject(HttpReturnType.Success, updatedExec).ToJson();
            });
        }

        [HttpPost("Profile/Update")]
        public async Task<string> UpdateExecProfile([FromHeader] string request, [FromHeader] string apikey)
        {
            APIKey key;

            Hashtable requestMap = JsonConvert.DeserializeObject<Hashtable>(Tools.DecodeFromBase64(request));

            return await Task.Run(() =>
            {
                ExecProfile updatedExecProfile = null;

                try
                {
                    if ((key = Services.APIKeyManagerInstance.GetAPIKey(apikey)) == null)
                        return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();

                    if (key.HasPermission("ExecUpdate"))
                        updatedExecProfile = Services.ExecProfileManagerInstance.UpdateExecProfile(requestMap);
                }
                catch (Exception e)
                
                {
                    return new HttpError(HttpErrorType.Unknown, e.Message).ToJson();
                }

                return new HttpObject(HttpReturnType.Success, updatedExecProfile).ToJson();
            });
        }

        [HttpGet("Profile/GetActive")]
        public async Task<string> GetActiveProfiles([FromHeader]string apikey)
        {
            APIKey key = null;

            return await Task.Run(() =>
            {
                if ((key = Services.APIKeyManagerInstance.GetAPIKey(apikey)) == null) 
                    return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();
                
                if (!key.HasPermission("ExecRead"))
                    return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();

                return new HttpObject(HttpReturnType.Success, Services.ExecProfileManagerInstance.GetActiveProfiles()).ToJson();
            });
        }

        [HttpPut("Image/Create")]
        public async Task<string> CreateExecImage([FromBody] RequestModel request, [FromHeader] string apikey)
        {
            Hashtable requestMap = JsonConvert.DeserializeObject<Hashtable>(Tools.DecodeFromBase64(request.Request));

            APIKey key;
               
            if ((key = Services.APIKeyManagerInstance.GetAPIKey(apikey)) == null)
                return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();
            if (!key.HasPermission("ExecCreate"))
                return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();
           
            return await Task.Run(() =>
            {
                ExecImageBase64 image = null;
            
                try
                {
                    Services.ExecImageManagerInstance.AddExecImage(
                        (image = new ExecImageBase64((long)requestMap["id"], requestMap["buffer"].ToString()))); 

                    image.SaveToFile();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return new HttpError(HttpErrorType.Unknown, e.Message).ToJson();
                }
                
                return new HttpObject(HttpReturnType.Success, image).ToJson();
            });
        }

        [HttpGet("Image/{id}")]
        public async Task<string> GetExecImage(int id, [FromHeader] string apikey)
        {
            APIKey key;
               
            if ((key = Services.APIKeyManagerInstance.GetAPIKey(apikey)) == null)
                return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();
            
            if (!key.HasPermission("ExecRead"))
                return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson(); 
            
            return await Task.Run(() => {
                ExecImageBase64 image = null;
                
                if ((image = Services.ExecImageManagerInstance.GetImageByID(id)) == null)
                    return new HttpError(HttpErrorType.FileNotFoundError, $"Image with ID \"{id}\" was not found.").ToJson();
                    
                return new HttpObject(HttpReturnType.Success, image.ToJson()).ToJson();
            });
        }

        public ExecController()
        {
        }
    }
}  


