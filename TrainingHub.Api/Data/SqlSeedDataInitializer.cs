using Microsoft.EntityFrameworkCore;

namespace TrainingHub.Data
{
    public static class SqlSeedDataInitializer
    {
        public static async Task SeedAsync(TrainingHubDbContext dbContext)
        {
            var assembly = typeof(SqlSeedDataInitializer).Assembly;
            var resourceName = assembly.GetManifestResourceNames()
                .Single(name => name.EndsWith(".Database.SeedData.sql", StringComparison.Ordinal));

            await using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException("Unable to load the SeedData.sql resource.");
            using var reader = new StreamReader(stream);
            var sql = await reader.ReadToEndAsync();

            await dbContext.Database.ExecuteSqlRawAsync(sql);
        }
    }
}
