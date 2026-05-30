using TrainingHub.Reporting.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Setup HttpClient to talk to your Web API
builder.Services.AddHttpClient("TrainingHubApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:7209/");
});

// Setup Cookie Authentication
builder.Services.AddAuthentication("ReportingCookie")
    .AddCookie("ReportingCookie", options =>
    {
        options.Cookie.Name = "TrainingHub.Reporting.Auth";
        options.LoginPath = "/Account/Login"; // Where to send users who aren't logged in
        options.AccessDeniedPath = "/Account/AccessDenied"; // Where to send users with the wrong role
    });

// Allows the ReportService to look inside the HttpContext (the cookies/claims)
builder.Services.AddHttpContextAccessor();

// Registers the ReportService so we can inject it into our Controllers
builder.Services.AddScoped<ReportService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
