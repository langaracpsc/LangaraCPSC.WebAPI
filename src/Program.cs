using System.Runtime.CompilerServices;
using KeyMan;
using OpenDatabase;
using OpenDatabaseAPI;

namespace LangaraCPSC.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            DatabaseConfiguration config;
            
            Services.ExecManagerInstance = new ExecManager(config = DatabaseConfiguration.LoadFromFile("DatabaseConfig.json"));
            Services.ExecProfileManagerInstance = new ExecProfileManager(config);
            Services.ExecImageManagerInstance = new ExecImageManager(config);
            Services.APIKeyManagerInstance = new APIKeyManager(new PostGRESDatabase(config));

            Services.APIKeyManagerInstance.LoadKeys();
            //
            // Services.APIKeyManagerInstance.AddAPIKey(new APIKeyBuilder().SetUserID("100401242")
            //     .SetKeyValidityTime(new KeyValidityTime(DateTime.Now))
            //     .SetIsLimitless(true)
            //     .AddPermission("ExecCreate", true)
            //     .AddPermission("ExecDelete", true)
            //     .AddPermission("ExecUpdate", true).GenerateKey());

            app.Run();
        }
     }
} 
