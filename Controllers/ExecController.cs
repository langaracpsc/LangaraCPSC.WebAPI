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
        public async Task<string> Get([FromHeader] string apikey)
        {
            if (!Services.APIKeyManagerInstance.IsValid(apikey, new string[]{ "ExecRead" }))
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
                if (!Services.APIKeyManagerInstance.IsValid(apikey, new string[]{ "ExecRead" }))
                    return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();  
                
                ExecProfile profile = Services.ExecProfileManagerInstance.GetProfileById(id);

                if ((profile = Services.ExecProfileManagerInstance.GetProfileById(id)) == null)
                    return new HttpObject(HttpReturnType.Error, $"Exec profile with {id} not found.").ToJson();

                return new HttpObject(HttpReturnType.Success, profile.ToJson()).ToJson();
            });
        }

        [HttpPost("Create")]
        public async Task<string> CreateExec([FromBody] string request, [FromHeader] string apikey)
        {
            return await Task.Run(() =>
            {
                if (!Services.APIKeyManagerInstance.IsValid(apikey, new string[]{ "ExecCreate" }))
                    return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();  
                
                Exec exec;
                
                Dictionary<string, object> requestMap = JsonConvert.DeserializeObject<Dictionary<string, object>>(request);

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
        public async Task<string> CreateExecProfile([FromBody] string request, [FromHeader] string apikey)
        {
            return await Task.Run(() =>
            {
                if (!Services.APIKeyManagerInstance.IsValid(apikey, new string[]{ "ExecCreate" }))
                    return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();  

                ExecProfile execProfile = null;

                
                Hashtable requestMap = JsonConvert.DeserializeObject<Hashtable>(request);
                
                try
                {
                    Console.WriteLine(JsonConvert.SerializeObject(requestMap));
                    
                    if (Services.APIKeyManagerInstance.IsValid(apikey, new string[] { "ExecCreate" }))
                    {
                        if ((execProfile = Services.ExecProfileManagerInstance.CreateProfile((long)requestMap["studentid"],requestMap["imageid"].ToString(), requestMap["description"].ToString())) == null)
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
        public async Task<string> EndTenure([FromBody] string request, [FromHeader] string apikey)
        {
            return await Task.Run(() =>
            { 
                long id;
                
                if (!Services.APIKeyManagerInstance.IsValid(apikey, new string[]{ "ExecEnd" }))
                    return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();

                Hashtable requestMap = JsonConvert.DeserializeObject<Hashtable>(request);
                
                if (!requestMap.ContainsKey("studentid"))
                    return new HttpError(HttpErrorType.InvalidParamatersError, "Invalid parameters.").ToJson();

                APIKey key;

                try
                {
                    Services.ExecManagerInstance.EndTenure(id = (long)requestMap["studentid"]);
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

            return await Task.Run(() =>
            {
                if (!Services.APIKeyManagerInstance.IsValid(apikey, new string[]{ "ExecUpdate" }))
                    return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();

                Exec updatedExec = null;

                Hashtable requestMap = JsonConvert.DeserializeObject<Hashtable>(request);
                
                try
                {
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
            return await Task.Run(() =>
            {
                if (!Services.APIKeyManagerInstance.IsValid(apikey, new string[]{ "ExecUpdate" }))
                    return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();

                Hashtable requestMap = JsonConvert.DeserializeObject<Hashtable>(request);
                
                ExecProfile updatedExecProfile = null;

                try
                {
                    updatedExecProfile = Services.ExecProfileManagerInstance.UpdateExecProfile(requestMap);
                }
                catch (Exception e)
                
                {
                    return new HttpError(HttpErrorType.Unknown, e.Message).ToJson();
                }

                return new HttpObject(HttpReturnType.Success, updatedExecProfile).ToJson();
            });
        }

        [HttpGet("Profile/Active")]
        public async Task<string> GetActiveProfiles([FromHeader]string apikey)
        {
            APIKey key = null;

            return await Task.Run(() =>
            {
                if (!Services.APIKeyManagerInstance.IsValid(apikey, new string[]{ "ExecRead" }))
                    return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();

                return new HttpObject(HttpReturnType.Success, Services.ExecProfileManagerInstance.GetActiveProfiles()).ToJson();
            });
        }
 
        [HttpPut("Image/Create")]
        public async Task<string> CreateExecImage([FromBody] RequestModel request, [FromHeader] string apikey)
        {
            return await Task.Run(() =>
            {
                if (!Services.APIKeyManagerInstance.IsValid(apikey, new string[]{ "ExecCreate" }))
                    return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();
                
                Hashtable requestMap = JsonConvert.DeserializeObject<Hashtable>(request.Request);

                APIKey key;
           
                ExecImageBase64 image = null;
            
                try
                {
                    Services.ExecImageManagerInstance.AddExecImage(
                        (image = new ExecImageBase64((long)requestMap["id"], requestMap["buffer"].ToString()))); 

                    image.SaveToFile($"{Services.ExecImageManagerInstance.ImageDir}/{image.Path}");
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
            if (!Services.APIKeyManagerInstance.IsValid(apikey, new string[]{ "ExecRead" }))
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


