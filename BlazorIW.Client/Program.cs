using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorIW.Client.Pages;
using BlazorIW.Client.Services;


var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<FileService>();
builder.Services.AddScoped<BrowserStorageService>();
builder.Services.AddScoped<WordPressService>();
builder.Services.AddScoped<HtmlContentService>();
builder.Services.AddScoped<LocalizationService>();

// Register Counter component as custom element <my-counter>
builder.RootComponents.RegisterCustomElement<Counter>("my-counter");

await builder.Build().RunAsync();
