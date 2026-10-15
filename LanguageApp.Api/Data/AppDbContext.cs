using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext {
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

	public DbSet<User> Users { get; set; }
	public DbSet<TranslationLearningInfo> TranslationLearningInfoTable { get; set; }
	public DbSet<Translation> Translations { get; set; }
	public DbSet<UserSettings> UserSettingsTable { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder) {
		modelBuilder.Entity<User>()
			.HasIndex(user => user.Email)
			.IsUnique();

		modelBuilder.Entity<UserSettings>(settings => {
			settings.ToTable("UserSettings");
			settings.HasKey(s => s.UserId);
			settings.HasOne(s => s.User)
				.WithOne(user => user.Settings)
				.HasForeignKey<UserSettings>(s => s.UserId)
				.OnDelete(DeleteBehavior.Cascade);
			settings.Property(s => s.Language1Tag).HasMaxLength(10);
			settings.Property(s => s.Language2Tag).HasMaxLength(10);
			settings.Property(s => s.Theme).HasMaxLength(10);
			settings.Property(s => s.VocabularyBatchSize).HasDefaultValue(UserSettings.DefaultVocabularyBatchSize);
		});

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
