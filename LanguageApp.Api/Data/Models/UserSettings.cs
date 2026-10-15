public class UserSettings {
	// Shared primary key: one settings row per user
	public int UserId { get; set; }
	public User User { get; set; } = null!;

	public bool AutoPlayAudio { get; set; } = false;
	public double AudioLoudness { get; set; } = 0.8;
	public string Language1Tag { get; set; } = "ru";
	public string Language2Tag { get; set; } = "de";
	public string Theme { get; set; } = "light";
}
