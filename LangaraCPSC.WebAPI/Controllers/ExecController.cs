using System.Collections;
using System.Text.Json;
using KeyMan;
using LangaraCPSC.WebAPI.DbModels;
using Microsoft.AspNetCore.Mvc;

namespace LangaraCPSC.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExecController : ControllerBase
    {
        private readonly ExecManager _ExecManager;

        private readonly ExecImageManager _ExecImageManager;
        
        private readonly ExecProfileManager _ExecProfileManager;

        private readonly APIKeyManager _ApiKeyManager;
        
        [HttpGet("ListAll")]
        public async Task<string> Get([FromHeader] string apikey)
        {
            if (!this._ApiKeyManager.IsValid(apikey, new string[]{ "ExecRead" }))
                return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();  

            return await Task.Run(() =>
            {
                return new HttpObject(HttpReturnType.Success,  this._ExecManager.GetExecs()).ToJson();
            });
        }

        [HttpGet("Profile/{id}")]
        public async Task<string> GetProfile([FromRoute] long id, [FromHeader] string apikey)
        {
            return await Task.Run(() =>
            {
                if (!this._ApiKeyManager.IsValid(apikey, new string[]{ "ExecRead" }))
                    return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();  
                
                
                ExecProfile profile;

                try
                {
                    profile = this._ExecProfileManager.GetProfileById(id);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    
                    return new HttpObject(HttpReturnType.Error, $"Exec profile with {id} not found.").ToJson();
                }

                return new HttpObject(HttpReturnType.Success, profile.ToJson()).ToJson();
            });
        }

        [HttpPost("Create")]
        public async Task<string> CreateExec([FromBody] Hashtable request, [FromHeader] string apikey)
        {
            return await Task.Run(() =>
            {
                if (!this._ApiKeyManager.IsValid(apikey, new string[]{ "ExecCreate" }))
                    return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();  
                
                Exec exec;

                try
                {
                    if ((exec = this._ExecManager.CreateExec(((JsonElement)request["studentid"]).GetInt64(),
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
                if (!this._ApiKeyManager.IsValid(apikey, new string[]{ "ExecCreate" }))
                    return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();  

                ExecProfile execProfile = null;
                
                try
                {
                    if ((execProfile = this._ExecProfileManager.CreateProfile(((JsonElement)request["studentid"]).GetInt64(),request["imageid"].ToString(), request["description"].ToString())) == null) 
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
                
                if (!this._ApiKeyManager.IsValid(apikey, new string[]{ "ExecEnd" }))
                    return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();
                
                if (!request.ContainsKey("studentid"))
                    return new HttpError(HttpErrorType.InvalidParamatersError, "Invalid parameters.").ToJson();

                APIKey key;

                try
                {
                    this._ExecManager.EndTenure(id = ((JsonElement)request["studentid"]).GetInt64());
                }
                catch (Exception e)
                {
                    return new HttpError(HttpErrorType.Unknown, e.Message).ToJson();
                }

                return new HttpObject(HttpReturnType.Success, id).ToJson();
            });
        }

        [HttpPost("Update")]
        public async Task<string> UpdateExec([FromHeader] DbModels.Exec request, [FromHeader] string apikey)
        {
            APIKey key;

            return await Task.Run(() =>
            {
                if (!this._ApiKeyManager.IsValid(apikey, new string[]{ "ExecUpdate" }))
                    return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();

                Exec updatedExec = null;
                
                try
                {
                    updatedExec = this._ExecManager.UpdateExec(request);
                }
                catch (Exception e)
                {
                    return new HttpError(HttpErrorType.Unknown, e.Message).ToJson();
                }

                return new HttpObject(HttpReturnType.Success, updatedExec).ToJson();
            });
        }

        [HttpPost("Profile/Update")]
        public async Task<string> UpdateExecProfile([FromBody] Execprofile request, [FromHeader] string apikey)
        {
            return await Task.Run(() =>
            {
                if (!this._ApiKeyManager.IsValid(apikey, new string[]{ "ExecUpdate" }))
                        return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();
                
                ExecProfile updatedExecProfile = null;

                try
                {
                    this._ExecProfileManager.UpdateProfile(request.Id, request.Imageid,request.Description);

                    // return new HttpObject(HttpReturnType.Success, this._ExecProfileManager.GetProfileById(request.Id)).ToJson();
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
                if (!this._ApiKeyManager.IsValid(apikey, new string[]{ "ExecRead" }))
                    return new HttpError(HttpErrorType.Forbidden, "500: Forbidden").ToJson();

                List<Hashtable> completeProfileMaps = new List<Hashtable>();
                
                if (complete && image)
                {
                    List<ExecImageProfile> execProfiles = this._ExecProfileManager.GetActiveImageProfiles();
                    
                    foreach (ExecImageProfile profile in execProfiles)
                        completeProfileMaps.Add(profile.GetComplete(this._ExecManager));
                    
                    return new HttpObject(HttpReturnType.Success, completeProfileMaps).ToJson();
                }
                
                if (complete)
                { 

                    List<ExecProfile> execProfiles = this._ExecProfileManager.GetActiveProfiles();

                    foreach (ExecProfile profile in execProfiles)
                        completeProfileMaps.Add(profile.GetComplete(this._ExecManager));
                    
                    return new HttpObject(HttpReturnType.Success, completeProfileMaps).ToJson();
                }
                
                
                return new HttpObject(HttpReturnType.Success, 
                            (image) ? this._ExecProfileManager.GetActiveImageProfiles() 
                                : this._ExecProfileManager.GetActiveProfiles()).ToJson();
            });
        }
 
        [HttpPut("Image/Create")]
        public async Task<string> CreateExecImage([FromBody] Hashtable request, [FromHeader] string apikey)
        {
            return await Task.Run(() =>
            {
                if (!this._ApiKeyManager.IsValid(apikey, new string[]{ "ExecCreate" }))
                    return new HttpError(HttpErrorType.Forbidden, "403: Forbidden").ToJson();
           
                ExecImageBase64 image = null;

                try
                {
                    long id = ((JsonElement)request["id"]).GetInt64();
                   
                    image = new ExecImageBase64(id, request["buffer"].ToString(), request["path"].ToString());

                    if (!this._ExecImageManager.ExecImageExists(id))
                        this._ExecImageManager.AddExecImage(image);
                    else
                        this._ExecImageManager.UpdateImage(id, image.Path);
                } 
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return new HttpError(HttpErrorType.
                        Unknown, e.Message).ToJson();
                }

                if (!System.IO.File.Exists(image.Path))
                    image.SaveToFile($"Images/{image.Path}");
               
                return new HttpObject(HttpReturnType.Success, image).ToJson();
            });
        }

        [HttpGet("Image/{id}")]
        public async Task<string> GetExecImage([FromRoute] int id, [FromHeader] string apikey)
        {
            Console.WriteLine($"Tried fetching image {id}");

            if (!this._ApiKeyManager.IsValid(apikey, new string[] { "ExecRead" }))
            {
                Console.WriteLine($"Invalid key provided");
                
                return new HttpError(HttpErrorType.Forbidden, "403: Forbidden").ToJson();
            }
           
            return await Task.Run(() => {
                ExecImageBase64 image = null;

                try
                {
                    image = this._ExecImageManager.GetImageByID(id);
                }
                catch (Exception)
                {
                    return new HttpError(HttpErrorType.FileNotFoundError, $"Image with ID \"{id}\" was not found.")
                        .ToJson();
                }

                return new HttpObject(HttpReturnType.Success, image).ToJson();
            });
        } 

        public ExecController(IExecManager execManager, IExecProfileManager profileManager, IExecImageManager imageManager, APIKeyManager apiKeyManager)
        {
            this._ExecManager = execManager as ExecManager;
            this._ExecImageManager = imageManager as ExecImageManager;
            this._ExecProfileManager = profileManager as ExecProfileManager;
            this._ApiKeyManager = apiKeyManager as APIKeyManager;
        }
    }
}  


