using System.Collections;
using KeyMan;
using LangaraCPSC.WebAPI;
using LangaraCPSC.WebAPI.DbModels;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<LCSDBContext>();
builder.Services.AddDbContext<APIKeyDBContext>();

builder.Services.AddScoped<IExecManager, ExecManager>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CORS", policy =>
    {
        policy.AllowAnyOrigin();
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("CORS");

app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "Images")),
    RequestPath  = "/Images",
    ServeUnknownFileTypes  = true
});

app.UseHttpsRedirection();

Console.WriteLine(Path.Combine(builder.Environment.ContentRootPath, "Images"));

app.UseAuthorization();
app.MapControllers(); 

IDictionary environmentVariables = Environment.GetEnvironmentVariables();


string[] configKeys = new string[] {
    "CAL_ID", "HOSTNAME", "DATABASE", "USERNAME", "PASSWORD", "PORT" 
};

foreach (string key in configKeys)
{   
    try
    {
        environmentVariables[key]?.ToString();
    }
    catch (NullReferenceException e)
    {
        Console.Error.WriteLine($"Required environment variable {key} is not set.");

        return;
    } 
}


APIKeyDBContext dbContext = new APIKeyDBContext();

// Services.ExecImageManagerInstance = new ExecImageManager(config);
// Services.ExecProfileManagerInstance = new ExecProfileManager(config, "ExecProfiles", "Execs", Services.ExecImageManagerInstance);
Services.APIKeyManagerInstance = new APIKeyManager(dbContext);
Services.EventManagerInstance = new EventManager(environmentVariables["CAL_ID"].ToString());

Services.APIKeyManagerInstance.LoadKeys();

// Services.APIKeyManagerInstance.AddAPIKey(new APIKeyBuilder().SetUserID("100401242")
//     .SetKeyValidityTime(new KeyValidityTime(DateTime.Now))
//     .SetIsLimitless(true)
//     .AddPermission("ExecCreate", true)
//     .AddPermission("ExecRead", true)
//     .AddPermission("ExecDelete", true)
//     .AddPermission("ExecUpdate", true)
//     .AddPermission("ExecEnd", true)
//     .GenerateKey());
//
// Services.APIKeyManagerInstance.AddAPIKey(new APIKeyBuilder().SetUserID("100401242")
//     .SetKeyValidityTime(new KeyValidityTime(DateTime.Now))
//     .SetIsLimitless(true)
//     .AddPermission("ExecCreate", true)
//     .AddPermission("ExecRead", true)
//     .AddPermission("ExecUpdate", true)
//     .
// GenerateKey());
//
app.Run();
