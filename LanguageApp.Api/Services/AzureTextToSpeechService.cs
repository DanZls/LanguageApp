using Microsoft.CognitiveServices.Speech;
using NAudio.Lame;
using NAudio.Wave;
using SoundTouch;

public class AzureTextToSpeechService
{
	private readonly IConfiguration _config;

	public AzureTextToSpeechService(
		IConfiguration config
	) {
		_config = config;
	}


	private async Task<SpeechConfig> GetSpeechConfigAsync()
	{
		var config = SpeechConfig.FromSubscription(
			subscriptionKey: _config["AzureSpeech:ApiKey"], 
			region: _config["AzureSpeech:Region"]
		);
		config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio24Khz96KBitRateMonoMp3);
		return config;
	}


	public async Task<byte[]> GetAudioAsync(
		string text,
		string languageTag,
		string voiceModelTag
	) {
		var speechConfig = await GetSpeechConfigAsync();
		speechConfig.SpeechSynthesisVoiceName = voiceModelTag;
    speechConfig.SpeechSynthesisLanguage = languageTag;

		// null audio config = in-memory output only (no speaker)
		using var synthesizer = new SpeechSynthesizer(speechConfig, audioConfig: null);
		using var result = await synthesizer.SpeakTextAsync(text);

		if(result.Reason != ResultReason.SynthesizingAudioCompleted) {
			var error = SpeechSynthesisCancellationDetails.FromResult(result);
			throw new InvalidOperationException($"{error.ErrorCode}: {error.ErrorDetails}");
		}

		return result.AudioData;
	}


	private byte[] StretchAudio(
		byte[] mp3Bytes,
		double tempoChangePercent = -25
	) {
		// 1. Decode MP3 → PCM samples
		using var inputStream = new MemoryStream(mp3Bytes);
		using var mp3Reader = new Mp3FileReader(inputStream);
		using var pcmStream = WaveFormatConversionStream.CreatePcmStream(mp3Reader);

		var waveFormat = pcmStream.WaveFormat;
		var pcmBytes = new byte[pcmStream.Length];
		pcmStream.ReadExactly(pcmBytes);

		// Convert bytes → float samples
		var samples = new float[pcmBytes.Length / 2];
		for (int i = 0; i < samples.Length; i++)
			samples[i] = BitConverter.ToInt16(pcmBytes, i * 2) / 32768f;

		// 2. Apply tempo stretch (no pitch change)
		var st = new SoundTouchProcessor();
		st.SampleRate = waveFormat.SampleRate;
		st.Channels = waveFormat.Channels;
		st.TempoChange = tempoChangePercent; // +25 = 25% slower playback duration

		st.PutSamples(samples, samples.Length / waveFormat.Channels);
		st.Flush();

		var stretched = new List<float>();
		var buffer = new float[4096 * waveFormat.Channels];
		uint received;
		while ((received = (uint)st.ReceiveSamples(buffer, 4096)) > 0)
			stretched.AddRange(buffer.Take((int)(received * waveFormat.Channels)));

		// Convert float samples → bytes
		var outBytes = new byte[stretched.Count * 2];
		for (int i = 0; i < stretched.Count; i++)
		{
			short s = (short)Math.Clamp(stretched[i] * 32768f, short.MinValue, short.MaxValue);
			BitConverter.GetBytes(s).CopyTo(outBytes, i * 2);
		}

		// 3. Re-encode PCM → MP3
		using var outStream = new MemoryStream();
		using var writer = new LameMP3FileWriter(outStream, waveFormat, LAMEPreset.STANDARD);
		writer.Write(outBytes, 0, outBytes.Length);
		writer.Flush();
		return outStream.ToArray();
	}
}