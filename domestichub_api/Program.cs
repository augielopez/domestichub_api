using domestichub_api.Data;
using domestichub_api.Services;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("SupabaseDb");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Pass configuration to SupabaseHttpClient
var supabaseClient = new SupabaseHttpClient(configuration);
builder.Services.AddSingleton(supabaseClient);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<AppleEmailService>();
builder.Services.AddScoped<SupabaseEmailService>();
builder.Services.AddSingleton<DatabaseService>();

// Add configuration
builder.Services.AddSingleton<IConfiguration>(configuration);

// Register SupabaseHttpClient with configuration
builder.Services.AddSingleton<SupabaseHttpClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();