using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using VismaBot.Models;

namespace VismaBot.Dialogs
{
    public class HandleMessage
    {
        private int _inputCount;
        private StringBuilder responsBuilder;

        public string CreateResponseMessage(ChatRequest msg)
        {
            // Create a client.
            ITextAnalyticsAPI client = new TextAnalyticsAPI();
            client.AzureRegion = AzureRegions.Westeurope;
            client.SubscriptionKey = Environment.GetEnvironmentVariable("clientSubscriptionKey");
            _inputCount++;

            //Luis
            var httpClient = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            var luisAppId = Environment.GetEnvironmentVariable("luisAppId");
            var subscriptionKey = Environment.GetEnvironmentVariable("subscriptionKey");

            // The request header contains your subscription key
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            // The "q" parameter contains the utterance to send to LUIS
            queryString["q"] = msg.message.text;

            // These optional request parameters are set to their default values
            queryString["timezoneOffset"] = "0";
            queryString["verbose"] = "false";
            queryString["spellCheck"] = "false";
            queryString["staging"] = "false";
            var uri = "https://westeurope.api.cognitive.microsoft.com/luis/v2.0/apps/" + luisAppId + "?" + queryString;
            var response = httpClient.GetAsync(uri).Result;
            string lang = null;


            var dataFromLuis = JsonConvert.DeserializeObject<LuisResponse>
                (response.Content.ReadAsStringAsync().Result);

            if (dataFromLuis.entities.Length > 0)
                lang = dataFromLuis.entities[0].entity;


            //Finne språket
            LanguageBatchResult res = client.DetectLanguage(
                new BatchInput(
                    new List<Input>
                    {
                        new Input(_inputCount.ToString(), msg.message.text)
                    }));


            // calculate something for us to return

            StringBuilder keyWordBuilder = new StringBuilder();
            keyWordBuilder.Append(" ");

            // Printing language results.
            foreach (var document in res.Documents)
            {
                //Finne nøkkelfraser
                KeyPhraseBatchResult res2 = client.KeyPhrases(
                    new MultiLanguageBatchInput(
                        new List<MultiLanguageInput>
                        {
                            new MultiLanguageInput(document.DetectedLanguages[0].Iso6391Name, _inputCount.ToString(),
                                msg.message.text)
                        }));


                // Printing keyphrases
                foreach (var doc2 in res2.Documents)
                {
                    foreach (string keyphrase in doc2.KeyPhrases)
                        keyWordBuilder.Append(keyphrase + " ");

                    if (doc2.KeyPhrases.Count == 0)
                        keyWordBuilder.Append("Fant ingen nøkkelfraser");
                }

                // Extracting sentiment
                SentimentBatchResult res3 = client.Sentiment(
                    new MultiLanguageBatchInput(
                        new List<MultiLanguageInput>
                        {
                            new MultiLanguageInput(document.DetectedLanguages[0].Iso6391Name, _inputCount.ToString(),
                                msg.message.text)
                        }));


                // Printing sentiment results
                foreach (var doc3 in res3.Documents)
                {
                    ConsultantResponse dataFromResponsefromConsultant = null;
                    var httpConsultant = new HttpClient();
                    httpConsultant
                        .DefaultRequestHeaders
                        .Accept
                        .Add(new MediaTypeWithQualityHeaderValue("application/json")); //ACCEPT header


                    if (lang != null)
                    {
                        var responsefromConsultant =
                            httpConsultant.GetAsync(
                                $"http://37.139.15.166/consultants?skill={dataFromLuis.entities[0].entity}").Result;
                        dataFromResponsefromConsultant =
                            JsonConvert.DeserializeObject<ConsultantResponse>(
                                responsefromConsultant.Content.ReadAsStringAsync().Result);
                    }


                    string returnConsultantIfData(ConsultantResponse cr)
                    {
                        int count = 0;

                        if (cr != null && cr.consultants.Length > 0)
                        {
                            StringBuilder cnBuilder = new StringBuilder();
                            cnBuilder.AppendLine(
                                $"I hear you are looking for people that know {dataFromLuis.entities[0].entity} programming language. In our resource database i found: ");

                            foreach (var c in cr.consultants)
                            {
                                cnBuilder.Append(c.name);
                                count++;

                                if (count < cr.consultants.Length)
                                {
                                    cnBuilder.Append(", ");
                                }

                            }

                            return cnBuilder.ToString();
                        }
                        return null;
                    }

                    var textInput = msg.message.text;
                    var langIs = document.DetectedLanguages[0].Name;
                    var keyFrases = keyWordBuilder.ToString().TrimEnd();
                    var emotionScoreIs = $"{doc3.Score:0.00}";

                    bool onlyLuisApi = true;

                    responsBuilder = new StringBuilder();

                    if (onlyLuisApi)
                    {
                        //Only luis fetch programming skills
                        responsBuilder.
                            Append(returnConsultantIfData(dataFromResponsefromConsultant));
                    }
                    else
                    {
                        //With detect language, sentiment, key frases and luis programming skills
                        responsBuilder.
                            Append("Hello! You wrote ").
                            AppendLine(textInput + ".").
                            Append("The language is most likely: ").
                            AppendLine(langIs + ".").
                            Append("The key frases are: ").
                            AppendLine(keyFrases + ".").
                            Append("Based what you wrote i detected the sentiment score: ").
                            AppendLine(emotionScoreIs + " On a scale between 0-1, where 0 is the most negative(sad) and 1 is most positive(happy).").
                            Append(returnConsultantIfData(dataFromResponsefromConsultant));
                    }

                   
                }
            }

            return responsBuilder.ToString();
        }
    }
}