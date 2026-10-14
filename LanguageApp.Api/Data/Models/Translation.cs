public class Translation {
	public int Id { get; set; }
	public int? FrequencyIndex { get; set; }
	public string? Language1Tag { get; set; }
	public string? Language2Tag { get; set; }
	public string Term { get; set; } = "";
	public string TermMeaning { get; set; } = "";
	public string TermTranslation { get; set; } = "";
	public string[] TermTranslationTags { get; set; } = [];
	public byte[]? TermTranslationAudio { get; set; }
	public bool IsFlagged { get; set; } = false;
	public bool IsSkipped { get; set; } = false;
	
	public ICollection<TranslationLearningInfo> TranslationLearningInfos { get; set; } = [];
}