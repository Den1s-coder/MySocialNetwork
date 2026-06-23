using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using SocialNetwork.API.Extensions;
using SocialNetwork.API.Hubs;
using SocialNetwork.API.Middleware;
using SocialNetwork.API.Services;
using SocialNetwork.Application;
using SocialNetwork.Application.Interfaces;
using SocialNetwork.Application.Mappings;
using SocialNetwork.Domain.Enums;
using SocialNetwork.Domain.Interfaces;
using SocialNetwork.Infrastructure;
using SocialNetwork.Infrastructure.Services;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSignalR();
builder.Services.AddControllers();

builder.Services.AddAutoMapper(typeof(CommentProfile),
    typeof(PostProfile),
    typeof(UserProfile),
    typeof(ChatProfile),
    typeof(MessageProfile));

builder.Services.AddMediator();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddApplication();

builder.Services.AddScoped<INotificationPublisher, NotificationPublisher>();

var storageConnection = builder.Configuration.GetValue<string>("AzureStorage:ConnectionString");
var containerConfig = builder.Configuration.GetSection("AzureStorage:Containers");

var containers = new Dictionary<ContainerType, string>
{
    { ContainerType.Avatars, containerConfig.GetValue<string>("Avatars") },
    { ContainerType.PostPhotos, containerConfig.GetValue<string>("PostPhotos") },
    { ContainerType.CommentPhotos, containerConfig.GetValue<string>("CommentPhotos") },
    { ContainerType.MessagePhotos, containerConfig.GetValue<string>("MessagePhotos") }
};

builder.Services.AddSingleton(sp => new BlobServiceClient(storageConnection));
builder.Services.AddScoped<ICloudStorageService>(sp =>
    new AzureBlobStorageService(sp.GetRequiredService<BlobServiceClient>(), containers, storageConnection));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.WithOrigins("http://localhost:5173", "http://20.2.91.81")
               .AllowCredentials()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddAuth();

builder.Services.AddDbContext<SocialDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerAuth();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/chatHub");
app.MapHub<NotificationHub>("/notificationHub");

app.Run();
