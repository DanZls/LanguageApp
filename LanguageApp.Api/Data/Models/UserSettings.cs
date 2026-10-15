public class UserSettings {
	// Shared primary key: one settings row per user
	public int UserId { get; set; }
	public User User { get; set; } = null!;

	public bool AutoPlayAudio { get; set; } = false;
	public double AudioLoudness { get; set; } = 0.8;
	public string Language1Tag { get; set; } = "ru";
	public string Language2Tag { get; set; } = "de";
	public int VocabularyBatchSize { get; set; } = DefaultVocabularyBatchSize;
	public const int DefaultVocabularyBatchSize = 50;
	public const int MinVocabularyBatchSize = 5;
	public const int MaxVocabularyBatchSize = 200;
	public string Theme { get; set; } = "light";
}
