record ParsePageRequest {
	public string Prompt {get; set;} = "";
	public string DocumentPage {get; set;} = "";

	public string ToJson() {
		var options = new System.Text.Json.JsonSerializerOptions
		{ 
			WriteIndented = true,
			Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};
		return System.Text.Json.JsonSerializer.Serialize(this, options);
	}
}