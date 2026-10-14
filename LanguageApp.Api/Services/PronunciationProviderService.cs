public class PronunciationProviderService {
	private readonly IConfiguration _config;
	private AzureTextToSpeechService _azureTextToSpeechService;

	public PronunciationProviderService(
		IConfiguration config,
		AzureTextToSpeechService azureTextToSpeechService
	) {
		_config = config;
		_azureTextToSpeechService = azureTextToSpeechService;
	}

	public async Task SaveGermanPronunciations() {
		string dictionaryPath = $"Resources/Processed/DictionaryRuDeFrequencyPages";
		string outputDirectory = "Resources/Audio/German";
		string languageTag = "de-DE";
		string voiceModelTag = "de-DE-ConradNeural"; // de-DE-KatjaNeural de-DE-ConradNeural de-DE-BerndNeural

		var dictionaryTranslationTerms = DictionaryParserService.ParseGeneratedDictionary(dictionaryPath, 0, 199)
			.ToArray();

		for(uint i = 0; i < dictionaryTranslationTerms.Length; i++) {
			var translation = dictionaryTranslationTerms[i];
			if(translation.Id < 1001 || translation.Id > 5000)
				continue;
			var audio = await _azureTextToSpeechService.GetAudioAsync(
				translation.TermTranslation,
				languageTag,
				voiceModelTag
			);
			await File.WriteAllBytesAsync(
				path: $"{outputDirectory}/{translation.Id}--{translation.TermTranslation}.mp3",
				bytes: audio
			);
		}
	}
}