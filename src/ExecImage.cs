using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OpenDatabase;
using OpenDatabase.Json;
using OpenDatabaseAPI;

namespace LangaraCPSC.WebAPI
{
    public class ExecImageBase64 : IRecord, IPayload
    {
        public long ID;

        public string Path;

        protected string Buffer;

        public bool SaveToFile(string file)
        {
            return FileIO.WriteToFile(this.Buffer, file);
        }
        
        public bool SaveToFile()
        {
            return FileIO.WriteToFile(this.Buffer, $"{this.ID}.png");
        }

        public static ExecImageBase64 LoadFromFile(string file)
        {
            string[] split = null;

            return new ExecImageBase64(int.Parse((split = file.Split('.'))[0]), FileIO.ReadFromFile(file));
        }

        public static ExecImageBase64 FromRecord(Record record)
        {
            try
            {
                return ExecImageBase64.LoadFromFile(record.Values[1].ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }

        public string ToJson()
        {
            Hashtable imageMap = new Hashtable();

            imageMap.Add("ID", this.ID);
            imageMap.Add("Buffer", this.Buffer);

            return JsonConvert.SerializeObject(imageMap);
        }

        public Record ToRecord()
        {
            return new Record(new string[]
            {
                "ID",
                "Path"
            }, 
            new object[]
            {
                this.ID,
                this.Path
            });
        }

        public ExecImageBase64(long id, string buffer)
        { 
            this.ID = id;
            this.Buffer = buffer;

            this.Path = $"{this.ID}.png";
        }
    }

    public class ExecImageManager
    {
        protected PostGRESDatabase DatabaseInstance;

        protected Dictionary<long, ExecImageBase64> ExecImageMap;

        protected Table ExecImageTable;

        public string ImageDir;

        public static Table GetTableSchema(string tableName)
        {
            return new Table(tableName, new Field[]
            {
                new Field("ID", FieldType.VarChar, new Flag[] { Flag.NotNull, Flag.PrimaryKey }, 64),
                new Field("Path", FieldType.VarChar, new Flag[] { Flag.NotNull }, 512)
            });
        }

        public bool AddExecImage(ExecImageBase64 execImage)
        {
            return this.DatabaseInstance.InsertRecord(execImage.ToRecord(), this.ExecImageTable.Name);
        }

        public bool ExecImageExists(long id)
        {
            return (this.DatabaseInstance.FetchQueryData($"SELECT * FROM {this.ExecImageTable.Name} WHERE ID=\'{id}\'",
                this.ExecImageTable.Name).Length > 0);
        }

        public ExecImageBase64 GetImageByID(long id)
        {
            if (this.ExecImageMap.ContainsKey(id))
                return this.ExecImageMap[id];

            ExecImageBase64 image = null;

            Record[] records;

            if ((records = this.DatabaseInstance.FetchQueryData(
                    $"SELECT * FROM {this.ExecImageTable.Name} WHERE ID=\'{id}\'", this.ExecImageTable.Name)) == null)
                return null;
            
            if (records.Length < 1)
                return null;
            
            Console.WriteLine(JsonConvert.SerializeObject(records));
           
            this.ExecImageMap.Add((image = ExecImageBase64.LoadFromFile(records[0].Values[1].ToString())).ID, image);

            return image;
        }

        public bool LoadImages()
        {
            Record[] imageRecords = this.DatabaseInstance.FetchQueryData($"SELECT * FROM {this.ExecImageTable.Name}", this.ExecImageTable.Name);

            ExecImageBase64 image;

            try
            {
                for (int x = 0; x < imageRecords.Length; x++)
                   this.ExecImageMap.Add((image = ExecImageBase64.FromRecord(imageRecords[x])).ID, image); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                
                return false;
            }
            
            return true;
        }

        public bool DeleteImage(long id)
        {
            if (this.ExecImageExists(id))
                return this.DatabaseInstance.ExecuteQuery($"DELETE FROM {this.ExecImageTable.Name}");

            return false;
        }

        public void AssertTable()
        {
            if (!this.DatabaseInstance.TableExists(this.ExecImageTable.Name))
                this.DatabaseInstance.ExecuteQuery(this.ExecImageTable.GetCreateQuery());
        }

        public ExecImageManager(DatabaseConfiguration configuration, string tableName = "ExecImages", string imageDir = "Images")
        {
            this.DatabaseInstance = new PostGRESDatabase(configuration);

            this.ExecImageMap = new Dictionary<long, ExecImageBase64>();
            this.ExecImageTable = ExecImageManager.GetTableSchema(tableName);
            this.ImageDir = imageDir;

            this.DatabaseInstance.Connect();
            this.AssertTable();
                        
            FileIO.AssertDirectory(this.ImageDir);
        }
    }
} 