using System.Collections;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Razor.TagHelpers;
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
            return new ExecProfile((long)record.Values[0], record.Values[1].ToString(), record.Values[2].ToString());
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

        public ExecProfileManager(DatabaseConfiguration configuration, string tableName = "ExecProfiles")
        {
            this.DatabaseInstance = new PostGRESDatabase(configuration);
            this.ProfileMap = new Dictionary<long, ExecProfile>();

            this.DatabaseInstance.Connect();

            this.ExecProfileTable = new Table(tableName, new Field[] {
                new Field("ID", FieldType.Int, new Flag[] { Flag.NotNull, Flag.PrimaryKey }),
                new Field("ImageID", FieldType.Char, new Flag[] { Flag.NotNull }, 36 ),
                new Field("Description", FieldType.VarChar, new Flag[] { Flag.NotNull }, 10000)
            });
            
            this.AssertTable();
        }
    }
} 
