using Microsoft.EntityFrameworkCore;

public class AzureSqlDatabaseService(AppDbContext db)
{
	private readonly AppDbContext _db = db;


  public async Task<TranslationDto> GetNextTranslation(
		string userEmail,
		string language1Tag,
		string language2Tag
	) {
		int hoursAfterFirstLearn = 3*24 - 6;
		int hoursAfterSecondLearn = 7*24 - 6;
		int batchMaxSize = 50;

		DateTimeOffset now = DateTimeOffset.Now;
		// Cutoffs are computed here so the predicates compare bare columns to parameters (sargable)
		DateTimeOffset firstLearnCutoff = now.AddHours(-hoursAfterFirstLearn);
		DateTimeOffset secondLearnCutoff = now.AddHours(-hoursAfterSecondLearn);

		// Lightweight projection (no audio blob); the batch is the 50 lowest-frequency due translations
		var batch = await _db.TranslationLearningInfoTable
			.AsNoTracking()
			.Where(tlInfo => 
				(tlInfo.FirstLearnAt == null)
				|| (tlInfo.SecondLearnAt == null && tlInfo.FirstLearnAt != null && tlInfo.FirstLearnAt <= firstLearnCutoff)
				|| (tlInfo.ThirdLearnAt == null && tlInfo.SecondLearnAt != null && tlInfo.SecondLearnAt <= secondLearnCutoff)
			)
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

		// Same selection as before, done in memory on the already-limited batch
		int firstBucket = (int)Math.Floor((double)batch.First().FrequencyIndex / batchMaxSize);
		var next = batch
			.Where(t => (int)Math.Floor((double)t.FrequencyIndex / batchMaxSize) == firstBucket)
			.OrderBy(t => t.LastViewAt)
			.ThenBy(t => t.FrequencyIndex)
			.First();

		return await _db.TranslationLearningInfoTable
			.AsNoTracking()
			.Where(tlInfo => tlInfo.Id == next.Id)
			.Select(tlInfo => new TranslationDto {
				TranslationId = tlInfo.TranslationId,
				TranslationLearningInfoId = tlInfo.Id,
				Term = tlInfo.Translation.Term,
				TermMeaning = tlInfo.Translation.TermMeaning,
				TermTranslation = tlInfo.Translation.TermTranslation,
				TermTranslationTags = tlInfo.Translation.TermTranslationTags,
				TermTranslationAudio = tlInfo.Translation.TermTranslationAudio,
				IsTranslationFlagged = tlInfo.Translation.IsFlagged,
				IsTranslationSkipped = tlInfo.Translation.IsSkipped,
			})
			.FirstAsync();
	}


	public async Task UpdateTranslation(
		TranslationUpdateDto translationUpdate
	) {
		DateTimeOffset now = DateTimeOffset.Now;

		TranslationLearningInfo translationLearningInfo = await _db.TranslationLearningInfoTable
			.Where(tlInfo => tlInfo.Id == translationUpdate.TranslationLearningInfoId)
			.Where(tlInfo => tlInfo.TranslationId == translationUpdate.TranslationId)
			.FirstAsync();

		// Stub entity so only the changed flag columns are written and the audio blob is never loaded
		Translation translationStub = new() { Id = translationUpdate.TranslationId };
		_db.Translations.Attach(translationStub);

		if (translationUpdate.IsViewed != null) {
			translationLearningInfo.LastViewAt = now;
		}
		if (translationUpdate.IsLearned != null && translationUpdate.IsLearned == true) {
			if (translationLearningInfo.FirstLearnAt == null)
				translationLearningInfo.FirstLearnAt = now;
			else if (translationLearningInfo.SecondLearnAt == null)
				translationLearningInfo.SecondLearnAt = now;
			else if (translationLearningInfo.ThirdLearnAt == null)
				translationLearningInfo.ThirdLearnAt = now;
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
			translation.TermTranslationAudio = audioBytes;
			// _db.Dictionaries.Add(translation);
		}
		await _db.SaveChangesAsync();
	}

	private async Task<int> GetUserId (string userEmail) {
		int userId = await _db.Users
			.AsNoTracking()
			.Where(user => user.Email == userEmail)
			.Select(user => user.Id)
			.FirstAsync();
		return userId;
	}
}