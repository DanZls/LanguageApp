using Microsoft.EntityFrameworkCore;

public class AzureSqlDatabaseService(AppDbContext db)
{
	private readonly AppDbContext _db = db;
	private static readonly string[] AllowedLanguages = ["ru", "de"];
	private static readonly string[] AllowedThemes = ["light", "dark"];


  // Returns the whole "current bucket" of due translations (see the selection rules below), so the
  // client can walk through it without asking the server for every single word.
  public async Task<List<TranslationBatchItemDto>> GetTranslationBatch(
		string userEmail,
		string language1Tag,
		string language2Tag
	) {
		int successfulAttemptsToLearn = 2;
		int daysAfterLastLearn = 3;
		// Translations learned on day D become available again on day D + 3 at 00:00,
		// so anything learned before the start of (today - (days - 1)) is due.
		DateTimeOffset now = DateTimeOffset.Now;
		DateTimeOffset lastLearnCutoff = new DateTimeOffset(now.Date.AddDays(-(daysAfterLastLearn - 1)), now.Offset);

		int batchMaxSize = await _db.UserSettingsTable
			.AsNoTracking()
			.Where(s => s.User.Email == userEmail)
			.Select(s => s.VocabularyBatchSize)
			.FirstOrDefaultAsync();
		if (batchMaxSize <= 0)
			batchMaxSize = UserSettings.DefaultVocabularyBatchSize;

		// Lightweight projection (no audio blob); the batch is the lowest-frequency due translations
		var batch = await _db.TranslationLearningInfoTable
			.AsNoTracking()
			.Where(tlInfo => tlInfo.LearnMeter < successfulAttemptsToLearn)
			.Where(tlInfo => (tlInfo.LastLearnAt == null) || (tlInfo.LastLearnAt < lastLearnCutoff))
			.Where(tlInfo => tlInfo.User.Email == userEmail)
			.Where(tlInfo => tlInfo.Translation.Language1Tag == language1Tag)
			.Where(tlInfo => tlInfo.Translation.Language2Tag == language2Tag)
			.Where(tlInfo => !tlInfo.Translation.IsSkipped)
			.OrderBy(tlInfo => tlInfo.Translation.FrequencyIndex)
			.Take(batchMaxSize)
			.Select(tlInfo => new {
				tlInfo.Id,
				tlInfo.LastViewAt,
				FrequencyIndex = tlInfo.Translation.FrequencyIndex!.Value,
			})
			.ToListAsync();

		if (batch.Count == 0)
			return [];

		// Only translations from the same frequency bucket as the first one are served together,
		// least recently viewed first (never viewed first), then by frequency
		int firstBucket = (int)Math.Floor((double)batch.First().FrequencyIndex / batchMaxSize);
		var bucketItems = batch
			.Where(t => (int)Math.Floor((double)t.FrequencyIndex / batchMaxSize) == firstBucket)
			.OrderBy(t => t.LastViewAt)
			.ThenBy(t => t.FrequencyIndex)
			.ToList();

		List<int> ids = bucketItems.Select(t => t.Id).ToList();
		Dictionary<int, TranslationBatchItemDto> dtoById = (await _db.TranslationLearningInfoTable
			.AsNoTracking()
			.Where(tlInfo => ids.Contains(tlInfo.Id))
			.Select(tlInfo => new TranslationBatchItemDto {
				TranslationId = tlInfo.TranslationId,
				TranslationLearningInfoId = tlInfo.Id,
				Term = tlInfo.Translation.Term,
				TermMeaning = tlInfo.Translation.TermMeaning,
				TermTranslation = tlInfo.Translation.TermTranslation,
				AlternativeForms = tlInfo.Translation.AlternativeForms,
				TermTranslationTags = tlInfo.Translation.TermTranslationTags,
				TermTranslationAudio = tlInfo.Translation.Pronunciation!.Audio,
				IsTranslationFlagged = tlInfo.Translation.IsFlagged,
				IsTranslationSkipped = tlInfo.Translation.IsSkipped,
			})
			.ToListAsync())
			.ToDictionary(dto => dto.TranslationLearningInfoId);

		List<TranslationBatchItemDto> result = [];
		foreach (var item in bucketItems) {
			// The row can disappear between the two queries (e.g. a concurrent write); skip it then
			if (!dtoById.TryGetValue(item.Id, out TranslationBatchItemDto? dto))
				continue;

			dto.FrequencyIndex = item.FrequencyIndex;
			dto.LastViewAt = item.LastViewAt;
			result.Add(dto);
		}
		return result;
	}


	public async Task UpdateTranslation(
		TranslationUpdateDto translationUpdate
	) {
		TranslationLearningInfo translationLearningInfo = await _db.TranslationLearningInfoTable
			.Where(tlInfo => tlInfo.Id == translationUpdate.TranslationLearningInfoId)
			.Where(tlInfo => tlInfo.TranslationId == translationUpdate.TranslationId)
			.FirstAsync();

		// Stub entity so only the changed flag columns are written and the audio blob is never loaded
		Translation translationStub = new() { Id = translationUpdate.TranslationId };
		_db.Translations.Attach(translationStub);

		if (translationUpdate.IsViewed != null) {
			translationLearningInfo.LastViewAt = DateTimeOffset.Now;
			if (translationUpdate.IsLearned == null || translationUpdate.IsLearned == false)
				translationLearningInfo.LearnMeter = 0;
		}
		if (translationUpdate.IsLearned != null && translationUpdate.IsLearned == true) {
			translationLearningInfo.LastLearnAt = DateTimeOffset.Now;
			translationLearningInfo.LearnMeter += 1;
		}
		if (translationUpdate.IsTranslationFlagged != null) {
			translationStub.IsFlagged = translationUpdate.IsTranslationFlagged.Value;
			_db.Entry(translationStub).Property(t => t.IsFlagged).IsModified = true;
		}
		if (translationUpdate.IsTranslationSkipped != null) {
			translationStub.IsSkipped = translationUpdate.IsTranslationSkipped.Value;
			_db.Entry(translationStub).Property(t => t.IsSkipped).IsModified = true;
		}

		await _db.SaveChangesAsync();
	}


  public async Task AddUser(string userEmail)
	{
		_db.Users.Add(new User {
			Email = userEmail
		});
		await _db.SaveChangesAsync();
	}


  public async Task AddUserTranslationLearningInfo(
		string userEmail,
		string language1Tag,
		string language2Tag
	) { 
		int userId = await GetUserId(userEmail);
		var translationIds = await _db.Translations
			.AsNoTracking()
			.Where(translation => (translation.Language1Tag == language1Tag) && (translation.Language2Tag == language2Tag))
			.Select(translation => translation.Id)
			.ToListAsync();
		DateTimeOffset now = DateTimeOffset.Now;
		_db.TranslationLearningInfoTable.AddRange(translationIds.Select(translationId => new TranslationLearningInfo {
			UserId = userId,
			TranslationId = translationId,
			LastViewAt = now
		}));
		await _db.SaveChangesAsync();
	}


  public async Task AddDictionaryRuDeTranslations()
	{
		string dictionaryPath = $"Resources/Processed/DictionaryRuDeFrequencyPages";
		string audioPath = "Resources/Audio/German";
		var dictionaryTranslationTerms = DictionaryParserService.ParseGeneratedDictionary(dictionaryPath, 1, 99)
			.ToArray();
		foreach (var translation in dictionaryTranslationTerms) {
			var audioBytes = await File.ReadAllBytesAsync($"{audioPath}/{translation.Id}--{translation.TermTranslation}.mp3");
			translation.Pronunciation = new Pronunciation {
					Term = translation.TermTranslation,
					LanguageTag = translation.Language2Tag ?? "",
					Audio = audioBytes
				};
			// _db.Dictionaries.Add(translation);
		}
		await _db.SaveChangesAsync();
	}

	public async Task<UserSettingsDto> GetUserSettings(string userEmail) {
		UserSettings? settings = await _db.UserSettingsTable
			.AsNoTracking()
			.FirstOrDefaultAsync(s => s.User.Email == userEmail);
		// Users without a saved row (or unknown emails) get defaults
		return ToDto(settings ?? new UserSettings());
	}


	public async Task<UserSettingsDto> SaveUserSettings(string userEmail, UserSettingsDto dto) {
		if (!AllowedLanguages.Contains(dto.Language1Tag) || !AllowedLanguages.Contains(dto.Language2Tag))
			throw new ArgumentException("Unsupported language tag.");
		if (!AllowedThemes.Contains(dto.Theme))
			throw new ArgumentException("Unsupported theme.");
		if (dto.VocabularyBatchSize < UserSettings.MinVocabularyBatchSize
			|| dto.VocabularyBatchSize > UserSettings.MaxVocabularyBatchSize)
			throw new ArgumentException(
				$"Batch size must be between {UserSettings.MinVocabularyBatchSize} and {UserSettings.MaxVocabularyBatchSize}.");
		if (double.IsNaN(dto.AudioLoudness))
			throw new ArgumentException("Invalid audio loudness.");

		User user = await _db.Users
			.Include(u => u.Settings)
			.FirstOrDefaultAsync(u => u.Email == userEmail)
			?? _db.Users.Add(new User { Email = userEmail }).Entity;
		user.Settings ??= new UserSettings();
		user.Settings.AutoPlayAudio = dto.AutoPlayAudio;
		user.Settings.AudioLoudness = Math.Clamp(dto.AudioLoudness, 0, 1);
		user.Settings.Language1Tag = dto.Language1Tag;
		user.Settings.Language2Tag = dto.Language2Tag;
		user.Settings.VocabularyBatchSize = dto.VocabularyBatchSize;
		user.Settings.Theme = dto.Theme;
		await _db.SaveChangesAsync();
		return ToDto(user.Settings);
	}


	private static UserSettingsDto ToDto(UserSettings s) =>
		new(s.AutoPlayAudio, s.AudioLoudness, s.Language1Tag, s.Language2Tag, s.VocabularyBatchSize, s.Theme);


	private async Task<int> GetUserId (string userEmail) {
		int userId = await _db.Users
			.AsNoTracking()
			.Where(user => user.Email == userEmail)
			.Select(user => user.Id)
			.FirstAsync();
		return userId;
	}
}