using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlServer(
		builder.Configuration.GetConnectionString("LanguageAppDatabaseConnectionString"),
		sqlOptions => sqlOptions
			.EnableRetryOnFailure(
				maxRetryCount: 12,
				maxRetryDelay: TimeSpan.FromSeconds(10),
				errorNumbersToAdd: null
			)
	)
);

builder.Services.AddSingleton<PronunciationProviderService>();
builder.Services.AddSingleton<AzureTextToSpeechService>();
builder.Services.AddScoped<AzureSqlDatabaseService>();

builder.Services.AddCors(options =>
{
	options.AddPolicy("FrontendDev", policy =>
	{
		policy
			.WithOrigins(builder.Configuration["FrontendOrigin"]!)
			.AllowAnyHeader()
			.AllowAnyMethod();
			// .AllowCredentials(); // for cookies/auth credentials
	});
});

var app = builder.Build();

app.UseCors("FrontendDev");

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}


app.MapHealthChecks("/health");


app.MapGet("/api/get-next-translation", async (
	AzureSqlDatabaseService dbService,
	string userEmail,
	string language1Tag,
	string language2Tag
) => {
	try	{
		TranslationDto result = await dbService.GetNextTranslation(userEmail, language1Tag, language2Tag);
		return Results.Ok(result);
	}
	catch (Exception ex) {
		return Results.BadRequest(new {error = ExceptionService.GetExceptionDetails(ex)});
	}
});


app.MapPatch("/api/update-translation", async (
	AzureSqlDatabaseService dbService,
	TranslationUpdateDto translationUpdate
) => {
	try	{
		await dbService.UpdateTranslation(translationUpdate);
		return Results.Ok();
	}
	catch (Exception ex) {
		return Results.BadRequest(new {error = ExceptionService.GetExceptionDetails(ex)});
	}
});


app.MapGet("/api/add-user", async (AzureSqlDatabaseService dbService) =>
{
	try	{
		await dbService.AddUser("dan.zloschastiev@gmail.com");
		await dbService.AddUserTranslationLearningInfo("dan.zloschastiev@gmail.com", "ru", "de");
		return Results.Ok();
	}
	catch (Exception ex) {
		return Results.BadRequest(ExceptionService.GetExceptionDetails(ex));
	}
});


app.MapGet("/tech/add-to-db-tables", async (AzureSqlDatabaseService dbService) =>
{
	try	{
		await dbService.AddDictionaryRuDeTranslations();
		return Results.Ok();
	}
	catch (Exception ex) {
		return Results.BadRequest(ExceptionService.GetExceptionDetails(ex));
	}
});


app.MapGet("/tech/generate-pronunciations-audio", async (
	PronunciationProviderService pronunciationProviderService
) => {
	try {
		await pronunciationProviderService.SaveGermanPronunciations();
		return Results.Ok(new { saved = true });
	}
	catch (Exception ex) {
		return Results.BadRequest(ExceptionService.GetExceptionDetails(ex));
	}
});


app.MapGet("/tech/CombineDictionary", async () => {
	try {
		var dictionaryParser = new DictionaryParserService();
		await dictionaryParser.CombineDocumentPagesAsync();
		return Results.Ok();
	}
	catch (Exception ex){
		return Results.BadRequest(ex.Message);
	}
});


app.Run();
