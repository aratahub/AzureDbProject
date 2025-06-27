using Entities;
using Data;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebApi.Services;
using Core.Services.DbOrders;
using Microsoft.OpenApi.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

var builder = WebApplication.CreateBuilder(args);

//************************
//*** Add the services ***
//************************

if (builder.Configuration["UseCosmos"] == "TRUE")
{
    //Azure Cosmos DB: Dependency Injection setup
    builder.Services.AddSingleton(s =>
    {
        var config = builder.Configuration.GetSection("CosmosDb");
        var client = new CosmosClient(config["Account"], config["Key"]);
        return client;
    });

    //Use the cosmos service
    builder.Services.AddSingleton<ISqlOrderService, CosmosOrderService>();
}
else
{
    if (builder.Configuration["UseInMemoryDB"] == "TRUE")
    {
        // Register the In-Memory DB 
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("InMemoryOrderDb"));
    }
    else
    {
        // *** Register the Azure Sqlserver DB ***  
        string connStr = string.Empty;

        try
        {
            var secretName = Environment.GetEnvironmentVariable("KeyVaultSecretName");
            var keyVaultUri = Environment.GetEnvironmentVariable("KeyVaultUri");

            if (string.IsNullOrEmpty(secretName) || string.IsNullOrEmpty(keyVaultUri))
                throw new Exception("Missing Key Vault config values.");

            var client = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
            KeyVaultSecret secret = client.GetSecret(secretName);

            if (string.IsNullOrEmpty(secret?.Value))
                throw new Exception($"Secret '{secretName}' was not found or has no value.");

            connStr = secret.Value;

            Console.WriteLine("Connection string retrieved from Key Vault.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to load from Azure Key Vault: " + ex.Message);
        }
        
        if (string.IsNullOrEmpty(connStr))
        {
            throw new Exception("No valid connection string found from Key Vault or environment variable.");
        }

        Console.WriteLine($"Connection string in use: {connStr}");

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connStr));

    }

    //Use the sql service for Sql server and In-Memory DBs
    builder.Services.AddScoped<ISqlOrderService, SqlOrderService>();
}


builder.Services.AddControllers()
    .AddNewtonsoftJson(); // Optional but helps if you're using Newtonsoft

builder.Services.AddEndpointsApiExplorer();

//Authentication setup
builder.Services.AddScoped<TokenService>();

var jwtKey = builder.Configuration["Jwt:Key"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };


    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("Token validated successfully.");
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            Console.WriteLine("JWT Token Received");
            Console.WriteLine($"Token: {context.Token}");
            return Task.CompletedTask;
        }
    };

});

builder.Services.AddAuthorization();


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Azure Containerized API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});


//Add a Health Check or Status Endpoint
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    // Middleware
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseRouting();

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // Enables routing to controller endpoints

app.MapHealthChecks("/health");


// Test the connection to the Sql Server DB
if ((builder.Configuration["UseInMemoryDB"] == "FALSE") && (builder.Configuration["UseCosmos"] == "FALSE"))
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            dbContext.Database.OpenConnection();
            dbContext.Database.CloseConnection();
            Console.WriteLine("Successfully connected to SQL Server database.");
        }
        catch (Exception ex)
        {
            var logFilePath = Path.Combine(AppContext.BaseDirectory, "startup-error.log");
            var errorMessage = $"[{DateTime.Now}] Database connection failed: {ex.Message}{Environment.NewLine}";

            File.AppendAllText(logFilePath, errorMessage);

            Console.WriteLine("Database connection failed. See 'startup-error.log' for details.");
            throw; // Causes app startup to fail
        }
    }
}

//Test creating cosmos records when in cosmos mode
if (builder.Configuration["UseCosmos"] == "TRUE")
{
    var cosmosService = new CosmosDbService(builder.Configuration["CosmosDb:Account"],
                                        builder.Configuration["CosmosDb:Key"],
                                        builder.Configuration["CosmosDb:DatabaseName"],
                                        builder.Configuration["CosmosDb:ContainerName"]);

    var order = new DbOrder
    {
        ProductName = "Product 3",
        Quantity = 12,
        UnitPrice = 30.50m
    };

    await cosmosService.AddItemAsync(order);
}
else
{
    //Test creating sql server records when in sql server or in-memory mode
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.DbOrder.AddRange(
            new DbOrder { ProductName = "School Bag", Quantity = 1, UnitPrice = 10.99m },
            new DbOrder { ProductName = "Shoes", Quantity = 2, UnitPrice = 35.50m }
        );
        context.SaveChanges();
    }
}


app.Run();
