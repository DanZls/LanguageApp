class DictionaryParserService {
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
	
}