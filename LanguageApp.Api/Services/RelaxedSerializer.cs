static class RelaxedSerializer {
	public static string SerializeToJson(object obj) {
		var options = new System.Text.Json.JsonSerializerOptions
		{ 
			WriteIndented = true,
			Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};
		return System.Text.Json.JsonSerializer.Serialize(obj, options);
	}
}