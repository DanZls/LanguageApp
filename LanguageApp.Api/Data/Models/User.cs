public class User {
	public int Id { get; set; }
	public string Email { get; set; } = "";
	
	public UserSettings? Settings { get; set; }
	public ICollection<TranslationLearningInfo> TranslationLearningInfos { get; set; } = [];
}