public class TranslationUpdateDto {
	public int TranslationId { get; set; }
	public int TranslationLearningInfoId { get; set; }
	public bool? IsTranslationFlagged { get; set; }
	public bool? IsTranslationSkipped { get; set; }
	public bool? IsViewed { get; set; }
	public bool? IsLearned { get; set; }
}