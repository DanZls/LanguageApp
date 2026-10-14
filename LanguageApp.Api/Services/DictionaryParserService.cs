using System.Text;

class DictionaryParserService {
	private readonly AiService _aiService;
  

	public DictionaryParserService(AiService aiService) {
		_aiService = aiService;
	}

	public static List<Translation> ParseGeneratedDictionary(
		string dictionaryPath,
		uint startPageNumber = 0,
		uint endPageNumber = 0
	) {
		List<Translation> dictionaryList = [];
		for (uint pageNumber = startPageNumber; pageNumber <= endPageNumber; pageNumber++) {
			string path = $"{dictionaryPath}/{pageNumber}.txt";
			var dictionaryPageList = File.ReadAllLines(path)
				.Select(line => {
					string[] splitLine = line.Split('|');
					return new Translation {
						Id = int.Parse(splitLine[0]),
						Term = splitLine[1],
						TermMeaning = splitLine[2],
						TermTranslation = splitLine[3],
						TermTranslationTags = splitLine[4].ToLower().Replace(" ", "").Split("; "),
					};
				});
			// if (dictionaryPageList.ToArray().Length < 50)
			// 	throw new Exception($"page {pageNumber}: error");
			dictionaryList.AddRange(dictionaryPageList);
		}
		return dictionaryList;
	}


	public async Task<List<string>> GetDocumentPagesAsync(string documentPath, int linesNumberOnPage, int overlapping) {
		string[] fileLines = await File.ReadAllLinesAsync(documentPath);
		List<string> pages = [];
		int currentLineNumber = 0;
		while (currentLineNumber < fileLines.Length) {
			string pageLines = fileLines
				.Skip(currentLineNumber)
				.Take(linesNumberOnPage)
				.Aggregate((prev, next) => $"{prev}\n{next}" );
			pages.Add(pageLines);
			currentLineNumber += linesNumberOnPage - overlapping;
		}
		return pages;
	}


	public async Task CombineDocumentPagesAsync() {
		int startPage = 0;
		int endPage = 901;
		string pageFilePathTemplate = "Resources/Processed/DictExplRuOzhegovPages/DictExplRuOzhegov{pageNumber}.txt";
		string combinedFilePath = "Resources/Processed/DictExplRuOzhegovProcessed.txt";
		await File.WriteAllTextAsync(combinedFilePath, "");
		using StreamWriter streamWriter = File.AppendText(combinedFilePath);
		for (int pageNumber = startPage; pageNumber <= endPage; pageNumber++) {
			string pageFilePath = pageFilePathTemplate.Replace("{pageNumber}", pageNumber.ToString());
			string[] pageLines = await File.ReadAllLinesAsync(pageFilePath);
			foreach (var pageLine in pageLines) {
				if (pageLine.Trim() == "")
					continue;
				streamWriter.WriteLine(pageLine.Trim().Replace("ё", "е"));
			}
		}
	}


	public async Task ParseDictionaryRuBgChukalovAsync() {
		int[] pageNumbers = [1, 23, 75, 231, 553];
		int pageSizeLines = 150;
		int delay = 10 * 1000;
		string promptPath = "Resources/Prompts/ParseRuBgDictionaryPage2.txt";
		string documentPath = "Resources/Dictionaries/DictionaryRuBgChukalov.txt";
		List<string> pages = await GetDocumentPagesAsync(documentPath, pageSizeLines, overlapping: 5);
		string prompt = File.ReadAllText(promptPath);
		var outputDictionary = new StringBuilder();
		for (int pageNumber = 0; pageNumber < pages.Count; pageNumber++) {
			if (!pageNumbers.Contains(pageNumber))
				continue;
			string page = pages[pageNumber];
			var pageParseRequest = new ParsePageRequest();
			pageParseRequest.Prompt = prompt;
			pageParseRequest.DocumentPage = page;
			string aiResponse = await _aiService.CompleteChatWithOpenAiAsync(pageParseRequest.ToJson());
			File.WriteAllText($"Resources/Output/Intermediate/DictionaryRuBgProcessed{pageNumber}.txt", aiResponse);
			using (StreamWriter writer = File.AppendText($"Resources/Output/DictionaryRuBgProcessed.Total.txt")) {
				writer.WriteLine(aiResponse);
			}
			outputDictionary.Append(aiResponse);
			await Task.Delay(delay);
		}
		var dateTime = DateTime.Now.ToString("yyyy.MM.dd-HH.mm");
		File.WriteAllText($"Resources/Output/DictionaryRuBgProcessed[{dateTime}].txt", outputDictionary.ToString());
	}


	public async Task<string> ParseDictionaryRuUshakovAsync() {
		var pageNumbers = new HashSet<int>(Enumerable.Range(0, 150 + 1));
		int pageSizeLines = 300;
		int overlapping = 15;
		int delay = 20 * 1000;

		string documentPath = "Resources/1.Raw/DictionaryRuExplanatoryUshakov.txt";
		List<string> pages = await GetDocumentPagesAsync(documentPath, pageSizeLines, overlapping);
		string prompt = File.ReadAllText("Resources/Prompts/ParseDictionaryRuUshakovPage.txt");

		var outputDictionary = new StringBuilder();
		for (int pageNumber = 0; pageNumber < pages.Count; pageNumber++) {
			if (!pageNumbers.Contains(pageNumber))
				continue;
			string page = pages[pageNumber];
			var pageParseRequest = new ParsePageRequest {
				Prompt = prompt,
				DocumentPage = page
			};
			string aiResponse = await _aiService.CompleteChatWithOpenAiAsync(pageParseRequest.ToJson());
			File.WriteAllText(
				$"Resources/2.Processed/DictionaryRuUshakovPages/DictionaryRuUshakov{pageNumber}.txt", 
				aiResponse
			);
			outputDictionary.Append(aiResponse);
			await Task.Delay(delay);
		}
		var dateTime = DateTime.Now.ToString("yyyy.MM.dd-HH.mm");
		File.WriteAllText(
			$"Resources/2.Processed/Intermediate/DictionaryRuUshakovProcessed[{dateTime}].txt", 
			outputDictionary.ToString()
		);
		return "Ok";
	}
	

	public async Task<string> ParseDictRuOzhegovAsync() {
		int startPage = 601;
		int endPage = 1000;
		int pageSizeLines = 50;
		int overlapping = 5;
		int delay = 30 * 1000;
		string documentPath = "Resources/Raw/DictExplRuOzhegov.txt";
		string prompt = File.ReadAllText("Resources/Prompts/ParseDictRuOzhegovPage.txt");

		var pageNumbers = new HashSet<int>(Enumerable.Range(startPage, endPage - startPage + 1));
		List<string> pages = await GetDocumentPagesAsync(documentPath, pageSizeLines, overlapping);

		for (int pageNumber = 0; pageNumber < pages.Count; pageNumber++) {
			if (!pageNumbers.Contains(pageNumber))
				continue;
			string page = pages[pageNumber];
			var pageParseRequest = new ParsePageRequest {
				Prompt = prompt,
				DocumentPage = page
			};
			string aiResponse = await _aiService.CompleteChatWithOpenAiAsync(pageParseRequest.ToJson());
			File.WriteAllText(
				$"Resources/Processed/DictExplRuOzhegovPages/DictExplRuOzhegov{pageNumber}.txt", 
				aiResponse
			);
			await Task.Delay(delay);
		}
		return "Ok";
	}


	public async Task CreateDictionaryRuBgFrequencyPagesAsync(
		int? pageNumber = null,
		IEnumerable<int>? pageNumbers = null,
		int startPageNumber = 200,
		int endPageNumber = 1000
	) {
		string dictionaryRuFrequencyPath = "Resources/Raw/DictionaryRuFrequency30000.txt";
		string dictionaryRuBgPath = "Resources/Processed/DictionaryRuBgChukalovProcessed.txt";
		string dictExplRuPath = "Resources/Processed/DictExplRuOzhegovProcessed.txt";
		string promptPath = "Resources/Prompts/CreateDictionaryRuBgFrequencyPage.txt";
		string outputPagePathTemplate = "Resources/Processed/DictionaryRuBgFrequencyPages/{pageNumber}.txt";
		int linesOnPage = 50;
		int delay = 5 * 1000;

		List<string> dictionaryRuFrequencyPages = await GetDocumentPagesAsync(dictionaryRuFrequencyPath, linesOnPage, overlapping: 0);
		
		string[] dictionaryRuBgLines = await File.ReadAllLinesAsync(dictionaryRuBgPath);
		var dictionaryRuBgLinesTable = new Dictionary<string, string>();
		foreach (string line in dictionaryRuBgLines) {
			string keyWord = line.Split(' ').First().Trim();
			dictionaryRuBgLinesTable.TryAdd(keyWord, "");
			dictionaryRuBgLinesTable[keyWord] += line + "\n";
		}
		
		string[] dictExplRuLines = await File.ReadAllLinesAsync(dictExplRuPath);
		var dictExplRuLinesTable = new Dictionary<string, string>();
		foreach (string line in dictExplRuLines) {
			string keyWord = line.Split('|').First().Trim();
			dictExplRuLinesTable.TryAdd(keyWord, "");
			dictExplRuLinesTable[keyWord] += line + "\n";
		}

		string prompt = await File.ReadAllTextAsync(promptPath);

		IEnumerable<int> targetPageNumbers;
		if (pageNumber.HasValue) {
			targetPageNumbers = [pageNumber.Value];
		}
		else if (pageNumbers != null && pageNumbers.Any()) {
			targetPageNumbers = pageNumbers.Distinct().OrderBy(n => n);
		}
		else {
			targetPageNumbers = Enumerable.Range(startPageNumber, endPageNumber - startPageNumber + 1);
		}

		foreach (int targetPageNumber in targetPageNumbers) {
			if (targetPageNumber < 0 || targetPageNumber >= dictionaryRuFrequencyPages.Count)
				continue;

			string dictionaryRuFrequencyPage = dictionaryRuFrequencyPages[targetPageNumber];
			string dictionaryRuBgPage = dictionaryRuFrequencyPage
				.Split("\n")
				.Select(line => line.Split(' ')[2])
				.Select(word => dictionaryRuBgLinesTable.GetValueOrDefault(word, ""))
				.Distinct()
				.Aggregate((prev, next) => $"{prev}\n{next}");
			string dictExplRuPage = dictionaryRuFrequencyPage
				.Split("\n")
				.Select(line => line.Split(' ')[2])
				.Select(word => dictExplRuLinesTable.GetValueOrDefault(word, ""))
				.Distinct()
				.Aggregate((prev, next) => $"{prev}\n{next}");
			var request = new {
				Instructions = prompt,
				DictionaryFrequencyRu = dictionaryRuFrequencyPage,
				DictionaryExplanatoryRu = dictExplRuPage,
				DictionaryRuBg = dictionaryRuBgPage
			};
			string serializedRequest = RelaxedSerializer.SerializeToJson(request);
			string aiResponse = await _aiService.CompleteChatWithOpenAiAsync(serializedRequest);
			string outputPagePath = outputPagePathTemplate.Replace("{pageNumber}", targetPageNumber.ToString());
			File.WriteAllText(outputPagePath, aiResponse);
			await Task.Delay(delay);
		}
	}
	
}