using System.Collections;
using System.Net.Mime;
using Newtonsoft.Json;
using OpenDatabase;
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
            
            for (int x = 0; x < records.Length; x++)
                activeExecs.Add(Exec.FromRecord(records[x]));

            return activeExecs;
        }

        public ExecImageProfile GetExecImageProfile(string id)
        {
            Record[] profileRecords = this.DatabaseInstance.FetchQueryData(
                        $"SELECT * FROM {this.ExecProfileTable.Name} WHERE ID=\"{id}\"", this.ExecProfileTable.Name),
                imageRecords = null; 
                
            ExecProfile profile = null;

            if (profileRecords.Length < 1)
                return null;
            
            
            return new ExecImageProfile(profile, null);
        }

        public List<ExecImageProfile> GetActiveImageProfiles()
        {
            List<ExecImageProfile> execImageProfiles = new List<ExecImageProfile>();
            
            
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

        public ExecProfile UpdateExecProfile(Hashtable updateMap)
        {
            string[] keys = new string[updateMap.Keys.Count];
            object[] values = new object[updateMap.Values.Count];

            long id = (int)updateMap["id"]; 
            
            updateMap.Keys.CopyTo(keys, 0);
            updateMap.Values.CopyTo(values, 0);
            
            return (this.DatabaseInstance.UpdateRecord(new Record(new string[]{ "id" }, new object[]{ updateMap["id"]}), new Record(keys, values), this.ExecProfileTable.Name)) ? this.GetProfileById(id) : null; 
        }

        public bool DeleteProfileWithID(long id)
        {
            if (this.ProfileExists(id))
                return this.DatabaseInstance.ExecuteQuery($"DELETE FROM {this.ExecProfileTable.Name} WHERE ID=\'{this.ExecProfileTable.Name}\';");
            
            return false;
        }

        private void AssertTable()
        {
            if (!this.DatabaseInstance.TableExists(this.ExecProfileTable.Name))
                this.DatabaseInstance.ExecuteQuery(this.ExecProfileTable.GetCreateQuery());
        }

        public ExecProfileManager(DatabaseConfiguration configuration, string tableName = "ExecProfiles", string execTableName = "Execs")
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
            
            this.AssertTable();
        }
    }

    public class ExecImageProfile : IPayload
    {
        protected ExecProfile Profile;

        public ExecImageBase64 Image { get; private set; }

        public string ToJson()
        {
            return null;
        }

        public ExecImageProfile(ExecProfile profile, ExecImageBase64 image)
        {
            this.Profile = profile;
            this.Image = image;
        }
    }
} 
