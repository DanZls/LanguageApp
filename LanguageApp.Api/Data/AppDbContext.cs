using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext {
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

	public DbSet<User> Users { get; set; }
	public DbSet<TranslationLearningInfo> TranslationLearningInfoTable { get; set; }
	public DbSet<Translation> Translations { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder) {
		modelBuilder.Entity<User>()
			.HasIndex(user => user.Email)
			.IsUnique();

		// Language pair filter + FrequencyIndex ordering, restricted to non-skipped rows
		modelBuilder.Entity<Translation>()
			.HasIndex(translation => new { translation.Language1Tag, translation.Language2Tag, translation.FrequencyIndex })
			.HasFilter("[IsSkipped] = 0");

		// User lookup + join to Translation, with the date columns included to avoid key lookups
		modelBuilder.Entity<TranslationLearningInfo>()
			.HasIndex(tlInfo => new { tlInfo.UserId, tlInfo.TranslationId })
			.IncludeProperties(tlInfo => new { tlInfo.FirstLearnAt, tlInfo.SecondLearnAt, tlInfo.ThirdLearnAt, tlInfo.LastViewAt });
	}
}
