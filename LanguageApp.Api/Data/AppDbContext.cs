using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext {
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

	public DbSet<User> Users { get; set; }
	public DbSet<TranslationLearningInfo> TranslationLearningInfoTable { get; set; }
	public DbSet<Translation> Translations { get; set; }
	public DbSet<UserSettings> UserSettingsTable { get; set; }
	public DbSet<Pronunciation> Pronunciations { get; set; }

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

		// 1:1 with Translation; the audio column is named "Pronunciation" (a property can't share its type's name)
		modelBuilder.Entity<Pronunciation>(pronunciation => {
			pronunciation.HasOne(p => p.Translation)
				.WithOne(translation => translation.Pronunciation)
				.HasForeignKey<Pronunciation>(p => p.TranslationId)
				.OnDelete(DeleteBehavior.Cascade);
			pronunciation.HasIndex(p => p.TranslationId).IsUnique();
			pronunciation.Property(p => p.LanguageTag).HasMaxLength(10);
			pronunciation.Property(p => p.Audio).HasColumnName("Pronunciation");
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
