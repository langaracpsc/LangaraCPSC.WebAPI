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
    /// <summary>
    /// Represents an executive image in base64 format.
    /// </summary>
    public class ExecImageBase64 : IPayload
    {
        /// <summary>
        /// The unique identifier of the executive image.
        /// </summary>
        public readonly long ID;

        /// <summary>
        /// The file path of the executive image.
        /// </summary>
        public readonly string Path;

        /// <summary>
        /// The base64-encoded image data.
        /// </summary>
        public string Buffer { get; private set; }

        /// <summary>
        /// Saves the image data to a file.
        /// </summary>
        /// <param name="file">The file path to save the image to.</param>
        /// <returns>True if the image was saved successfully, false otherwise.</returns>
        public bool SaveToFile(string file)
        {
            return FileIO.WriteBytesToFile(Convert.FromBase64String(this.Buffer), file);
        }

        /// <summary>
        /// Saves the image data to the file specified by the <see cref="Path"/> property.
        /// </summary>
        /// <returns>True if the image was saved successfully, false otherwise.</returns>
        public bool SaveToFile()
        {
            return FileIO.WriteBytesToFile(Convert.FromBase64String(this.Buffer), this.Path);
        }

        /// <summary>
        /// Loads an <see cref="ExecImageBase64"/> instance from a file.
        /// </summary>
        /// <param name="file">The file path to load the image from.</param>
        /// <returns>A new instance of the <see cref="ExecImageBase64"/> class.</returns>
        public static ExecImageBase64 LoadFromFile(string file)
        {
            string[] split = file.Split('/');

            string front = split[^1].Split('.')[0];

            int parsed = 0;
            int.TryParse((split[^1].Split('.'))[0], out parsed);

            return new ExecImageBase64(parsed, Convert.ToBase64String(FileIO.ReadBytesFromFile(file)), file);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ExecImageBase64"/> class from a database model.
        /// </summary>
        /// <param name="image">The database model to create the image from.</param>
        /// <param name="imageDir">The directory where the image files are stored.</param>
        /// <returns>A new instance of the <see cref="ExecImageBase64"/> class.</returns>
        public static ExecImageBase64 FromModel(DbModels.Execimage image, string? imageDir = null)
        {
            if (imageDir != null)
                return ExecImageBase64.LoadFromFile($"{imageDir}/{image.Path}");

            return new ExecImageBase64(long.Parse(image.Id), null, image.Path);
        }

        /// <summary>
        /// Converts the current instance of the <see cref="ExecImageBase64"/> class to a database model.
        /// </summary>
        /// <returns>A new instance of the <see cref="DbModels.Execimage"/> class.</returns>
        public DbModels.Execimage ToModel()
        {
            return new DbModels.Execimage()
            {
                Id = this.ID.ToString(),
                Path = this.Path
            };
        }

        /// <summary>
        /// Converts the current instance of the <see cref="ExecImageBase64"/> class to a JSON string.
        /// </summary>
        /// <returns>A JSON string representation of the <see cref="ExecImageBase64"/> instance.</returns>
        public string ToJson()
        {
            JsonObject json = new JsonObject();

            json.Add(new KeyValuePair<string, JsonNode?>("ID", this.ID));
            json.Add(new KeyValuePair<string, JsonNode?>("Buffer", this.Buffer));

            return json.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecImageBase64"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the executive image.</param>
        /// <param name="buffer">The base64-encoded image data.</param>
        /// <param name="path">The file path of the executive image.</param>
        public ExecImageBase64(long id, string buffer, string path = null)
        {
            this.ID = id;
            this.Buffer = buffer;

            this.Path = (path == null) ? $"{this.ID}.png" : path;
        }
    }

    /// <summary>
    /// Defines the interface for managing executive images.
    /// </summary>
    public interface IExecImageManager
    {
        /// <summary>
        /// Updates the file path of an existing executive image.
        /// </summary>
        /// <param name="id">The unique identifier of the executive image.</param>
        /// <param name="path">The new file path of the executive image.</param>
        /// <returns>True if the update was successful, false otherwise.</returns>
        bool UpdateImage(long id, string path);

        /// <summary>
        /// Adds a new executive image.
        /// </summary>
        /// <param name="execImage">The executive image to add.</param>
        /// <returns>True if the image was added successfully, false otherwise.</returns>
        bool AddExecImage(ExecImageBase64 execImage);

        /// <summary>
        /// Checks if an executive image with the specified ID exists.
        /// </summary>
        /// <param name="id">The unique identifier of the executive image.</param>
        /// <returns>True if the image exists, false otherwise.</returns>
        bool ExecImageExists(long id);

        /// <summary>
        /// Gets an executive image by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the executive image.</param>
        /// <returns>The executive image with the specified identifier.</returns>
        ExecImageBase64 GetImageByID(long id);

        /// <summary>
        /// Loads all executive images from the database.
        /// </summary>
        /// <returns>True if the images were loaded successfully, false otherwise.</returns>
        bool LoadImages();

        /// <summary>
        /// Deletes an executive image by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the executive image.</param>
        /// <returns>True if the image was deleted successfully, false otherwise.</returns>
        bool DeleteImage(long id);
    }

    /// <summary>
    /// Implements the <see cref="IExecImageManager"/> interface.
    /// </summary>
    public class ExecImageManager : IExecImageManager
    {
        /// <summary>
        /// A dictionary that maps executive image IDs to their corresponding <see cref="ExecImageBase64"/> instances.
        /// </summary>
        protected Dictionary<long, ExecImageBase64> ExecImageMap;

        /// <summary>
        /// The database context used by the <see cref="ExecImageManager"/>.
        /// </summary>
        private readonly LCSDBContext DBContext;

        /// <summary>
        /// The directory where the executive image files are stored.
        /// </summary>
        public string ImageDir;

        /// <summary>
        /// Updates the file path of an existing executive image.
        /// </summary>
        /// <param name="id">The unique identifier of the executive image.</param>
        /// <param name="path">The new file path of the executive image.</param>
        /// <returns>True if the update was successful, false otherwise.</returns>
        public bool UpdateImage(long id, string path)
        {
            try
            {
                DbModels.Execimage? image = this.DBContext.Execimages.FirstOrDefault(e => e.Id == id.ToString());

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

        /// <summary>
        /// Adds a new executive image.
        /// </summary>
        /// <param name="execImage">The executive image to add.</param>
        /// <returns>True if the image was added successfully, false otherwise.</returns>
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

        /// <summary>
        /// Checks if an executive image with the specified ID exists.
        /// </summary>
        /// <param name="id">The unique identifier of the executive image.</param>
        /// <returns>True if the image exists, false otherwise.</returns>
        public bool ExecImageExists(long id)
        {
            return (this.DBContext.Execimages.FirstOrDefault(e => e.Id == id.ToString()) != null);
        }

        /// <summary>
        /// Gets an executive image by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the executive image.</param>
        /// <returns>The executive image with the specified identifier.</returns>
        public ExecImageBase64 GetImageByID(long id)
        {
            if (this.ExecImageMap.ContainsKey(id))
                return this.ExecImageMap[id];

            Execimage? imageModel = this.DBContext.Execimages.FirstOrDefault(e => e.Id == id.ToString());

            if (imageModel == null)
                throw new Exception($"Image with id {id} not found");

            ExecImageBase64 image = ExecImageBase64.FromModel(imageModel);

            string path = $"{this.ImageDir}/{image.Path}";

            if (!File.Exists(path))
                throw new FileNotFoundException($"Image file {path} not found.");

            this.ExecImageMap.Add((image = ExecImageBase64.LoadFromFile(path)).ID, image);

            return image;
        }

        /// <summary>
        /// Loads all executive images from the database.
        /// </summary>
        /// <returns>True if the images were loaded successfully, false otherwise.</returns>
        public bool LoadImages()
        {
            try
            {
                foreach (DbModels.Execimage image in this.DBContext.Execimages)
                    this.ExecImageMap.Add(long.Parse(image.Id), ExecImageBase64.FromModel(image));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                return false;
            }

            return true;
        }
        /// <summary>
        /// Deletes an executive image by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the executive image.</param>
        /// <returns>True if the image was deleted successfully, false otherwise.</returns>
        public bool DeleteImage(long id)
        {
            try
            {
                Execimage? image = this.DBContext.Execimages.FirstOrDefault(e => e.Id == id.ToString());

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

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecImageManager"/> class.
        /// </summary>
        /// <param name="dbContext">The database context used by the <see cref="ExecImageManager"/>.</param>
        /// <param name="imageDir">The directory where the executive image files are stored.</param>
        public ExecImageManager(LCSDBContext dbContext, string imageDir = "Images")
        {
            this.ExecImageMap = new Dictionary<long, ExecImageBase64>();
            this.DBContext = dbContext;
            this.ImageDir = imageDir;

            FileIO.AssertDirectory(this.ImageDir);
        }
    }
}