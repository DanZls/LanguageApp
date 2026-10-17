public class Pronunciation {
	public int Id { get; set; }
	public int TranslationId { get; set; }
	// Copy of Translation.TermTranslation the audio was generated from
	public string Term { get; set; } = "";
	public string LanguageTag { get; set; } = "";
	public byte[]? Audio { get; set; }

	public Translation Translation { get; set; } = null!;
}
