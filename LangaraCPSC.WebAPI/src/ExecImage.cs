using System.Collections;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using LangaraCPSC.WebAPI.DbModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace LangaraCPSC.WebAPI
{
    public class ExecImageBase64 :  IPayload
    {
        public readonly long ID;

        public readonly string Path;

        public string Buffer { get; private set; }

        public bool SaveToFile(string file)
        {
            return FileIO.WriteBytesToFile(Convert.FromBase64String(this.Buffer), file);
        }
        
        public bool SaveToFile()
        {
            return FileIO.WriteBytesToFile(Convert.FromBase64String(this.Buffer), this.Path);
        }

        public static ExecImageBase64 LoadFromFile(string file)
        {
            string[] split = file.Split('/');

            return new ExecImageBase64(int.Parse((split[^1].Split('.'))[0]), Convert.ToBase64String(FileIO.ReadBytesFromFile(file)));
        }

        public static ExecImageBase64 FromModel(DbModels.Execimage image, string? imageDir = null)
        {
            if (imageDir != null) 
                return ExecImageBase64.LoadFromFile($"{imageDir}/{image.Path}");

            return new ExecImageBase64(long.Parse(image.Id), null, image.Path);
        }
        
        public DbModels.Execimage ToModel()
        {
            return new DbModels.Execimage()
            {
                Id = this.ID.ToString(),
                Path  = this.Path
            };
        }

        public string ToJson()
        {
            JsonObject json = new JsonObject();
           
            json.Add(new KeyValuePair<string, JsonNode?>("ID", this.ID));
            json.Add(new KeyValuePair<string, JsonNode?>("Buffer", this.Buffer));

            return json.ToString();
        }
        
        public ExecImageBase64(long id, string buffer, string path = null)
        { 
            this.ID = id;
            this.Buffer = buffer; 
                
            this.Path = (path == null) ?  $"{this.ID}.png" : path;
        }
    }

    public class ExecImageManager
    {
        protected Dictionary<long, ExecImageBase64> ExecImageMap;

        private readonly LCSDBContext DBContext;
            
        public string ImageDir;
        
        public bool UpdateImage(long id, string path)
        {
            try
            { 
                DbModels.Execimage? image = this.DBContext.Execimages.FirstOrDefault(e => long.Parse(e.Id) == id);

                if (image == null)
                    throw new NullReferenceException();

                image.Path = path;
                
                this.DBContext.SaveChanges();
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e);
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
            return true;
        }

        public bool AddExecImage(ExecImageBase64 execImage)
        {
            try
            {
                this.DBContext.Execimages.Add(execImage.ToModel());
                this.DBContext.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
            return true; 
        }

        public bool ExecImageExists(long id)
        {
            return (this.DBContext.Execimages.FirstOrDefault(e => long.Parse(e.Id) == id) != null);
        }

        public ExecImageBase64 GetImageByID(long id)
        {
            if (this.ExecImageMap.ContainsKey(id))
                return this.ExecImageMap[id];

            Execimage? imageModel  = this.DBContext.Execimages.FirstOrDefault(e => long.Parse(e.Id) == id);

            if (imageModel == null)
                throw new Exception($"Image with id {id} not found");

            ExecImageBase64 image = ExecImageBase64.FromModel(imageModel);

            string path = $"{this.ImageDir}/{image.Path}";

            if (!File.Exists(path))
                throw new FileNotFoundException($"Image file {path} not found."); 
            
            this.ExecImageMap.Add((image = ExecImageBase64.LoadFromFile(path)).ID, image);
            
            return image;
        }

        public bool LoadImages()
        {
            try
            {
                foreach (DbModels.Execimage image in this.DBContext.Execimages)
                    this.ExecImageMap.Add(long.Parse(image.Id), ExecImageBase64.FromModel(image));
            }
            catch (Exception e)
            {
                Console. WriteLine(e);
                
                return false;
            }
            
            return true;
        }

        public bool DeleteImage(long id)
        {
            try
            {
                Execimage? image = this.DBContext.Execimages.FirstOrDefault(e => long.Parse(e.Id) == id);

                if (image == null)
                    throw new NullReferenceException();

                this.DBContext.Execimages.Remove(image);
                this.DBContext.SaveChanges();
            }
            catch (NullReferenceException)
            {
                Console.WriteLine($"Image with id {id} not found.");
                throw; 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return true;
        }

        public ExecImageManager(LCSDBContext dbContext, string imageDir = "Images")
        {
            this.ExecImageMap = new Dictionary<long, ExecImageBase64>();
            this.DBContext = dbContext;
            this.ImageDir = imageDir;
                        
            FileIO.AssertDirectory(this.ImageDir);
        }
    }
    
} 
