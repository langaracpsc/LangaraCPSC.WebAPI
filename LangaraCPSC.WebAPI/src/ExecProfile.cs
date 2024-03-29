using System.Collections;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text.Json;
using LangaraCPSC.WebAPI.DbModels;
using Newtonsoft.Json;

namespace LangaraCPSC.WebAPI
{
    /// <summary>
    /// Represents an executive profile.
    /// </summary>
    public class ExecProfile : IPayload
    {
        /// <summary>
        /// The unique identifier of the executive profile.
        /// </summary>
        public long ID;

        /// <summary>
        /// The unique identifier of the image associated with the executive profile.
        /// </summary>
        public string ImageID;

        /// <summary>
        /// The description of the executive profile.
        /// </summary>
        public string Description;

        /// <summary>
        /// Converts the current instance of the <see cref="ExecProfile"/> class to a JSON string.
        /// </summary>
        /// <returns>A JSON string representation of the <see cref="ExecProfile"/> instance.</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Converts the current instance of the <see cref="ExecProfile"/> class to a hash table.
        /// </summary>
        /// <returns>A hash table representation of the <see cref="ExecProfile"/> instance.</returns>
        public Hashtable ToMap()
        {
            Hashtable map = new Hashtable();

            map.Add("ID", this.ID);
            map.Add("ImageID", this.ImageID);
            map.Add("Description", this.Description);

            return map;
        }

        /// <summary>
        /// Gets a complete hash table representation of the executive profile, including the executive's name, email, and position.
        /// </summary>
        /// <param name="manager">The <see cref="ExecManager"/> instance used to retrieve the executive information.</param>
        /// <returns>A hash table containing the complete executive profile information.</returns>
        public virtual Hashtable GetComplete(ExecManager manager)
        {
            Hashtable completeMap = this.ToMap();

            Exec? exec = manager.GetExec(this.ID);

            Hashtable position = new Hashtable();

            position.Add("ID", exec?.Position);
            position.Add("Name", Exec.PositionStrings[(int)exec?.Position]);

            completeMap.Add("Name", exec.Name);
            completeMap.Add("Email", exec.Email);
            completeMap.Add("Position", position);

            return completeMap;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ExecProfile"/> class from a database model.
        /// </summary>
        /// <param name="model">The database model to create the executive profile from.</param>
        /// <returns>A new instance of the <see cref="ExecProfile"/> class.</returns>
        public static ExecProfile FromModel(Execprofile model)
        {
            return new ExecProfile((long)model.Id, model.Imageid, model.Description);
        }

        /// <summary>
        /// Converts the current instance of the <see cref="ExecProfile"/> class to a database model.
        /// </summary>
        /// <returns>A new instance of the <see cref="Execprofile"/> class.</returns>
        public Execprofile ToModel()
        {
            return new Execprofile
            {
                Id = (int)this.ID,
                Imageid = this.ImageID,
                Description = this.Description
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecProfile"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the executive profile.</param>
        /// <param name="imageID">The unique identifier of the image associated with the executive profile.</param>
        /// <param name="description">The description of the executive profile.</param>
        public ExecProfile(long id, string imageID, string description)
        {
            this.ID = id;
            this.ImageID = imageID;
            this.Description = description;
        }
    }

    /// <summary>
    /// Represents an executive profile with an associated image.
    /// </summary>
    public class ExecImageProfile : ExecProfile, IPayload
    {
        /// <summary>
        /// The image associated with the executive profile.
        /// </summary>
        public string Image { get; private set; }

        /// <summary>
        /// Converts the current instance of the <see cref="ExecImageProfile"/> class to a JSON string.
        /// </summary>
        /// <returns>A JSON string representation of the <see cref="ExecImageProfile"/> instance.</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Gets a complete hash table representation of the executive image profile, including the image.
        /// </summary>
        /// <param name="manager">The <see cref="ExecManager"/> instance used to retrieve the executive information.</param>
        /// <returns>A hash table containing the complete executive image profile information.</returns>
        public override Hashtable GetComplete(ExecManager manager)
        {
            Hashtable completeMap = base.GetComplete(manager);

            completeMap.Add("Image", this.Image);

            return completeMap;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecImageProfile"/> class.
        /// </summary>
        /// <param name="profile">The executive profile.</param>
        /// <param name="image">The image associated with the executive profile.</param>
        public ExecImageProfile(ExecProfile profile, string image) : base(profile.ID, profile.ImageID, profile.Description)
        {
            this.Image = image;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecImageProfile"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the executive profile.</param>
        /// <param name="imageID">The unique identifier of the image associated with the executive profile.</param>
        /// <param name="description">The description of the executive profile.</param>
        /// <param name="image">The image associated with the executive profile.</param>
        public ExecImageProfile(long id, string imageID, string description, string image) : base(id, imageID, description)
        {
            this.Image = image;
        }
    }

    /// <summary>
    /// Defines the interface for managing executive profiles.
    /// </summary>
    public interface IExecProfileManager
    {
        /// <summary>
        /// Checks if a profile with the specified ID exists.
        /// </summary>
        /// <param name="id">The unique identifier of the executive profile.</param>
        /// <returns>True if the profile exists, false otherwise.</returns>
        bool ProfileExists(long id);

        /// <summary>
        /// Creates a new executive profile.
        /// </summary>
        /// <param name="id">The unique identifier of the executive profile.</param>
        /// <param name="imageID">The unique identifier of the image associated with the executive profile.</param>
        /// <param name="description">The description of the executive profile.</param>
        /// <returns>The newly created executive profile.</returns>
        ExecProfile CreateProfile(long id, string imageID, string description);

        /// <summary>
        /// Updates an existing executive profile.
        /// </summary>
        /// <param name="id">The unique identifier of the executive profile.</param>
        /// <param name="imageID">The new unique identifier of the image associated with the executive profile.</param>
        /// <param name="description">The new description of the executive profile.</param>
        /// <returns>True if the update was successful, false otherwise.</returns>
        bool UpdateProfile(long id, string? imageID, string? description);

        /// <summary>
        /// Gets an executive profile by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the executive profile.</param>
        /// <returns>The executive profile with the specified identifier, or null if not found.</returns>
        ExecProfile? GetProfileById(long id);

        /// <summary>
        /// Gets a list of all active executives.
        /// </summary>
        /// <returns>A list of all active executives.</returns>
        List<Exec> GetActiveExecs();

        /// <summary>
        /// Gets an executive image profile by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the executive profile.</param>
        /// <returns>The executive image profile with the specified identifier.</returns>
        ExecImageProfile GetExecImageProfile(long id);

        /// <summary>
        /// Gets a list of all active executive image profiles.
        /// </summary>
        /// <returns>A list of all active executive image profiles.</returns>
        List<ExecImageProfile> GetActiveImageProfiles();

        /// <summary>
        /// Gets a list of all active executive profiles.
        /// </summary>
        /// <returns>A list of all active executive profiles.</returns>
        List<ExecProfile> GetActiveProfiles();

        /// <summary>
        /// Deletes an executive profile by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the executive profile.</param>
        /// <returns>True if the profile was successfully deleted, false otherwise.</returns>
        bool DeleteProfileWithID(long id);
    }

    /// <summary>
    /// Implements the <see cref="IExecProfileManager"/> interface.
    /// </summary>
    public class ExecProfileManager : IExecProfileManager
    {
        /// <summary>
        /// A dictionary that maps executive profile IDs to their corresponding <see cref="ExecProfile"/> instances.
        /// </summary>
        private readonly Dictionary<long, ExecProfile> ProfileMap;

        /// <summary>
        /// The <see cref="ExecImageManager"/> instance used to manage executive images.
        /// </summary>
        private readonly ExecImageManager ImageManager;

        /// <summary>
        /// The database context used by the <see cref="ExecProfileManager"/>.
        /// </summary>
        private readonly LCSDBContext DBContext;

        /// <summary>
        /// Checks if a profile with the specified ID exists.
        /// </summary>
        /// <param name="id">The unique identifier of the executive profile.</param>
        /// <returns>True if the profile exists, false otherwise.</returns>
        public bool ProfileExists(long id)
        {
            return this.DBContext.Execprofiles.FirstOrDefault(e => e.Id == id) != null;
        }

        /// <summary>
        /// Creates a new executive profile.
        /// </summary>
        /// <param name="id">The unique identifier of the executive profile.</param>
        /// <param name="imageID">The unique identifier of the image associated with the executive profile.</param>
        /// <param name="description">The description of the executive profile.</param>
        /// <returns>The newly created executive profile.</returns>
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
        
        /// <summary>
        /// Updates an existing executive profile.
        /// </summary>
        /// <param name="id">The unique identifier of the executive profile.</param>
        /// <param name="imageID">The new unique identifier of the image associated with the executive profile.</param>
        /// <param name="description">The new description of the executive profile.</param>
        /// <returns>True if the update was successful, false otherwise.</returns>
        public bool UpdateProfile(long id, string? imageID, string? description)
        {
            Execprofile? profile = this.DBContext.Execprofiles.FirstOrDefault(e => e.Id == id);

            if (profile == null)
                throw new Exception($"Profile with id {id} not found.");

            profile.Imageid = imageID ?? profile.Imageid;
            profile.Description = description ?? profile.Description;

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

        /// <summary>
        /// Gets an executive profile by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the executive profile.</param>
        /// <returns>The executive profile with the specified identifier, or null if not found.</returns>
        public ExecProfile? GetProfileById(long id)
        {
            if (this.ProfileMap.ContainsKey(id))
                return this.ProfileMap[id];

            Execprofile? profile = this.DBContext.Execprofiles.FirstOrDefault(e => e.Id == id);

            if (profile == null)
                throw new Exception($"Profile with id {id} not found.");

            return ExecProfile.FromModel(profile);
        }

        /// <summary>
        /// Gets a list of all active executives.
        /// </summary>
        /// <returns>A list of all active executives.</returns>
        public List<Exec> GetActiveExecs()
        {
            string defaultDate = (new DateTime()).ToString();

            return this.DBContext.Execs.Where(e => e.Tenureend == defaultDate)
                    .Select(e => Exec.FromModel(e))
                    .ToList();
        }

        /// <summary>
        /// Gets an executive image profile by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the executive profile.</param>
        /// <returns>The executive image profile with the specified identifier.</returns>
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

        /// <summary>
        /// Gets a list of all active executive image profiles.
        /// </summary>
        /// <returns>A list of all active executive image profiles.</returns>
        public List<ExecImageProfile> GetActiveImageProfiles()
        {
            List<ExecImageProfile> execImageProfiles = new List<ExecImageProfile>();

            List<ExecProfile> activeProfiles = this.GetActiveProfiles();

            foreach (ExecProfile profile in activeProfiles)
                execImageProfiles.Add(new ExecImageProfile(profile, this.ImageManager.GetImageByID(profile.ID).Path));

            return execImageProfiles;
        }

        /// <summary>
        /// Gets a list of all active executive profiles.
        /// </summary>
        /// <returns>A list of all active executive profiles.</returns>
        public List<ExecProfile> GetActiveProfiles()
        {
            List<ExecProfile> profiles = new List<ExecProfile>();

            List<Exec> execs = new List<Exec>();

            if ((execs = this.GetActiveExecs()) == null)
                return null;

            Dictionary<long, Exec> execMap = new Dictionary<long, Exec>();

            foreach (Exec exec in execs)
                execMap.Add(exec.ID, exec);


            foreach (Execprofile execprofile in this.DBContext.Execprofiles)
                if (execMap.ContainsKey(execprofile.Id))
                    profiles.Add(ExecProfile.FromModel(execprofile));

            return profiles;
        }

        /// <summary>
        /// Deletes an executive profile by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the executive profile.</param>
        /// <returns>True if the profile was successfully deleted, false otherwise.</returns>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecProfileManager"/> class.
        /// </summary>
        /// <param name="imageManager">The <see cref="ExecImageManager"/> instance used to manage executive images.</param>
        /// <param name="dbContext">The database context used by the <see cref="ExecProfileManager"/>.</param>
        /// <param name="tableName">The name of the executive profile table in the database.</param>
        /// <param name="execTableName">The name of the executive table in the database.</param>
        public ExecProfileManager(IExecImageManager imageManager, LCSDBContext dbContext, string tableName = "ExecProfiles", string execTableName = "Execs")
        {
            this.ProfileMap = new Dictionary<long, ExecProfile>();
            this.ImageManager = imageManager as ExecImageManager;
            this.DBContext = dbContext;
        }
    }
}