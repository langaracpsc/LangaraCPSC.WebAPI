using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Routing.Constraints;
using OpenDatabase;
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

        public ExecTenure(DateTime start, DateTime end)
        {
            this.Start = start;
            this.End = end;
        }
    }

    /// <summary>
    /// Stores info about an Exec
    /// </summary>
    public class Exec : IRecord
    {
        public string ID { get; set; }

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
                "Start",
                "End"
            }, new object[] {
                this.ID,
                this.Name.FirstName,
                this.Name.LastName,
                (int)this.Position 
            });
        }

        public static Exec FromRecord(Record record)
        {
            return new Exec(record.Values[0].ToString(), new ExecName(record.Values[1].ToString(), record.Values[2].ToString()), (ExecPosition)((int)record.Values[3]), new ExecTenure(DateTime.Parse(record.Values[4].ToString()), DateTime.Parse(record.Values[5].ToString())));
        }
        public Exec(string id, ExecName name, ExecPosition position, ExecTenure tenure)
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
        
        public Dictionary<string, Exec> ExecMap;

        public Exec CreateExec(ExecName name, ExecPosition position, ExecTenure tenure)
        {
            Exec exec;
            
            this.DatabaseConnection.InsertRecord((exec = new Exec(Guid.NewGuid().ToString(), name, position, tenure)).ToRecord(), this.ExecTableName);
            this.ExecMap.Add(exec.ID, exec);
            
            return exec;
        }

        public void EndTenure(string id)
        {
            Record[] records = this.DatabaseConnection.FetchQueryData($"SELECT * FROM {this.ExecTableName} WHERE ID=\'{id}\'", this.ExecTableName);

            if (records.Length < 1)
                throw new Exception($"Exec with ID \"{id}\" not found.");

            this.DatabaseConnection.UpdateRecord(new Record(new string[]{"ID"}, new object[]{ id }), records[0], this.ExecTableName);
        }

        public ExecManager(DatabaseConfiguration databaseConfiguration, string execTable = "Execs")
        {
            this.DatabaseConnection = new PostGRESDatabase(databaseConfiguration);
            this.ExecTableName = execTable;
            this.ExecMap = new Dictionary<string, Exec>();
            
            this.DatabaseConnection.Connect();

            this.ExecTable = new Table(this.ExecTableName, new Field[] {
                new Field("ID", FieldType.Char, new Flag[]{ Flag.PrimaryKey, Flag.NotNull }, 32),
                new Field("FirstName", FieldType.VarChar, new Flag[] { Flag.NotNull }, 64),
                new Field("LastName", FieldType.VarChar, new Flag[] {} , 64),
                new Field("Position", FieldType.Int, new Flag[]{ Flag.NotNull }),
                new Field("Start", FieldType.VarChar, new Flag[] { Flag.NotNull }, 64)
            }); 
        }
    }
}
