record AiRequest {
	public string Prompt {get; set;} = "";
	public List<TextFile> TextFiles {get; set;} = [];

	public string ToJson() {
		return System.Text.Json.JsonSerializer.Serialize(
			this, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
		);
	}
}


record TextFile {
	public string Name {get; set;} = "";
	public string Content {get; set;} = "";
}