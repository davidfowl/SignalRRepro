using SignalRRepro;
using SignalRRepro.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddHostedService<MessageSender>();
builder.Services.AddSingleton(typeof(HubConnectionManager<>));
// builder.Services.AddHostedService<Logger>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
// Essential disable buffering for writes in signalr's transport layer
app.MapHub<MyHub>("/myHub");

app.Run();