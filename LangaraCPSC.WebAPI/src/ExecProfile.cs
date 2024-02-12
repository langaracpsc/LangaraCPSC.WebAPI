using System.Collections;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text.Json;
using LangaraCPSC.WebAPI.DbModels;
using Newtonsoft.Json;

namespace LangaraCPSC.WebAPI
{
    public class ExecProfile : IPayload
    {
        public long ID;

        public string ImageID;

        public string Description;

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public Hashtable ToMap()
        {
            Hashtable map = new Hashtable(); 

            map.Add("ID", this.ID);
            map.Add("ImageID", this.ImageID);
            map.Add("Description", this.Description);

            return map;
        }
        
        public virtual Hashtable GetComplete(ExecManager manager)
        {
            Hashtable completeMap = this.ToMap(); 
            
            Exec exec = manager.GetExec(this.ID);
    
            completeMap.Add("Name", exec.Name);
            completeMap.Add("Email", exec.Email);
            completeMap.Add("Position", exec.Position);
                
            return completeMap;
        }

        public static ExecProfile FromModel(Execprofile model)
        {
            return new ExecProfile((long)model.Id, model.Imageid, model.Description);
        }

        public Execprofile ToModel()
        {
            return new Execprofile
            {   
                Id = (int)this.ID,
                Imageid = this.ImageID,
                Description =  this.Description
            }; 
        }

        public ExecProfile(long id, string imageID, string description)
        {
            this.ID = id;
            this.ImageID = imageID;
            this.Description = description;
        }
    }

    public class ExecImageProfile : ExecProfile, IPayload
    {
        public string Image { get; private set; }
       
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public override Hashtable GetComplete(ExecManager manager)
        {
            Hashtable completeMap = base.GetComplete(manager);
 
            completeMap.Add("Image", this.Image);

            return completeMap;
        }

        public ExecImageProfile(ExecProfile profile, string image) : base(profile.ID, profile.ImageID, profile.Description)
        {
            this.Image = image;
        }

        public ExecImageProfile(long id, string imageID, string description, string image) : base(id, imageID, description)
        {
            this.Image = image;
        }
    }

    public interface IExecProfileManager
    {
        bool ProfileExists(long id);
       
        ExecProfile CreateProfile(long id, string imageID, string description);

        bool UpdateProfile(long id, string imageID, string description);

        ExecProfile? GetProfileById(long id);

        List<Exec> GetActiveExecs();

        ExecImageProfile GetExecImageProfile(long id);

        List<ExecImageProfile> GetActiveImageProfiles();

        List<ExecProfile> GetActiveProfiles();
        
        bool DeleteProfileWithID(long id);
    }

    public class ExecProfileManager : IExecProfileManager
    {
        private readonly Dictionary<long, ExecProfile> ProfileMap;

        private readonly ExecImageManager ImageManager;

        private readonly LCSDBContext DBContext; 
        
        public bool ProfileExists(long id)
        {
            return this.DBContext.Execprofiles.FirstOrDefault(e => e.Id ==  id) != null;
        }
        
        public ExecProfile CreateProfile(long id, string imageID, string description)
        {
            ExecProfile profile = new ExecProfile(id, imageID, description);

            try
            {
                this.DBContext.Execprofiles.Add(profile.ToModel());
                this.DBContext.SaveChanges();

                this.ProfileMap.Add(profile.ID, profile); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return profile;
        }

        public bool UpdateProfile(long id, string imageID, string description)
        {
            Execprofile? profile = this.DBContext.Execprofiles.FirstOrDefault(e => e.Id == id);
            
            if (profile == null)
                throw new Exception($"Profile with id {id} not found.");

            profile.Imageid = imageID;
            profile.Description = description;

            try
            {
                this.DBContext.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
            return true;
        }

        public ExecProfile? GetProfileById(long id)
        {
            if (this.ProfileMap.ContainsKey(id))
                return this.ProfileMap[id];

            Execprofile? profile = this.DBContext.Execprofiles.FirstOrDefault(e => e.Id == id);
            
            if (profile == null)
                throw new Exception($"Profile with id {id} not found.");
            
            return ExecProfile.FromModel(profile);
        }

        public List<Exec> GetActiveExecs()
        {
            return this.DBContext.Execs.Where(e => e.Tenureend == null)
                    .Select(e => Exec.FromModel(e))
                    .ToList(); 
        }

        public ExecImageProfile GetExecImageProfile(long id)
        {
            ExecProfile? profile = null;

            try
            {
                profile = this.GetProfileById(id);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return new ExecImageProfile(profile, this.ImageManager.GetImageByID(id).Path);
        }
 
        public List<ExecImageProfile> GetActiveImageProfiles()
        {
            List<ExecImageProfile> execImageProfiles = new List<ExecImageProfile>();

            List<ExecProfile> activeProfiles = this.GetActiveProfiles();
            
            foreach (ExecProfile profile in activeProfiles)
                execImageProfiles.Add(new ExecImageProfile(profile, this.ImageManager.GetImageByID(profile.ID).Path));

            return execImageProfiles;
        }

        public List<ExecProfile> GetActiveProfiles()
        {
            List<ExecProfile> profiles = new List<ExecProfile>();

            List<Exec> execs = new List<Exec>();
                
            if ((execs = this.GetActiveExecs()) == null)
                return null;

            return this.DBContext.Execprofiles.Where(e => execs.FirstOrDefault(exec => exec.ID == e.Id) != null)
                                        .Select(e => ExecProfile.FromModel(e))
                                        .ToList();
        }
        
        public bool DeleteProfileWithID(long id)
        {
            Execprofile? profile = this.DBContext.Execprofiles.FirstOrDefault(e => e.Id == id);
            
            if (profile == null)
                throw new Exception($"Profile with id {id} not found.");

            try
            {
                this.DBContext.Execprofiles.Remove(profile);
                this.DBContext.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e); 
                throw;
            }

            return true;
        }
        
        public ExecProfileManager(IExecImageManager imageManager, LCSDBContext dbContext, string tableName = "ExecProfiles", string execTableName = "Execs")
        {
            this.ProfileMap = new Dictionary<long, ExecProfile>();
            this.ImageManager = imageManager as ExecImageManager;
            this.DBContext = dbContext;
        }
    }
} 
