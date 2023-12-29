using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
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
                return new HttpObject(HttpReturnType.Success,  Services.ExecManagerInstance.GetExecs()).ToJson();
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
                    if ((exec = Services.ExecManagerInstance.CreateExec(((JsonElement)request["studentid"]).GetInt64(), 
                            new ExecName(request["firstname"].ToString(), request["lastname"].ToString()),
                            request["email"].ToString(),
                            (ExecPosition)((JsonElement)request["position"]).GetInt64(), new ExecTenure(DateTime.Now))) == null)
                        return new HttpError(HttpErrorType.InvalidParamatersError, "Failed to create exec.").ToJson();
                }
                catch (Exception e)
                {
                    return new HttpError(HttpErrorType.Unknown, e.Message).ToJson();
                }

                return new HttpObject(HttpReturnType.Success, exec).ToJson();
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
                    if ((execProfile = Services.ExecProfileManagerInstance.CreateProfile(((JsonElement)request["studentid"]).GetInt64(),request["imageid"].ToString(), request["description"].ToString())) == null) 
                        return new HttpError(HttpErrorType.InvalidParamatersError, "Failed to create exec profile.").ToJson();
        
                }
                catch (Exception e)
                {
                    return new HttpError(HttpErrorType.Unknown, e.Message).ToJson();
                }

                return new HttpObject(HttpReturnType.Success, execProfile).ToJson();
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
                    Services.ExecManagerInstance.EndTenure(id = ((JsonElement)request["studentid"]).GetInt64());
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
        public async Task<string> GetActiveProfiles([FromHeader]string apikey, [FromQuery] bool complete = false, [FromQuery] bool image = false)
        {
            APIKey key = null;

            return await Task.Run(() =>
            {
                if (!Services.APIKeyManagerInstance.IsValid(apikey, new string[]{ "ExecRead" }))
                    return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();

                List<Hashtable> completeProfileMaps = new List<Hashtable>();
                
                if (complete && image)
                {
                    List<ExecImageProfile> execProfiles = Services.ExecProfileManagerInstance.GetActiveImageProfiles();
                    
                    foreach (ExecImageProfile profile in execProfiles)
                        completeProfileMaps.Add(profile.GetComplete(Services.ExecManagerInstance));
                    
                    return new HttpObject(HttpReturnType.Success, completeProfileMaps).ToJson();
                }
                
                if (complete)
                { 

                    List<ExecProfile> execProfiles = Services.ExecProfileManagerInstance.GetActiveProfiles();

                    foreach (ExecProfile profile in execProfiles)
                        completeProfileMaps.Add(profile.GetComplete(Services.ExecManagerInstance));
                    
                    return new HttpObject(HttpReturnType.Success, completeProfileMaps).ToJson();
                }
                
                
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
           
                ExecImageBase64 image = null;

                try
                {
                    Services.ExecImageManagerInstance.AddExecImage(
                        (image = new ExecImageBase64(((JsonElement)request["id"]).GetInt64(), request["buffer"].ToString()))
                        ); 

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
            Console.WriteLine($"Tried fetching image {id}");

            if (!Services.APIKeyManagerInstance.IsValid(apikey, new string[] { "ExecRead" }))
            {
                Console.WriteLine($"Invalid key provided");
                
                return new HttpError(HttpErrorType.Forbidden, "403: Forbidden").ToJson();
            }
           
            return await Task.Run(() => {
                ExecImageBase64 image = null;
                
                if ((image = Services.ExecImageManagerInstance.GetImageByID(id)) == null)
                    return new HttpError(HttpErrorType.FileNotFoundError, $"Image with ID \"{id}\" was not found.").ToJson();
                    
                return new HttpObject(HttpReturnType.Success, image).ToJson();
            });
        } 

        public ExecController()
        {
        }
    }
}  


