using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace AnonfilesScraper {
    class Program {
        static async Task Main(string[] args) {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[10];
            var random = new Random();

            Console.WriteLine("ID Amount:");
            string amount = Console.ReadLine();
            int amountNumber;

            Int32.TryParse(amount, out amountNumber);

            List<string> idList = new List<string>(amountNumber);

            string workingFilePath = Path.Join(Environment.CurrentDirectory,"\\working.txt");
            
            if (!File.Exists(workingFilePath)) {
                using(StreamWriter sw = File.CreateText(workingFilePath)) {
                    sw.WriteLine("Working ID List");
                    sw.WriteLine("");
                }
            }

            for (int i = 0; i < amountNumber; i++) {
                for (int j = 0; j < stringChars.Length; j++) {
                    stringChars[j] = chars[random.Next(chars.Length)];
                }
                var finalString = new String(stringChars);
                idList.Add(finalString);
            }

            Console.WriteLine(idList.Count + " ID\'s generated.");

            for (int i = 0; i < idList.Count; i++) {
                await CheckURL("https://api.anonfiles.com/v2/file/" + idList[i] + "/info", idList[i], i);
            }
        }

        static async Task<string> CheckURL(string url, string id, int current) {
            string workingFilePath = Path.Join(Environment.CurrentDirectory,"\\working.txt");

            var client = new RestClient(url);
            var response = client.Execute(new RestRequest());
            var test = response.Content;
            var json = JsonConvert.DeserializeObject(response.Content);
            JObject obj = JObject.Parse(response.Content);
            string status = obj["status"].Value<string>();
            if(status == "False") {
                Console.WriteLine(current + " invalid.");
                Console.SetCursorPosition(0, Console.CursorTop);
                return ("Invalid");
            } else if(status == "True") {
                Console.WriteLine(id + " Working");
                using(StreamWriter sw = File.AppendText(workingFilePath)) {
                   await sw.WriteLineAsync(id);
                }
                return("Working");
            }

             return "";
        }
    }
}
