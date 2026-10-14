public class TranslationDto {
	public int TranslationId { get; set; }
	public int TranslationLearningInfoId { get; set; }
	public string? Term { get; set; }
	public string? TermMeaning { get; set; }
	public string? TermTranslation { get; set; }
	public string[]? TermTranslationTags { get; set; }
	public byte[]? TermTranslationAudio { get; set; }
	public bool? IsTranslationFlagged { get; set; }
	public bool? IsTranslationSkipped { get; set; }
}