using HtmlAgilityPack;
using Lotto.Datamodels;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Lotto.Services
{
    public class LottoService
    {
        private readonly HttpClient _client = new HttpClient();

        public async Task<List<string>> GetArchives(string archiveUrl)
        {
            var archiveUrls = new List<string>();
            try
            {
                var responseBody = await _client.GetStringAsync(archiveUrl);
                var pageDocument = new HtmlDocument();
                pageDocument.LoadHtml(responseBody);

                var baseArchive = pageDocument.DocumentNode.SelectNodes("(//div[contains(@class,'archive-box')])");
                foreach (var baseArchiveNode in baseArchive)
                {
                    foreach (var archiveNode in baseArchiveNode.ChildNodes["div"].ChildNodes["ul"].Descendants("li"))
                    {
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

        public async Task<HtmlNodeCollection> GetMonthlyDraws(string drawUrl)
        {
            var responseBody = await _client.GetStringAsync(drawUrl);
            var pageDocument = new HtmlDocument();
            pageDocument.LoadHtml(responseBody);
            var monthlyDraw = pageDocument.DocumentNode.SelectNodes("(//div[@class='result-card'])");
            return monthlyDraw;
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
            catch (Exception ex)
            {
                return $"Failed to get jackpot. Error :{ex.Message}";
            }
        }

        public async Task<List<DrawResult>> GetLottoNumbers(HtmlNode currentDraw)
        {
            return GetWeeklyLottoNumbersFromCurrentMonth(currentDraw);
        }

        private static List<DrawResult> GetWeeklyLottoNumbersFromCurrentMonth(HtmlNode currentDraw)
        {
            var weeklyDraws = new List<DrawResult>();
            var orderedLists = currentDraw.SelectNodes("//ol[@class='draw-result']");
            foreach (var orderedList in orderedLists)
            {
                var drawResult = new DrawResult
                {
                    //Step 3 - Get the Date
                    DrawDate = GetDrawDate(currentDraw),

                    //Step 4 - Get the Jackpot 
                    Jackpot = GetJackpot(currentDraw)
                };

                var listItems = orderedList.SelectNodes("li");
                drawResult.IndividualDraw = GetWeeklyLottoNumbers(listItems);
                weeklyDraws.Add(drawResult);
            }
            return weeklyDraws;
        }

        private static IndividualDraw GetWeeklyLottoNumbers(HtmlNodeCollection currentWeekNode)
        {
            var lottoNumbers = new int[6];
            var lottoNumbersIndex = 0;
            var bonusBallNumber = -1;
            var blankNumber = false;

            foreach (var listItem in currentWeekNode)
            {
                if (bonusBallNumber != -1)
                    break;

                if (!blankNumber)
                {
                    if (string.IsNullOrEmpty(listItem.InnerText))
                    { blankNumber = true; }
                    else
                    {
                        lottoNumbers[lottoNumbersIndex] = int.Parse(listItem.InnerText);
                    }
                }
                else
                {
                    bonusBallNumber = int.Parse(listItem.InnerText);
                }
                lottoNumbersIndex++;
            }

            var currentWeekDraw = new IndividualDraw()
            {
                BonusBallNumber = bonusBallNumber,
                LottoNumbers = new LottoNumbers()
                {
                    BallOne = lottoNumbers[0],
                    BallTwo = lottoNumbers[1],
                    BallThree = lottoNumbers[2],
                    BallFour = lottoNumbers[3],
                    BallFive = lottoNumbers[4],
                    BallSix = lottoNumbers[5],
                }
            };

            return currentWeekDraw;
        }
    }
}
