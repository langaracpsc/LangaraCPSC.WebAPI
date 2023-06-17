using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Newtonsoft.Json;
using OpenDatabase;
using OpenDatabaseAPI;

namespace LangaraCPSC.WebAPI
{
    public class ExecProfile : IRecord, IPayload
    {
        public string ID;

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
            return new ExecProfile(record.Values[0].ToString(), record.Values[1].ToString(), record.Values[2].ToString());
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public ExecProfile(string id, string imageID, string description)
        {
            this.ID = id;
            this.ImageID = imageID;
            this.Description = description;
        }
    }

    public class ExecProfileManager
    {
        private PostGRESDatabase DatabaseInstance;

        private Dictionary<string, ExecProfile> ProfileMap;

        public Table ExecProfileTable;

        public bool ProfileExists(string id)
        {
            if (this.ProfileMap.ContainsKey(id))
                return true;
            
            return (this.DatabaseInstance.FetchQueryData($"SELECT * FROM {this.ExecProfileTable.Name}", this.ExecProfileTable.Name).Length > 0);
        }

        public void CreateProfile(string id, string imageID, string description)
        {
            ExecProfile profile;

            this.DatabaseInstance.InsertRecord((profile = new ExecProfile(id, imageID, description)).ToRecord(), this.ExecProfileTable.Name);
            
            this.ProfileMap.Add(id, profile); 
        }

        public bool UpdateProfile(string id, string imageID, string description)
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

        public ExecProfile GetProfileById(string id)
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

        public bool DeleteProfileWithID(string id)
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

        public ExecProfileManager(DatabaseConfiguration configuration, string tableName = "ExecProfiles")
        {
            this.DatabaseInstance = new PostGRESDatabase(configuration);
            this.ProfileMap = new Dictionary<string, ExecProfile>();

            this.DatabaseInstance.Connect();

            this.ExecProfileTable = new Table(tableName, new Field[] {
                new Field("ID", FieldType.Char, new Flag[] { Flag.NotNull, Flag.PrimaryKey }, 36),
                new Field("ImageID", FieldType.Char, new Flag[] { Flag.NotNull }, 36 ),
                new Field("Description", FieldType.VarChar, new Flag[] { Flag.NotNull }, 10000)
            });
            
            this.AssertTable();
        }
    }
} 

 