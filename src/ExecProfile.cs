using System.Collections;
using System.Net.Mime;
using System.Text.Json;
using Newtonsoft.Json;
using OpenDatabase;
using OpenDatabase.Json;
using OpenDatabaseAPI;

namespace LangaraCPSC.WebAPI
{
    public class ExecProfile : IRecord, IPayload
    {
        public long ID;

        public string ImageID;

        public string Description;

        public Record ToRecord()
        {
            return new Record(new string[]
            {
                "ID",
                "ImageID",
                "Description"
            }, new object[]
            {
                this.ID,
                this.ImageID,
                this.Description
            });
        }
      
        public static ExecProfile FromRecord(Record record)
        {
            return new ExecProfile((int)record.Values[0], record.Values[1].ToString(), record.Values[2].ToString());
        }

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

        public ExecProfile(long id, string imageID, string description)
        {
            this.ID = id;
            this.ImageID = imageID;
            this.Description = description;
        }
    }

    public class ExecProfileManager
    {
        private PostGRESDatabase DatabaseInstance;

        private Dictionary<long, ExecProfile> ProfileMap;

        private ExecImageManager ImageManager;
        
        public Table ExecProfileTable;

        public string ExecTableName; 
        
        public bool ProfileExists(long id)
        {
            if (this.ProfileMap.ContainsKey(id))
                return true;
            return (this.DatabaseInstance.FetchQueryData($"SELECT * FROM {this.ExecProfileTable.Name}", this.ExecProfileTable.Name).Length > 0);
        }

        public ExecProfile CreateProfile(long id, string imageID, string description)
        {
            ExecProfile profile;

            if (!this.DatabaseInstance.InsertRecord((profile = new ExecProfile(id, imageID, description)).ToRecord(),
                    this.ExecProfileTable.Name))
                return null;
            
            this.ProfileMap.Add(id, profile);

            return profile;
        }

        public bool UpdateProfile(long id, string imageID, string description)
        {
            if (this.ProfileExists(id))
            {
                this.DatabaseInstance.UpdateRecord(
                    new Record(new string[] { "ID" }, new object[] { id }),
                    new Record(new string[] { "ImageID", "Description" }, new object[] { imageID, description }),
                    this.ExecProfileTable.Name);

                return true; 
            }

            return false;
        }

        public ExecProfile GetProfileById(long id)
        {
            if (this.ProfileMap.ContainsKey(id))
                return this.ProfileMap[id];

            Record[] records = this.DatabaseInstance.FetchQueryData($"SELECT * FROM  {this.ExecProfileTable.Name} WHERE ID=\'{id}\'", this.ExecProfileTable.Name);

            ExecProfile profile = null;
            
            if (records.Length < 1)
                return profile;
            
            this.ProfileMap.Add((profile = ExecProfile.FromRecord(records[0])).ID, profile);
  
            return profile;
        }

        public List<Exec> GetActiveExecs()
        {
            List<Exec> activeExecs = null;

            Record[] records = null;
            
            if ((records = this.DatabaseInstance.FetchQueryData($"SELECT * FROM {this.ExecTableName} WHERE TenureEnd IS NULL", this.ExecTableName)) == null)
                return activeExecs;
            if (records.Length < 1) 
                return activeExecs;

            activeExecs = new List<Exec>();
            
            // for (int x = 0; x < records.Length; x++)
            //     activeExecs.Add(Exec.FromRecord(records[x]));

            return activeExecs;
        }

        public ExecImageProfile GetExecImageProfile(long id)
        {
            ExecProfile profile = this.GetProfileById(id);

            if (profile == null)
                return null;

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

            Record[] records;
            
            Record temp;

            if ((execs = this.GetActiveExecs()) == null)
                return null; 
            
            for (int x = 0; x < execs.Count; x++)
            {
                if ((records = this.DatabaseInstance.FetchQueryData(
                        $"SELECT * FROM {this.ExecProfileTable.Name} WHERE ID={execs[x].ID}",
                        this.ExecProfileTable.Name)) == null)
                    continue;
                
                if (records.Length < 1)
                    continue;
        
                profiles.Add(ExecProfile.FromRecord(records[0]));
            }
            
            return profiles;
        }

        public ExecProfile UpdateExecProfileJson(Hashtable updateMap)
        {
            string[] keys = new string[updateMap.Keys.Count];
            object[] values = new object[updateMap.Values.Count];

            long id = ((JsonElement)updateMap["id"]).GetInt64();

            updateMap.Keys.CopyTo(keys, 0);
            updateMap.Values.CopyTo(values, 0);
 
            for (int x = 0; x < values.Length; x++)
                values[x] = Tools.GetTypedJsonElementValue((JsonElement)values[x]);
            
            return (this.DatabaseInstance.UpdateRecord(new Record(new string[]{ "id" }, new object[]{ id }), new Record(keys, values), this.ExecProfileTable.Name)) 
                                ? this.GetProfileById(id)
                                : null;
        }

        public ExecProfile UpdateExecProfile(Hashtable updateMap)
        {
            string[] keys = new string[updateMap.Keys.Count];
            object[] values = new object[updateMap.Values.Count];
    
            long id = (int)updateMap["id"]; 
            
            updateMap.Keys.CopyTo(keys, 0);
            updateMap.Values.CopyTo(values, 0);
            
            return (this.DatabaseInstance.UpdateRecord(new Record(new string[]{ "id" }, new object[]{ updateMap["id"]}), new Record(keys, values), this.ExecProfileTable.Name)) 
                                ? this.GetProfileById(id)
                                : null;
        }

        public bool DeleteProfileWithID(long id)
        {
            if (this.ProfileExists(id))
                return this.DatabaseInstance.ExecuteQuery($"DELETE FROM {this.ExecProfileTable.Name} WHERE ID={id}';");
            
            return false;
        }

        private void AssertTable()
        {
            if (!this.DatabaseInstance.TableExists(this.ExecProfileTable.Name))
                this.DatabaseInstance.ExecuteQuery(this.ExecProfileTable.GetCreateQuery());
        }

        public ExecProfileManager(DatabaseConfiguration configuration, string tableName = "ExecProfiles", string execTableName = "Execs", ExecImageManager imageManager = null)
        {
            this.DatabaseInstance = new PostGRESDatabase(configuration);
            this.ProfileMap = new Dictionary<long, ExecProfile>();
            this.ExecTableName = execTableName; 
            
            this.DatabaseInstance.Connect();

            this.ExecProfileTable = new Table(tableName, new Field[] {
                new Field("ID", FieldType.Int, new Flag[] { Flag.NotNull, Flag.PrimaryKey }),
                new Field("ImageID", FieldType.VarChar, new Flag[] { Flag.NotNull }, 36 ),
                new Field("Description", FieldType.VarChar, new Flag[] { Flag.NotNull }, 10000)
            });

            this.ImageManager = imageManager;
            
            this.AssertTable();
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
} 
