using HtmlAgilityPack;
using Lotto.Datamodels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Lotto
{       //Step 1 - Get URLs of each previous month
        //Step 2 - Foreach previous month get all the draws for that month
        // Foreach draw 
        //Step 3 - Get the Date
        //Step 4 - Get the Jackpot status
        //Step 5 - Get the Lotto numbers
        //Step 6 - Get the BonusBall number
        //Step 7 - Get the PowerBall Number
        //Step 8 - Get the Strike numbers
    class Program
    {
        static readonly HttpClient client = new HttpClient();
        static HtmlNodeCollection _monthlyDraws;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var baseUrl = "http://lottoresults.co.nz";
            var baseArchiveUrl = "/lotto/archive";

            //Step 1 - Get URLs of each previous month
            var archiveUrls = await GetArchives(string.Concat(baseUrl, baseArchiveUrl));
            foreach (var archiveUrl in archiveUrls)
            {
                //Step 2 - Foreach previous month get all the draws for that month
                _monthlyDraws = await GetMonthlyDraws(string.Concat(baseUrl, archiveUrl));

                // Foreach draw 
                //Each monthly draw contains four draws.
                //not iterating through them..
                foreach (var draw in _monthlyDraws)
                {
                    var drawResult = new DrawResult();
                    //Step 3 - Get the Date
                    drawResult.DrawDate = GetDrawDate(draw);

                    //Step 4 - Get the Jackpot 
                    drawResult.Jackpot = GetJackpot(draw);

                    //Step 5 - Get the Lotto numbers
                    var allLottoNumbers = GetLottoNumbers(draw);
                    drawResult.LottoNumbers = new LottoNumbers()
                    {
                        BonusBallNumber = allLottoNumbers.FirstOrDefault().Key,
                        Numbers = allLottoNumbers.FirstOrDefault().Value
                    };

                    PrintDrawResults(drawResult);
                }
            }


        }

        private static async Task<List<string>> GetArchives(string archiveUrl)
        {
            var archiveUrls = new List<string>();
            try
            {
                string responseBody = await client.GetStringAsync(archiveUrl);
                HtmlDocument pageDocument = new HtmlDocument();
                pageDocument.LoadHtml(responseBody);

                var baseArchive = pageDocument.DocumentNode.SelectNodes("(//div[contains(@class,'archive-box')])");
                foreach (var baseArchiveNode in baseArchive)
                {
                    foreach (var archiveNode in baseArchiveNode.ChildNodes["div"].ChildNodes["ul"].Descendants("li"))
                    {
                        //Console.WriteLine(archiveNode.InnerText);
                        archiveUrls.Add(archiveNode.ChildNodes["a"].Attributes["href"].Value);
                    }
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
            return archiveUrls;
        }

        private static async Task<HtmlNodeCollection> GetMonthlyDraws(string drawUrl)
        {
            string responseBody = await client.GetStringAsync(drawUrl);
            HtmlDocument pageDocument = new HtmlDocument();
            pageDocument.LoadHtml(responseBody);
            return pageDocument.DocumentNode.SelectNodes("(//div[@class='result-card'])");           
        }

        private static DateTime? GetDrawDate(HtmlNode currentDraw)
        {
            if (currentDraw?.ChildNodes["div"]?.ChildNodes["h2"]?.InnerText != null)
            {
                return Convert.ToDateTime(currentDraw.ChildNodes["div"].ChildNodes["h2"].InnerText.Substring(currentDraw.ChildNodes["div"].ChildNodes["h2"].InnerText.IndexOf(",") + 1).Trim());
            }
            return null;
        }

        private static string GetJackpot(HtmlNode currentDraw)
        {
            try
            {
                return currentDraw.ChildNodes[3].ChildNodes[1].ChildNodes[3].InnerText;
            }
            catch(Exception ex)
            {
                return "Fail";
            }
        }

        private static Dictionary<int,Array> GetLottoNumbers(HtmlNode currentDraw)
        {            
            //Check for blank number
            //if blank the next number is the bonus ball and will be stored as the key
            var lottoResultBlock = currentDraw.SelectNodes("(//ol[@class='draw-result'])").Descendants("li");

            var lottoNumbers = new int[6];
            var lottoNumbersIndex = 0;
            var bonusBallNumber = -1;
            var blankNumber = false;
            foreach (var resultNumber in lottoResultBlock)
            {
                if (bonusBallNumber != -1)
                    break;

                if(!blankNumber)
                {
                    if (string.IsNullOrEmpty(resultNumber.InnerText))
                    { blankNumber = true; }
                    else
                    { lottoNumbers[lottoNumbersIndex] = int.Parse(resultNumber.InnerText); }                    
                }
                else
                {
                    bonusBallNumber = int.Parse(resultNumber.InnerText);
                }
                lottoNumbersIndex++;
            }
            return new Dictionary<int, Array>() { { bonusBallNumber, lottoNumbers } };            
        }

        private static void PrintDrawResults(DrawResult currentDraw)
        {
            Console.WriteLine($"DrawDate: {currentDraw.DrawDate}");
            Console.WriteLine($"JackPot: {currentDraw.Jackpot}");
            var lottoString = "";
            foreach(var lottoNumber in currentDraw.LottoNumbers.Numbers)
            {
                if(string.IsNullOrEmpty(lottoString))
                {
                    lottoString += lottoNumber;
                }
                else
                    lottoString += ", " + lottoNumber;
            }
            Console.WriteLine($"Lotto Numbers: {lottoString}   Bonus Ball Number: {currentDraw.LottoNumbers.BonusBallNumber}");
        }
        
    }

    
}
