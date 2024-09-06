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
    return new BlobServiceClient(connectionString);
});

builder.Services.AddScoped<BlobStorageService>();
builder.Services.AddScoped<DataLogic>();

// Register the container name from configuration
builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    return configuration.GetSection("AzureBlobStorage")["ContainerName"];
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