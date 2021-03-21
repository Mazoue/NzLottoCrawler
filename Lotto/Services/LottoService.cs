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
        private readonly HttpClient client = new HttpClient();

        public async Task<List<string>> GetArchives(string archiveUrl)
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

        public async Task<HtmlNodeCollection> GetMonthlyDraws(string drawUrl)
        {
            string responseBody = await client.GetStringAsync(drawUrl);
            HtmlDocument pageDocument = new HtmlDocument();
            pageDocument.LoadHtml(responseBody);
            var monthlyDraw = pageDocument.DocumentNode.SelectNodes("(//div[@class='result-card'])");
            return monthlyDraw;
        }

        public DateTime? GetDrawDate(HtmlNode currentDraw)
        {
            if (currentDraw?.ChildNodes["div"]?.ChildNodes["h2"]?.InnerText != null)
            {
                return Convert.ToDateTime(currentDraw.ChildNodes["div"].ChildNodes["h2"].InnerText.Substring(currentDraw.ChildNodes["div"].ChildNodes["h2"].InnerText.IndexOf(",") + 1).Trim());
            }
            return null;
        }

        public string GetJackpot(HtmlNode currentDraw)
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

        public async Task<List<IndividualDraw>> GetLottoNumbers(HtmlNode currentDraw)
        {
            //Check for blank number
            //if blank the next number is the bonus ball and will be stored as the key

            // var resultCards = currentDraw.SelectNodes("//div[contains(@class, 'result-card__content')]");

            var resultCards = currentDraw.SelectNodes("//div[@class='result-card']");

            var monthlyResults = new List<IndividualDraw>();
            foreach (var resultCard in resultCards)
            {
                Console.WriteLine(resultCard.InnerText);
                var drawResults = GetWeeklyLottoNumbers(resultCard);
                
                monthlyResults.Add(drawResults);
            }

            //return new Dictionary<int, Array>() { { bonusBallNumber, lottoNumbers } };            
            return monthlyResults;
        }

        private IndividualDraw GetWeeklyLottoNumbers(HtmlNode resultCard)
        {
            var lottoNumbers = new int[6];
            var lottoNumbersIndex = 0;
            var bonusBallNumber = -1;
            var blankNumber = false;

            var orderedLists = resultCard.SelectNodes("//ol[@class='draw-result']");
            foreach (var orderedList in orderedLists)
            {

                var listItems = orderedList.SelectNodes("li");
                foreach (var listItem in listItems)
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

            }

            return new IndividualDraw()
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
        }
    }
}
