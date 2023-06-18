using System.Runtime.CompilerServices;
using OpenDatabase;
using OpenDatabaseAPI;

namespace LangaraCPSC.WebAPI;

public class ExecImageBase64 : IRecord
{
    public string ID;


    public string Path; 
    
    protected string Buffer;

    public bool SaveToFile(string file)
    {
        return FileIO.WriteToFile(this.Buffer, file); 
    }

    public static ExecImageBase64 LoadFromFile(string file)
    {
        string[] split = null;
        
        return new ExecImageBase64((split = file.Split('.'))[0], FileIO.ReadFromFile(file));
    }

    public Record ToRecord()
    {
        return new Record(new string[] {
            "ID",
            "Path"
             
        }, new object[]
        {
            this.ID,
            this.Path
        });
    }
    
    public ExecImageBase64(string id, string buffer)
    {
        this.ID = id;
        this.Buffer = buffer;
    }
}

public class ExecImageManager
{
    protected PostGRESDatabase DatabaseInstance;

    protected Dictionary<string, ExecImageBase64> ExecImageMap;

    protected Table ExecImageTable;
    
    public static Table GetTableSchema(string tableName)
    {
        return new Table(tableName, new Field[] {
            new Field("ID", FieldType.VarChar, new Flag[]{Flag.NotNull, Flag.PrimaryKey}, 64),
            new Field("Path", FieldType.VarChar, new Flag[]{Flag.NotNull, Flag.PrimaryKey}, 512)
        });
    }
    
    public bool AddExecImage(ExecImageBase64 execImage)
    {
        return this.DatabaseInstance.InsertRecord(execImage.ToRecord(), this.ExecImageTable.Name);
    }

    public ExecImageManager(DatabaseConfiguration configuration, string tableName = "ExecImages")
    {
        this.DatabaseInstance = new PostGRESDatabase(configuration);

        this.ExecImageMap = new Dictionary<string, ExecImageBase64>();
        this.ExecImageTable = ExecImageManager.GetTableSchema(tableName);
    }
}


