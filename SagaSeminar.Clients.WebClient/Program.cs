using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SagaSeminar.Clients.WebClient;
using SagaSeminar.Shared.Extensions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddAntDesign();

builder.Services.ConfigureApiInfo(builder.Configuration);

builder.Services.AddServiceClients()
    .AddLogClient("black", nameof(SagaSeminar.Clients.WebClient));

await builder.Build().RunAsync();
