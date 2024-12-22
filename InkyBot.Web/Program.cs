using InkyBot.Data.Extensions;
using InkyBot.EInk.Configuration;
using InkyBot.EInk.Extensions;
using InkyBot.Telegram.Configuration;
using InkyBot.Telegram.Extensions;
using NeoSmart.Caching.Sqlite;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<TelegramConfiguration>(builder.Configuration.GetSection("Telegram"));
builder.Services.Configure<OpenEPaperConfiguration>(builder.Configuration.GetSection("OpenEPaper"));

builder.Services.AddControllers();
builder.Services.AddRazorPages();

builder.Services.AddInkyBotData(builder.Configuration.GetConnectionString("DefaultConnection")!);
builder.Services.AddInkyBotTelegram();
builder.Services.AddInkyBotEInk();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();