using System.Collections;
using System.Collections.Generic;
using KeyMan;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OpenDatabase.Json;

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
        public async Task<string> CreateExec([FromBody] Hashtable request, [FromHeader] string apikey)
        {
            return await Task.Run(() =>
            {
                if (!Services.APIKeyManagerInstance.IsValid(apikey, new string[]{ "ExecCreate" }))
                    return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();  
                
                Exec exec;

                try
                {
                    if ((exec = Services.ExecManagerInstance.CreateExec((long)request["studentid"],
                            new ExecName(request["firstname"].ToString(), request["lastname"].ToString()),
                            request["email"].ToString(),
                            (ExecPosition)(long)request["position"], new ExecTenure(DateTime.Now))) == null)
                        return new HttpError(HttpErrorType.InvalidParamatersError, "Failed to create exec.").ToJson();
                }
                catch (Exception e)
                {
                    return new HttpError(HttpErrorType.Unknown, e.Message).ToJson();
                }

                return new HttpObject(HttpReturnType.Success, exec.ToJson()).ToJson();
            });
        }

        [HttpPost("Profile/Create")]
        public async Task<string> CreateExecProfile([FromBody] Hashtable request, [FromHeader] string apikey)
        {
            return await Task.Run(() =>
            {
                if (!Services.APIKeyManagerInstance.IsValid(apikey, new string[]{ "ExecCreate" }))
                    return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();  

                ExecProfile execProfile = null;
                
                try
                {
                    Console.WriteLine(JsonConvert.SerializeObject(request));
                    
                    if (Services.APIKeyManagerInstance.IsValid(apikey, new string[] { "ExecCreate" }))
                    {
                        if ((execProfile = Services.ExecProfileManagerInstance.CreateProfile((long)request["studentid"],request["imageid"].ToString(), request["description"].ToString())) == null)
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
        public async Task<string> EndTenure([FromBody] Hashtable request, [FromHeader] string apikey)
        {
            return await Task.Run(() =>
            { 
                long id;
                
                if (!Services.APIKeyManagerInstance.IsValid(apikey, new string[]{ "ExecEnd" }))
                    return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();
                
                if (!request.ContainsKey("studentid"))
                    return new HttpError(HttpErrorType.InvalidParamatersError, "Invalid parameters.").ToJson();

                APIKey key;

                try
                {
                    Services.ExecManagerInstance.EndTenure(id = (long)request["studentid"]);
                }
                catch (Exception e)
                {
                    return new HttpError(HttpErrorType.Unknown, e.Message).ToJson();
                }

                return new HttpObject(HttpReturnType.Success, id).ToJson();
            });
        }

        [HttpPost("Update")]
        public async Task<string> UpdateExec([FromHeader] Hashtable request, [FromHeader] string apikey)
        {
            APIKey key;

            return await Task.Run(() =>
            {
                if (!Services.APIKeyManagerInstance.IsValid(apikey, new string[]{ "ExecUpdate" }))
                    return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();

                Exec updatedExec = null;
                
                try
                {
                    updatedExec = Services.ExecManagerInstance.UpdateExec(request);
                }
                catch (Exception e)
                {
                    return new HttpError(HttpErrorType.Unknown, e.Message).ToJson();
                }

                return new HttpObject(HttpReturnType.Success, updatedExec).ToJson();
            });
        }

        [HttpPost("Profile/Update")]
        public async Task<string> UpdateExecProfile([FromHeader] Hashtable request, [FromHeader] string apikey)
        {
            return await Task.Run(() =>
            {
                if (!Services.APIKeyManagerInstance.IsValid(apikey, new string[]{ "ExecUpdate" }))
                        return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();
                
                ExecProfile updatedExecProfile = null;

                try
                {
                    updatedExecProfile = Services.ExecProfileManagerInstance.UpdateExecProfile(request);
                }
                catch (Exception e)
                
                {
                    return new HttpError(HttpErrorType.Unknown, e.Message).ToJson();
                }

                return new HttpObject(HttpReturnType.Success, updatedExecProfile).ToJson();
            });
        }

        [HttpGet("Profile/Active")]
        public async Task<string> GetActiveProfiles([FromHeader]string apikey, [FromQuery] bool image)
        {
            APIKey key = null;

            return await Task.Run(() =>
            {
                if (!Services.APIKeyManagerInstance.IsValid(apikey, new string[]{ "ExecRead" }))
                    return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();

                return new HttpObject(HttpReturnType.Success, 
                            (image) ? Services.ExecProfileManagerInstance.GetActiveImageProfiles() 
                                : Services.ExecProfileManagerInstance.GetActiveProfiles()).ToJson();
            });
        }
 
        [HttpPut("Image/Create")]
        public async Task<string> CreateExecImage([FromBody] Hashtable request, [FromHeader] string apikey)
        {
            return await Task.Run(() =>
            {
                if (!Services.APIKeyManagerInstance.IsValid(apikey, new string[]{ "ExecCreate" }))
                    return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();

                APIKey key;
           
                ExecImageBase64 image = null;
            
                try
                {
                    Services.ExecImageManagerInstance.AddExecImage(
                        (image = new ExecImageBase64((long)request["id"], request["buffer"].ToString()))); 

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
        public async Task<string> GetExecImage([FromRoute] int id, [FromHeader] string apikey)
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


