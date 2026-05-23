using Microsoft.EntityFrameworkCore;
using TrainingHub.Data;
using TrainingHub.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<TrainingHubDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TrainingHubDbContext>();
    dbContext.Database.Migrate();

    var targetIssuedAt = new DateTime(2026, 8, 15);
    var targetResultRecordedAt = new DateTime(2026, 8, 11);

    var certificate = await dbContext.Certificates.FirstOrDefaultAsync(c => c.Id == 1);
    if (certificate != null && certificate.IssuedAt != targetIssuedAt)
    {
        certificate.IssuedAt = targetIssuedAt;
        certificate.Status = "Issued";
    }

    var extraEnrollmentExists = await dbContext.Enrollments.AnyAsync(e => e.Id == 3);
    if (!extraEnrollmentExists)
    {
        dbContext.Enrollments.Add(new Enrollment
        {
            Id = 3,
            TraineeId = 1,
            CourseSessionId = 3,
            Status = "Completed",
            EnrolledAt = new DateTime(2026, 7, 5),
            AttendanceStatus = "Present",
            ResultStatus = "Pass",
            ResultRecordedAt = targetResultRecordedAt
        });
    }

    if (dbContext.ChangeTracker.HasChanges())
    {
        await dbContext.SaveChangesAsync();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();