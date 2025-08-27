using Microsoft.Extensions.Options;
using Zanda.LLM;
using Zanda.LLM.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("Settings"));

// Add services to the container.
builder.Services.AddScoped<IOpenAiClient>(serviceProvider =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value;
    return new OpenAiClient(settings);
});

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

app.Run();