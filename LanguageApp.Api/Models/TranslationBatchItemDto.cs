// A translation served as part of a batch, with what the client needs to order it locally
public class TranslationBatchItemDto : TranslationDto {
	public int FrequencyIndex { get; set; }
	public DateTimeOffset? LastViewAt { get; set; }
}
