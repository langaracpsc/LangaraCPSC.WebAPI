using LangaraCPSC.WebAPI.DbModels;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace LangaraCPSC.WebAPI
{
    /// <summary>
    /// Exec position enum
    /// </summary>
    public enum ExecPosition
    {
        President,
        VicePresident,
        VicePresidentInternal,
        VicePresidentExternal,
        TechLead,
        AssistTechLead,
        GeneralRep,
        PublicRelations,
        Finance,
        Events,
        Secretary,
        Media
    }

    /// <summary>
    /// Stores a name
    /// </summary>
    public struct ExecName
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public ExecName(string firstName, string lastName)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
        }
    }
    
    public struct ExecTenure
    {
        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public ExecTenure(DateTime start, DateTime end = new DateTime())
        {
            this.Start = start;
            this.End = end;
        }
    }

    /// <summary>
    /// Stores info about an Exec
    /// </summary>
    public class Exec : IPayload
    {
        public long ID { get; set; }

        public ExecName Name { get; set; }

        public ExecPosition Position { get; set; }

        public string Email { get; set; }

        public ExecTenure Tenure { get; set; }

        public static string[] PositionStrings = new string[] {
            "President",
            "Vice President",
            "Vice President Internal",
            "Vice President External",
            "Tech Lead",
            "Assistant Tech Lead",
            "General Representative",
            "Director of Public Relations",
            "Director of Finance",
            "Director of Events",
            "Secratory",
            "Directory of Media"
        };

        public static Exec FromModel(DbModels.Exec model)
        {
            return new Exec(model.Id,
                new ExecName { FirstName = model.Firstname, LastName = model.Lastname },
                model.Email,
                (ExecPosition)model.Position,
                new ExecTenure { Start = DateTime.Parse(model.Tenurestart), End = (model.Tenureend == null) ? new DateTime() : DateTime.Parse(model.Tenureend) });
        }

        public DbModels.Exec ToModel()
        {
            return new DbModels.Exec
            {
                Id = (int)this.ID,
                Firstname = this.Name.FirstName,
                Lastname = this.Name.LastName,
                Email = this.Email,
                Position = (int)this.Position,
                Tenurestart = this.Tenure.Start.ToString(),
                Tenureend = this.Tenure.End.ToString()
            };
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public Exec(long id, ExecName name, string email, ExecPosition position, ExecTenure tenure)
        {
            this.ID = id;
            this.Name = name;
            this.Email = email;
            this.Position = position;
            this.Tenure = tenure;
        }
    }

    public interface IExecManager
    {
        Exec CreateExec(long studentID, ExecName name, string email, ExecPosition position, ExecTenure tenure);

        void EndTenure(long id);

        List<Exec> GetExecs();

        Exec? GetExec(long id);
        
        Exec? UpdateExec(DbModels.Exec updateModel);
    }

    public class ExecManager : IExecManager
    {
        public Dictionary<long, Exec> ExecMap;

        private readonly LCSDBContext _DbContext;
        
        protected static string[] ValidKeys = new string[] { 
            "ID",
            "FirstName",
            "LastName",
            "Email",
            "Position",
            "TenureStart",
            "TenureEnd" 
        };
        
        public Exec CreateExec(long studentID, ExecName name, string email, ExecPosition position, ExecTenure tenure)
        {
            Exec exec;
            
            this._DbContext.Execs.Add((exec = new Exec(studentID, name, email, position, tenure)).ToModel());
            
            this._DbContext.SaveChanges();
            
            this.ExecMap.Add(exec.ID, exec);

            return new Exec(studentID, name, email, position, tenure);
        }

        public void EndTenure(long id)
        {
            DbModels.Exec? exec = this._DbContext.Execs.Where(e => e.Id == id).FirstOrDefault();
            
            if (exec == null)  
                throw new Exception($"Exec with ID \"{id}\" not found.");

            if (exec.Tenureend == new DateTime().ToString())
                exec.Tenureend = DateTime.Now.ToString();

            this._DbContext.SaveChanges();
        }
        
        public List<Exec> GetExecs()
        {   
            return this._DbContext.Execs.Select(e => Exec.FromModel(e)).ToList();
        }

        public Exec? GetExec(long id)
        {
            if (this.ExecMap.ContainsKey(id))
                return this.ExecMap[id];

            DbModels.Exec? exec = this._DbContext.Execs.Where(e => e.Id == id).FirstOrDefault();

            if (exec ==  null)
                return null;
       
            Exec e = Exec.FromModel(exec);
        
            this.ExecMap.Add(id, e);
            
            return this.ExecMap[id];
        }

        public bool DeleteExec(long id)
        {
            try
            {
                DbModels.Exec? exec;

                if ((exec = this._DbContext.Execs.Where(e => e.Id == id).FirstOrDefault()) == null)
                    throw new NullReferenceException();
                    
                this._DbContext.Execs.Remove(exec);
                this._DbContext.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

            return true;
        }

        protected static bool IsKeyValid(string key)
        {
            if (Array.BinarySearch(ExecManager.ValidKeys, key) == -1)
                return false;

            return true;
        }

        public Exec? UpdateExec(DbModels.Exec? updateModel)
        {
            DbModels.Exec? exec;

            try
            {
                if (updateModel?.Id == null || updateModel.Id == 0)
                    throw new NullReferenceException();
                
                exec = this._DbContext.Execs.Where(e => e.Id == updateModel.Id).FirstOrDefault();

                if (exec == null)
                    throw new Exception($"Exec with id {updateModel.Id} not found");
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine("id not provided for the record to update.");
                throw;
            }

            exec.Firstname = updateModel.Firstname ?? exec.Firstname;
            exec.Lastname = updateModel.Lastname ?? exec.Lastname;
            exec.Position = updateModel.Position ?? exec.Position;
            exec.Tenurestart = updateModel.Tenurestart ?? exec.Tenurestart;
            exec.Tenureend = updateModel.Tenureend ?? exec.Tenureend;
            exec.Email = updateModel.Email ?? exec.Email;
            
            return Exec.FromModel(exec);
        }

        public ExecManager(LCSDBContext dbContext)
        {
            this._DbContext = dbContext;
            this._DbContext.Database.EnsureCreated();
            
            this.ExecMap = new Dictionary<long, Exec>();
        }
    }
} 
 
 
 