using Azure.Storage.Blobs;
using UnredactV2.Components;
using UnredactV2.Logic;
using UnredactV2.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register the BlobServiceClient using the connection string from configuration
builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetSection("AzureBlobStorage")["ConnectionString"];
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Azure Blob Storage connection string is not configured properly.");
    }
    return new BlobServiceClient(connectionString);
});

builder.Services.AddScoped<BlobStorageService>();
builder.Services.AddScoped<DataLogic>();

builder.Services.AddSingleton<string>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var containerName = configuration.GetSection("AzureBlobStorage")["ContainerName"];
    if (containerName == null)
    {
        throw new InvalidOperationException("Azure Blob Storage container name is not configured properly.");
    }
    return containerName;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();