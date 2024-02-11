using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
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
            string[] split = file.Split('/'), split1;

            return new ExecImageBase64(int.Parse((split1 = split[split.Length - 1].Split('.'))[0]), Convert.ToBase64String(FileIO.ReadBytesFromFile(file)));
        }

        public string ToJson()
        {
            Hashtable imageMap = new Hashtable();

            imageMap.Add("ID", this.ID);
            imageMap.Add("Buffer", this.Buffer);
            

            return JsonConvert.SerializeObject(imageMap);
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
        
        public string ImageDir;
        

        public bool UpdateImage(long id, string path)
        {
            return false;
        }

        public bool AddExecImage(ExecImageBase64 execImage)
        {
            return false;
        }

        public bool ExecImageExists(long id)
        {
            return false;
        }

        public ExecImageBase64 GetImageByID(long id)
        {
            if (this.ExecImageMap.ContainsKey(id))
                return this.ExecImageMap[id];

            ExecImageBase64 image = null;

            string path = null; // $"{this.ImageDir}/{records[0].Values[1].ToString()}";

            if (!File.Exists(path))
                return null; 
            
            this.ExecImageMap.Add((image = ExecImageBase64.LoadFromFile(path)).ID, image);
            
            return image;
        }

        public bool LoadImages()
        {

            ExecImageBase64 image;

            try
            {
                // for (int x = 0; x < imageRecords.Length; x++)
                //    this.ExecImageMap.Add((image = ExecImageBase64.FromRecord(imageRecords[x])).ID, image); 
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
            return false;
        }

        public ExecImageManager(string tableName = "ExecImages", string imageDir = "Images")
        {
            this.ExecImageMap = new Dictionary<long, ExecImageBase64>();
            this.ImageDir = imageDir;
                        
            FileIO.AssertDirectory(this.ImageDir);
        }
    }
    
} 
