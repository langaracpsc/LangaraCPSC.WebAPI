using System.Collections;
using Newtonsoft.Json;
using OpenDatabase;
using OpenDatabase.Json;
using OpenDatabaseAPI;

namespace LangaraCPSC.WebAPI
{

    /// <summary>
    /// Exec position enum
    /// </summary>
    public enum ExecPosition
    {
        President,
        VicePresident,
        TechLead,
        GeneralRep,
        PublicRelations,
        Finance
    }

    /// <summary>
    /// Stores a name
    /// </summary>
    public struct ExecName
    {
        public string FirstName;
        public string LastName;

        public ExecName(string firstName, string lastName)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
        }
    }

    public struct ExecTenure
    {
        public DateTime Start;

        public DateTime End;

        public ExecTenure(DateTime start, DateTime end = new DateTime())
        {
            this.Start = start;
            this.End = end;
        }
    }

    /// <summary>
    /// Stores info about an Exec
    /// </summary>
    public class Exec : IRecord, IPayload
    {
        public long ID { get; set; }

        public ExecName Name { get; set; }

        public ExecPosition Position;
        
        public ExecTenure Tenure { get; set; }

        public Record ToRecord()
        {
            return new Record(new string[] {
                "ID",
                "FirstName",
                "LastName",
                "Position",
                "TenureStart",
                "TenureEnd"
            }, new object[] {
                this.ID,
                this.Name.FirstName,
                this.Name.LastName,
                (int)this.Position,
                this.Tenure.Start.ToString(), 
                (this.Tenure.End == new DateTime()) ? null : this.Tenure.End.ToString()
            });
        }

        public static Exec FromRecord(Record record)
        {
               return new Exec((int)record.Values[0],
                        new ExecName(record.Values[1].ToString(), record.Values[2].ToString()),
                        (ExecPosition)((int)record.Values[3]),
                        new ExecTenure(DateTime.Parse(record.Values[4].ToString()), 
                        ((record.Values[5] == null || record.Values[5] == "")) ? new DateTime() : DateTime.Parse(record.Values[5].ToString())));
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public Exec(long id, ExecName name, ExecPosition position, ExecTenure tenure)
        {
            this.ID = id;
            this.Name = name;
            this.Position = position;
            this.Tenure = tenure;
        }
    }


    public class ExecManager
    {
        public PostGRESDatabase DatabaseConnection;

        public string ExecTableName;

        protected Table ExecTable;

        public Dictionary<long, Exec> ExecMap;

        protected static string[] ValidKeys = new string[] { 
            "ID",
            "FirstName",
            "LastName",
            "Position",
            "TenureStart",
            "TenureEnd" 
        };
        
        public Exec CreateExec(long studentID, ExecName name, ExecPosition position, ExecTenure tenure)
        {
            Exec exec;

            this.DatabaseConnection.InsertRecord((exec = new Exec(studentID, name, position, tenure)).ToRecord(), this.ExecTableName);
            this.ExecMap.Add(exec.ID, exec);

            return exec;
        }

        public void EndTenure(long id)
        {
            Record[] records =
                this.DatabaseConnection.FetchQueryData($"SELECT * FROM {this.ExecTableName} WHERE ID={id}",
                    this.ExecTableName);

            if (records.Length < 1)
                throw new Exception($"Exec with ID \"{id}\" not found.");

            Exec exec = Exec.FromRecord(records[0]);

            exec.Tenure = new ExecTenure(exec.Tenure.Start, DateTime.Now);

            this.DatabaseConnection.UpdateRecord(new Record(new string[] { "ID" }, new object[] { id }),
                exec.ToRecord(), this.ExecTableName);
        }

        /// <summary>
        /// Checks and creates the the exec table if it doesnt exist
        /// </summary>
        public void AssertTable()
        {
            bool b;
            if (!(b = this.DatabaseConnection.TableExists(this.ExecTableName)))
                this.DatabaseConnection.ExecuteQuery(this.ExecTable.GetCreateQuery());

            Console.WriteLine(b);
        }

        public List<Exec> GetExecs()
        {
            List<Exec> execs = new List<Exec>();

            Record[] fetchedRecords = this.DatabaseConnection.FetchQueryData($"SELECT * FROM {this.ExecTableName}", this.ExecTableName);

            for (int x = 0; x < fetchedRecords.Length; x++)
                execs.Add(Exec.FromRecord(fetchedRecords[x]));

            return execs;
        }

        public Exec GetExec(long id)
        {
            Record[] records = this.DatabaseConnection.FetchQueryData($"SELECT * FROM {this.ExecTable.Name} WHERE ID={id}", this.ExecTable.Name);

            if (records.Length == 0)
                return null;

            return Exec.FromRecord(records[0]);
        }

        protected static bool IsKeyValid(string key)
        {
            if (Array.BinarySearch(ExecManager.ValidKeys, key) == -1)
                return false;

            return true;
        }

        public Exec UpdateExec(Hashtable updateMap)
        {
            string[] keys = new string[updateMap.Keys.Count];
            object[] values = new object[updateMap.Values.Count];

            long id;
            
            updateMap.Values.CopyTo(values, 0);
            updateMap.Keys.CopyTo(keys,0);

            foreach (string key in keys)
                if (!ExecManager.IsKeyValid(key))
                    return null;
            
            return (this.DatabaseConnection.UpdateRecord(new Record(new string[]{ "ID" }, new object[]{ id = (long)updateMap["ID"] }), new Record(keys, values), this.ExecTableName))  
                    ? this.GetExec(id) : null;
        }

        public ExecManager(DatabaseConfiguration databaseConfiguration, string execTable = "Execs")
        {
            this.DatabaseConnection = new PostGRESDatabase(databaseConfiguration);
            this.ExecTableName = execTable;
            this.ExecMap = new Dictionary<long, Exec>();
            
            this.DatabaseConnection.Connect();

            this.ExecTable = new Table(this.ExecTableName, new Field[] {
                new Field("ID", FieldType.Char, new Flag[]{ Flag.PrimaryKey, Flag.NotNull }, 36),
                new Field("FirstName", FieldType.VarChar, new Flag[] { Flag.NotNull }, 64),
                new Field("LastName", FieldType.VarChar, new Flag[] {} , 64),
                new Field("Position", FieldType.Int, new Flag[]{ Flag.NotNull }),
                new Field("TenureStart", FieldType.VarChar, new Flag[] { Flag.NotNull }, 64),
                new Field("TenureEnd", FieldType.VarChar, new Flag[] { }, 64)
            });
            
            this.AssertTable();
        }
    }
} 
 
 
 