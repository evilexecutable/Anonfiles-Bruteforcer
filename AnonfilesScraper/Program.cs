using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace AnonfilesScraper
{
    class Program
    {
        static string workingFilePath = Path.Join(Environment.CurrentDirectory, "\\working.txt");
        static int workingIDs = 0;
        static int invalidIDs = 0;
        static int remaining = 0;
        static async Task Main(string[] args)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[10];
            var random = new Random();

            Console.WriteLine("ID Amount:");
            string amount = Console.ReadLine();
            int amountNumber;

            Int32.TryParse(amount, out amountNumber);
            remaining = amountNumber;
            List<string> idList = new List<string>(amountNumber);


            if (!File.Exists(workingFilePath))
            {
                using (StreamWriter sw = File.CreateText(workingFilePath))
                {
                    sw.WriteLine("Working ID List");
                    sw.WriteLine("");
                }
            }

            for (int i = 0; i < amountNumber; i++)
            {
                for (int j = 0; j < stringChars.Length; j++)
                {
                    stringChars[j] = chars[random.Next(chars.Length)];
                }
                var finalString = new String(stringChars);
                idList.Add(finalString);
            }

            Console.WriteLine(idList.Count + " ID\'s generated.");
            DateTime start = DateTime.Now;
            Console.Clear();
            Console.CursorVisible = false;

            for (int i = 0; i < idList.Count; i++)
            {
                remaining += -1;

                if(i == 0)
                {
                    await CheckURL("https://api.anonfiles.com/v2/file/" + idList[i] + "/info", idList[i], i);
                    Console.WriteLine($"Started: {start} | Working: {workingIDs} | Invalid: {invalidIDs} | Remaining: {remaining} | Success Rate: 0%          ");
                }
                else
                {
                    int successRate = (workingIDs / (workingIDs + invalidIDs)) * 100;

                    await CheckURL("https://api.anonfiles.com/v2/file/" + idList[i] + "/info", idList[i], i);
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine($"Started: {start} | Working: {workingIDs} | Invalid: {invalidIDs} | Remaining: {remaining} | Success Rate: {Math.Round(successRate + 0.0, 2)}%          ");
                }
                
            }
        }

        static async Task<bool> CheckURL(string url, string id, int current)
        {
            var client = new RestClient(url);
            var response = client.Execute(new RestRequest());

            JObject obj = JObject.Parse(response.Content);
            string status = obj["status"].Value<string>();
            if (status == "False")
            {
                invalidIDs++;
                return (false);
            }
            else if (status == "True")
            {
                workingIDs++;
                using (StreamWriter sw = File.AppendText(workingFilePath))
                {
                    await sw.WriteLineAsync(id);
                }
                return (true);
            }

            return true;
        }
    }
}
