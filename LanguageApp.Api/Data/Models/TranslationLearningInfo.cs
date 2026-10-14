public class TranslationLearningInfo {
	public int Id { get; set; }
	public int UserId { get; set; } = new();
	public int TranslationId { get; set; } = new();
	public string? DictionaryName { get; set; }
	public DateTimeOffset? LastViewAt { get; set; }
	public DateTimeOffset? FirstLearnAt { get; set; }
	public DateTimeOffset? SecondLearnAt { get; set; }
	public DateTimeOffset? ThirdLearnAt { get; set; }

	public User User { get; set; } = new();
	public Translation Translation { get; set; } = new();
}