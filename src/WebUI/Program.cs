var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.WebHost.UseKestrel(options => options.AddServerHeader = false);
builder.Services.AddSession();
builder.Services.AddDistributedMemoryCache();
///Fluent Validation  configure
var assm = Assembly.GetExecutingAssembly();
builder.Services.AddValidatorsFromAssembly(assm);


///////////////set language
builder.Services.AddControllersWithViews().AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
//////////////

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.Configure<Settings>(builder.Configuration.GetSection("Settings"));
builder.Services.AddTransient<Settings>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<SmsCompanyFactory>();

///serilog implementation
var logger = new LoggerConfiguration()
  .ReadFrom.Configuration(builder.Configuration)
  .Enrich.FromLogContext()
  .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

var app = builder.Build();

///////SET supported cultures Language

string[] filePaths = Directory.GetFiles("Resources", "*.json");
List<string> languages = new List<string>();
foreach (string file in filePaths)
{
    using var str = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
    using var sReader = new StreamReader(str);
    using var reader = new JsonTextReader(sReader);
    while (reader.Read())
    {
        if (reader.TokenType == JsonToken.PropertyName && reader.Value as string == "languageDescription")
        {
            reader.Read();
            string shortlanguageName = file.Split(new[] { "Resources\\" }, StringSplitOptions.None)[1].Split(new[] { ".json" }, StringSplitOptions.None)[0];
            languages.Add(shortlanguageName);
        }
    }
}
List<CultureInfo> supportedCultures = new List<CultureInfo>();
foreach (var item in languages)
{
    supportedCultures.Add(new CultureInfo(item));
}

var options = new RequestLocalizationOptions
{
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};
app.UseRequestLocalization(options);
////////////////////

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseSession();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute("Default1", "", new { Controller = "Home", Action = "ResetPassword" });
app.MapControllerRoute("Default2", "resetsms", new { Controller = "Home", Action = "ResetSms" });
app.MapControllerRoute("Default3", "unlock", new { Controller = "Home", Action = "Unlock" });
app.MapControllerRoute("Default4", "sms", new { Controller = "Home", Action = "UnlockSms" });
app.MapControllerRoute("Default5", "generatecaptcha", new { Controller = "Home", Action = "GenerateCaptcha" });
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=ResetPassword}/{id?}");

app.Run();
