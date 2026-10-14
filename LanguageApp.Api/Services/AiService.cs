using System.Runtime.CompilerServices;
using OpenAI;
using OpenAI.Chat;

class AiService {
	private readonly IConfiguration _configuration;


	public AiService(IConfiguration configuration) {
		_configuration = configuration;
	}


	private void Log(string message) {
		string logEntry = $"[{DateTime.Now.ToString("yyyy.MM.dd HH:mm")}] {message}";
		using (StreamWriter writer = File.AppendText($"Logs/OpenAiLog.txt")) {
			writer.WriteLine(logEntry);
		}
	}


	public async Task<string> CompleteChatWithOpenAiAsync(string prompt) {
		string aiModel = "gpt-4.1";
		// string aiModel = "gpt-4.1-mini";
    var messages = new List<ChatMessage> { new UserChatMessage(prompt) };

		var openAiClient = new ChatClient(
			model: aiModel,
			apiKey: _configuration["OpenAiApiKey"]
		);

    ChatCompletion chatCompletion = await openAiClient.CompleteChatAsync(messages);
		string chatCompletionText = chatCompletion.Content[0].Text;
		int input = chatCompletion.Usage.InputTokenCount;
		int output = chatCompletion.Usage.OutputTokenCount;
		int total = chatCompletion.Usage.TotalTokenCount;

		Log($"OpenAi Request | {aiModel} | input {input}, output {output}, total {total}");

		return chatCompletionText;
	}


	public async Task<string> CompleteChatWithOpenAiAsync(string prompt, List<string> imagePaths){
    var messageContent = new List<ChatMessageContentPart>();
    messageContent.Add(ChatMessageContentPart.CreateTextPart(prompt));
    foreach (var imagePath in imagePaths){
			byte[] byteImage = File.ReadAllBytes(imagePath);
			BinaryData binaryImage = BinaryData.FromBytes(byteImage);
			string imageFormat = Path.GetExtension(imagePath).TrimStart('.');
			messageContent.Add(ChatMessageContentPart.CreateImagePart(binaryImage, $"image/{imageFormat}"));
    }
    var messages = new List<ChatMessage>{ new UserChatMessage(messageContent) };

		var openAiClient = new ChatClient(
			model: "gpt-4.1",
			apiKey: _configuration["OpenAiApiKey"]
		);
		
    ChatCompletion chatCompletion = await openAiClient.CompleteChatAsync(messages);
		string chatCompletionText = chatCompletion.Content[0].Text;
		int inputTokens = chatCompletion.Usage.InputTokenCount;
		int outputTokens = chatCompletion.Usage.OutputTokenCount;
		int totalTokens = chatCompletion.Usage.TotalTokenCount;

		return chatCompletionText;
	}
}
