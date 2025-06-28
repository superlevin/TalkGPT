/*
Author: Lin Shou Shan (superlevin@gmail.com)
WebSite: https://superlevin.ifengyuan.tw
Date: 2024.12.27 (Updated from 2023.4.16)
Description: This is a C# example that combines ChatGPT with Microsoft Cognitive Services, allowing for voice conversations with ChatGPT. 
Updated to latest versions with .NET 8.0, newest package dependencies, and GPT-4 for improved performance and faster responses.
It has the potential to evolve into interesting applications such as elderly care and other engaging functionalities.

*/
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Text;
using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;
using Microsoft.Extensions.Configuration;
// read TalkGPT Settings
IConfiguration config = new ConfigurationBuilder()
.AddJsonFile("talkgptsettings.json", optional: true, reloadOnChange: true)
.Build();
 string OpenAI_APIKey = config["OpenAI_APIKEY"];
 string Azure_Speech_SubscriptionKEY = config["Azure_Speech_SubscriptionKEY"];
 string Azure_Speech_Region = config["Azure_Speech_Region"];
 string Azure_Speech_Language = config["Azure_Speech_Language"];
 string Azure_Speech_VoiceName = config["Azure_Speech_VoiceName"];
//
Console.InputEncoding = Encoding.Unicode;
Console.OutputEncoding = Encoding.Unicode;

var speechConfig = SpeechConfig.FromSubscription(Azure_Speech_SubscriptionKEY, Azure_Speech_Region);
FromMic(speechConfig, OpenAI_APIKey, Azure_Speech_Language, Azure_Speech_VoiceName).GetAwaiter().GetResult();
async static Task Speak(SpeechConfig speechConfig,string Language,string VoiceName, string text)
{
    speechConfig.SpeechSynthesisLanguage = Language;
    speechConfig.SpeechSynthesisVoiceName = VoiceName; //"zh-TW-HsiaoChenNeural" 
    using SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer(speechConfig);
    SpeechSynthesisResult speak = await speechSynthesizer.SpeakTextAsync(text);
    if (speak.Reason != ResultReason.SynthesizingAudioCompleted)
    {
        Console.WriteLine(speak.Reason);
    }
}


async static Task FromMic(SpeechConfig speechConfig,string OpenAI_KEY, string Language, string VoiceName)
{
    using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
    using var recognizer = new SpeechRecognizer(speechConfig, Language, audioConfig);
    Console.WriteLine("I am TalkGPT robot. Please give commands through the microphone...., To exit the program, please say 'goodbye'. ");
    await Speak(speechConfig, Language,VoiceName, "I am TalkGPT robot. Please give commands through the microphone...., To exit the program, please say 'goodbye'. ");
    var text = "";
    
    // Create OpenAI service once and reuse for better performance
    OpenAIService service = new OpenAIService(new OpenAiOptions() { ApiKey = OpenAI_KEY });
    
    while (!text.Contains("goodbye"))
    {
        var result = await recognizer.RecognizeOnceAsync();
        text = result.Text;
        var gptresult = "";
        //
        if (text.Trim().Length > 0)
        {
            Console.WriteLine($"\nYou Say = '{text}' ");
            var completionResult = await service.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
            {
                Messages = new List<ChatMessage>
                {
                    ChatMessage.FromUser(text)
                },
                Model = Models.Gpt_4,
                MaxTokens = 150, // Limit response length for faster responses
                Temperature = 0.7f // Slightly lower temperature for more focused responses
            });
            if (completionResult.Successful)
            {
                gptresult = completionResult.Choices.First().Message.Content;
                Console.WriteLine($"\nTalkGPT = '{gptresult}'");
                await Speak(speechConfig, Language, VoiceName, gptresult);
            }
        }
        //
    }
    Console.WriteLine("\n\n See U Next Time~ \n\n");
    await Speak(speechConfig, Language, VoiceName, "See U Next Time");

}

