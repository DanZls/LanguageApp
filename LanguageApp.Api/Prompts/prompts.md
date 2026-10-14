# Translate dictionary pages based on examples
Process files [1-9] in [Resources/Processed/DictionaryRuBgPages]:
- the files contain translations from [russian] to [bulgarian] language with index number and explanation
- for each file, create a file with the same name, but in directory [Resources/Processed/DictionaryRuDeFrequencyPages]
- for each [ru-bg] translation in the file, create a similar [ru-de] ([russian-german]) translation and add them to the created file
- your created translation lines must consist of: index (as the original), [russian] word (in [russian]), translation to [german] (in [german]), linguistic tags useful for word conjugation (in [english])
- omit meaning explanations in the new files, but keep translations consistent with them
- if a word has several translations, pick only one of them, the most used (and relevant to the meaning)
- the tags must be related to the translation ([german]) word
- separate tags with ";" only, do not add extra symbols such as "+", "()"
- check an example in the target directory (if present)
- process files one by one, do not start reading the next file until the work with the previous is completed


# Check dictionary pages for translation correctness
Files in [Resources/Processed/DictionaryRuDeFrequencyPages-100] contain [russian-german] translations. 
Process files [1-20]:
- Fix any mistakes (if any), including those in translations and in linguistic tags (that must be related to the translated word).
- Read and edit files one by one.
- Do not use scripts or subagents.


# Mark obsolete translations
Resources\Processed\DictionaryRuBgFrequencyPages contains page files with ru-bg translations. The translations look like "[word index number]|[word in russian]|[word meaning]|[word in bulgarian]|[linguistic tags]" with [linguistic tags] related to [word in bulgarian].
This data is prepared for a language learning app.
However, some of the translations may be irrelevant to the learning purpose: 
- names and surnames of people
- archaic and unused words
- abbreviations irrelevant to other languages
- words related to russia and ussr (i.e. "рубль",  "компартия")
- swearing and dirty speech
- several different problems may be present at once
- perform the following procedure for pages 0-100
- process the files in batches of 5
- read page files fully and check them for the problems
- after analysis of a batch, output to the end of log.txt the indices of violating translations with the concise exact reason in "()" parentheses for each file
- move to the next batch only when the work with the current one is completed
- do not use any scripts for this job, do the work yourself with your own knowledge


