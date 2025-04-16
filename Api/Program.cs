using Application.Di;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", corsPolicyBuilder =>
    {
        corsPolicyBuilder.AllowAnyOrigin()  // Allow any origin
            .AllowAnyMethod()  // Allow any HTTP method (GET, POST, etc.)
            .AllowAnyHeader(); // Allow any header
    });
});

// Add services to the container.
builder.Services.ConfigureDi();
builder.Services.AddAuthenticationDependency(builder.Configuration);
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

    // Add JWT Bearer authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter 'Bearer' followed by your token.",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
var app = builder.Build();

app.UseCors("AllowAllOrigins");
// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(x =>
{
    x.SwaggerEndpoint("/swagger/v1/swagger.json", "teste");
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
