public record UserSettingsDto(
	bool AutoPlayAudio,
	double AudioLoudness,
	string Language1Tag,
	string Language2Tag,
	int VocabularyBatchSize,
	string Theme
);
