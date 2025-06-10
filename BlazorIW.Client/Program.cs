using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorIW.Client.Pages;


var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

// Register Counter component as custom element <my-counter>
builder.RootComponents.RegisterCustomElement<Counter>("my-counter");

await builder.Build().RunAsync();
