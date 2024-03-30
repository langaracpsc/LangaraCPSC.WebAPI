using System.Net.Http.Headers;
using LangaraCPSC.WebAPI.DbModels;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Update;
using Newtonsoft.Json;

namespace LangaraCPSC.WebAPI
{
    /// <summary>
    /// Represents the different executive positions within the organization.
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
    /// Represents the name of an executive.
    /// </summary>
    public struct ExecName
    {
        /// <summary>
        /// The first name of the executive.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The last name of the executive.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecName"/> struct.
        /// </summary>
        /// <param name="firstName">The first name of the executive.</param>
        /// <param name="lastName">The last name of the executive.</param>
        public ExecName(string firstName, string lastName)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
        }
    }

    /// <summary>
    /// Represents the tenure of an executive.
    /// </summary>
    public struct ExecTenure
    {
        /// <summary>
        /// The start date of the executive's tenure.
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// The end date of the executive's tenure.
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecTenure"/> struct.
        /// </summary>
        /// <param name="start">The start date of the executive's tenure.</param>
        /// <param name="end">The end date of the executive's tenure.</param>
        public ExecTenure(DateTime start, DateTime end = new DateTime())
        {
            this.Start = start;
            this.End = end;
        }
    }

    /// <summary>
    /// Represents an executive within the organization.
    /// </summary>
    public class Exec : IPayload
    {
        /// <summary>
        /// The unique identifier of the executive.
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        /// The name of the executive.
        /// </summary>
        public ExecName Name { get; set; }

        /// <summary>
        /// The position held by the executive.
        /// </summary>
        public ExecPosition Position { get; set; }

        /// <summary>
        /// The email address of the executive.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The tenure of the executive.
        /// </summary>
        public ExecTenure Tenure { get; set; }

        /// <summary>
        /// An array of strings representing the different executive positions.
        /// </summary>
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

        /// <summary>
        /// Creates a new instance of the <see cref="Exec"/> class from a database model.
        /// </summary>
        /// <param name="model">The database model to create the executive from.</param>
        /// <returns>A new instance of the <see cref="Exec"/> class.</returns>
        public static Exec FromModel(DbModels.Exec model)
        {
            return new Exec(model.Id,
                new ExecName { FirstName = model.Firstname, LastName = model.Lastname },
                model.Email,
                (ExecPosition)model.Position,
                new ExecTenure { Start = DateTime.Parse(model.Tenurestart), End = (model.Tenureend == null) ? new DateTime() : DateTime.Parse(model.Tenureend) });
        }

        /// <summary>
        /// Converts the current instance of the <see cref="Exec"/> class to a database model.
        /// </summary>
        /// <returns>A new instance of the <see cref="DbModels.Exec"/> class.</returns>
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

        /// <summary>
        /// Converts the current instance of the <see cref="Exec"/> class to a JSON string.
        /// </summary>
        /// <returns>A JSON string representation of the <see cref="Exec"/> instance.</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Exec"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the executive.</param>
        /// <param name="name">The name of the executive.</param>
        /// <param name="email">The email address of the executive.</param>
        /// <param name="position">The position held by the executive.</param>
        /// <param name="tenure">The tenure of the executive.</param>
        public Exec(long id, ExecName name, string email, ExecPosition position, ExecTenure tenure)
        {
            this.ID = id;
            this.Name = name;
            this.Email = email;
            this.Position = position;
            this.Tenure = tenure;
        }
    }

    /// <summary>
    /// Defines the interface for managing executives.
    /// </summary>
    public interface IExecManager
    {
        /// <summary>
        /// Creates a new executive.
        /// </summary>
        /// <param name="studentID">The unique identifier of the student.</param>
        /// <param name="name">The name of the executive.</param>
        /// <param name="email">The email address of the executive.</param>
        /// <param name="position">The position held by the executive.</param>
        /// <param name="tenure">The tenure of the executive.</param>
        /// <returns>The newly created executive.</returns>
        Exec CreateExec(long studentID, ExecName name, string email, ExecPosition position, ExecTenure tenure);

        /// <summary>
        /// Ends the tenure of an executive.
        /// </summary>
        /// <param name="id">The unique identifier of the executive.</param>
        void EndTenure(long id);

        /// <summary>
        /// Gets a list of all executives.
        /// </summary>
        /// <returns>A list of all executives.</returns>
        List<Exec> GetExecs();

        /// <summary>
        /// Gets an executive by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the executive.</param>
        /// <returns>The executive with the specified identifier, or null if not found.</returns>
        Exec? GetExec(long id);

        /// <summary>
        /// Updates an existing executive.
        /// </summary>
        /// <param name="updateModel">The updated executive model.</param>
        /// <returns>The updated executive, or null if the update failed.</returns>
        Exec? UpdateExec(DbModels.Exec updateModel);
    }

    /// <summary>
    /// Implements the <see cref="IExecManager"/> interface.
    /// </summary>
    public class ExecManager : IExecManager
    {
        /// <summary>
        /// A dictionary that maps executive IDs to their corresponding <see cref="Exec"/> instances.
        /// </summary>
        public Dictionary<long, Exec> ExecMap;

        /// <summary>
        /// The database context used by the <see cref="ExecManager"/>.
        /// </summary>
        private readonly LCSDBContext _DbContext;

        /// <summary>
        /// An array of valid keys for updating an executive.
        /// </summary>
        protected static string[] ValidKeys = new string[] {
            "ID",
            "FirstName",
            "LastName",
            "Email",
            "Position",
            "TenureStart",
            "TenureEnd"
        };

        /// <summary>
        /// Creates a new executive.
        /// </summary>
        /// <param name="studentID">The unique identifier of the student.</param>
        /// <param name="name">The name of the executive.</param>
        /// <param name="email">The email address of the executive.</param>
        /// <param name="position">The position held by the executive.</param>
        /// <param name="tenure">The tenure of the executive.</param>
        /// <returns>The newly created executive.</returns>
        public Exec CreateExec(long studentID, ExecName name, string email, ExecPosition position, ExecTenure tenure)
        {
            Exec exec;

            this._DbContext.Execs.Add((exec = new Exec(studentID, name, email, position, tenure)).ToModel());

            this._DbContext.SaveChanges();

            this.ExecMap.Add(exec.ID, exec);

            return new Exec(studentID, name, email, position, tenure);
        }

        /// <summary>
        /// Ends the tenure of an executive.
        /// </summary>
        /// <param name="id">The unique identifier of the executive.</param>
        public void EndTenure(long id)
        {
            DbModels.Exec? exec = this._DbContext.Execs.Where(e => e.Id == id).FirstOrDefault();

            if (exec == null)
                throw new Exception($"Exec with ID \"{id}\" not found.");

            if (exec.Tenureend == new DateTime().ToString())
                exec.Tenureend = DateTime.Now.ToString();

            this._DbContext.SaveChanges();
        }

        /// <summary>
        /// Gets a list of all executives.
        /// </summary>
        /// <returns>A list of all executives.</returns>
        public List<Exec> GetExecs()
        {
            return this._DbContext.Execs.Select(e => Exec.FromModel(e)).ToList();
        }

        /// <summary>
        /// Gets an executive by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the executive.</param>
        /// <returns>The executive with the specified identifier, or null if not found.</returns>
        public Exec? GetExec(long id)
        {
            if (this.ExecMap.ContainsKey(id))
                return this.ExecMap[id];

            DbModels.Exec? exec = this._DbContext.Execs.Where(e => e.Id == id).FirstOrDefault();

            if (exec == null)
                return null;

            Exec e = Exec.FromModel(exec);

            this.ExecMap.Add(id, e);

            return this.ExecMap[id];
        }

        /// <summary>
        /// Deletes an executive.
        /// </summary>
        /// <param name="id">The unique identifier of the executive.</param>
        /// <returns>True if the executive was successfully deleted, false otherwise.</returns>
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

        /// <summary>
        /// Checks if a given key is valid for updating an executive.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key is valid, false otherwise.</returns>
        protected static bool IsKeyValid(string key)
        {
            if (Array.BinarySearch(ExecManager.ValidKeys, key) == -1)
                return false;

            return true;
        }

        /// <summary>
        /// Updates an existing executive.
        /// </summary>
        /// <param name="updateModel">The updated executive model.</param>
        /// <returns>The updated executive, or null if the update failed.</returns>
        public Exec? UpdateExec(DbModels.Exec? updateModel)
        {
            DbModels.Exec? exec;

            try
            {
                if (updateModel?.Id == null || updateModel.Id == 0)
                    throw new NullReferenceException();

                exec = this._DbContext.Execs.First(e => e.Id == updateModel.Id);

                if (exec == null)
                    throw new Exception($"Exec with id {updateModel.Id} not found");
                
                exec.Firstname = updateModel.Firstname ?? exec.Firstname;
                exec.Lastname = updateModel.Lastname ?? exec.Lastname;
                exec.Position = updateModel.Position ?? exec.Position;
                exec.Tenurestart = updateModel.Tenurestart ?? exec.Tenurestart;
                exec.Tenureend = updateModel.Tenureend ?? exec.Tenureend;
                exec.Email = updateModel.Email ?? exec.Email;

                this._DbContext.SaveChanges();
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine("id not provided for the record to update.");
                throw;
            }

            Exec updated = Exec.FromModel(exec);

            if (this.ExecMap.ContainsKey(updated.ID)) 
                this.ExecMap[updated.ID] = updated;
            else
                this.ExecMap.Add(updated.ID, updated);

            return updated;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecManager"/> class.
        /// </summary>
        /// <param name="dbContext">The database context to use.</param>
        public ExecManager(LCSDBContext dbContext)
        {
            this._DbContext = dbContext;
            this._DbContext.Database.EnsureCreated();

            this.ExecMap = new Dictionary<long, Exec>();
        }
    }
}