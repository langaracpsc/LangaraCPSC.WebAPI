using System.Runtime.CompilerServices;
using OpenDatabase;

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
            
            //Services.ExecManagerInstance.CreateExec(new ExecName("David", "Bowie"), ExecPosition.President, new ExecTenure(DateTime.Now, new DateTime()));
                
            app.Run();
        }
    }
}