using HtmlAgilityPack;
using Lotto.Datamodels;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private static DateTime? GetDrawDate(List<string> currentDrawInnerText)
        {
            var drawDate = "";
            foreach (var innerTextLine in currentDrawInnerText.Where(innerTextLine => innerTextLine.Contains("Lotto Result for")))
            {
                drawDate = innerTextLine[(innerTextLine.IndexOf(",", StringComparison.Ordinal) + 1)..];
            }
            return DateTime.Parse(drawDate.Trim());
        }

        private static string GetJackpot(List<string> currentDrawInnerText)
        {
            var jackPot = "";
            foreach (var innerTextLine in currentDrawInnerText.Where(innerTextLine => innerTextLine.Contains("Jackpot:")))
            {
                jackPot = innerTextLine[(innerTextLine.IndexOf(":", StringComparison.Ordinal) + 1)..];
            }

            return jackPot.Trim();
        }

        public async Task<List<DrawResult>> GetLottoNumbers(HtmlNode currentDraw)
        {
            return GetWeeklyLottoNumbersFromCurrentMonth(currentDraw);
        }

        private static List<DrawResult> GetWeeklyLottoNumbersFromCurrentMonth(HtmlNode currentDraw)
        {
            var currentDrawInnerText = currentDraw.InnerText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var currentDrawInnerHtmlChunk = currentDraw.InnerHtml.ToString();
            var currentDrawInnerHtml =
                currentDrawInnerHtmlChunk.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            var getNumbersNodes = GetNumbersNodes(currentDrawInnerHtmlChunk);

            var weeklyDraws = new List<DrawResult>();

            var drawResult = new DrawResult
            {
                //Step 3 - Get the Date
                DrawDate = GetDrawDate(currentDrawInnerText.ToList()),

                //Step 4 - Get the Jackpot 
                Jackpot = GetJackpot(currentDrawInnerText.ToList()),

                IndividualDraw = GetDrawLottoNumbers(getNumbersNodes.LottoNumbersHtml.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList()),
                PowerBallNumber = GetDrawPowerBall(getNumbersNodes.PowerBallsHtml.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList()),
                Strike = GetDrawStrikeNumbers(getNumbersNodes.StrikeNumbersHtml.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList())
            };

            weeklyDraws.Add(drawResult);

            return weeklyDraws;
        }

        private static IndividualDraw GetNumbersNodes(string currentDrawInnerHtml)
        {
            var lottoNumberString = currentDrawInnerHtml.Substring(currentDrawInnerHtml.IndexOf("<ol class=\"draw-result\">", StringComparison.Ordinal) + 1);

            var weeklyLottoNumbers = lottoNumberString.Substring(0, lottoNumberString.IndexOf("</ol>", StringComparison.Ordinal));

            var currentDrawInnerHtmlTemp = currentDrawInnerHtml.Substring(currentDrawInnerHtml.IndexOf("</ol>", StringComparison.Ordinal) + 5);

            var powerBall = currentDrawInnerHtmlTemp.Substring(
                currentDrawInnerHtmlTemp.IndexOf("<ol class=\"draw-result draw-result--sub\">", StringComparison.Ordinal) + 1, currentDrawInnerHtmlTemp.IndexOf("</ol>", StringComparison.Ordinal));

            currentDrawInnerHtmlTemp = currentDrawInnerHtml.Substring(currentDrawInnerHtml.IndexOf("</ol>", StringComparison.Ordinal) + 5);

            var strikeNumbers = currentDrawInnerHtmlTemp.Substring(
                currentDrawInnerHtmlTemp.IndexOf("<ol class=\"draw-result draw-result--sub\">", StringComparison.Ordinal) + 1, currentDrawInnerHtmlTemp.IndexOf("</ol>", StringComparison.Ordinal));

            return new IndividualDraw()
            {
                LottoNumbersHtml = weeklyLottoNumbers,
                PowerBallsHtml = powerBall,
                StrikeNumbersHtml = strikeNumbers
            };
        }

        private static LottoNumbers GetDrawLottoNumbers(List<string> currentWeekNumbers)
        {
            var lottoNumbers = new List<int>();
            foreach (var currentNode in currentWeekNumbers)
            {
                if (currentNode.Contains("draw-result__ball"))
                {
                    if (currentNode.Trim().StartsWith("<li class=\"draw-result__ball"))
                    {
                        var substringLength = currentNode.LastIndexOf("<", StringComparison.Ordinal) - currentNode.IndexOf(">", StringComparison.Ordinal) - 1;
                        var lottoNumber = currentNode.Substring(currentNode.IndexOf(">", StringComparison.Ordinal) + 1, substringLength);
                        lottoNumbers.Add(Convert.ToInt32(lottoNumber));
                    }
                    else
                    {
                        var bonusBallNode = currentNode.Substring(currentNode.IndexOf("</li>", StringComparison.Ordinal) + 5);
                        var substringLength = bonusBallNode.LastIndexOf("<", StringComparison.Ordinal) - bonusBallNode.IndexOf(">", StringComparison.Ordinal) - 1;
                        var lottoNumber = bonusBallNode.Substring(bonusBallNode.IndexOf(">", StringComparison.Ordinal) + 1, substringLength);
                        lottoNumbers.Add(Convert.ToInt32(lottoNumber));
                    }
                }
            }

            return new LottoNumbers()
            {
                BallOne = lottoNumbers[0],
                BallTwo = lottoNumbers[1],
                BallThree = lottoNumbers[2],
                BallFour = lottoNumbers[3],
                BallFive = lottoNumbers[4],
                BallSix = lottoNumbers[5],
                BonusBall = lottoNumbers[6]
            };
        }
        private static int GetDrawPowerBall(List<string> powerBallNumbers)
        {
            var powerBallNumber = new int();
            foreach (var currentNode in powerBallNumbers)
            {
                if (currentNode.Contains("draw-result__ball"))
                {
                    var substringLength = currentNode.LastIndexOf("<", StringComparison.Ordinal) - currentNode.IndexOf(">", StringComparison.Ordinal) - 1;
                    var lottoNumber = currentNode.Substring(currentNode.IndexOf(">", StringComparison.Ordinal) + 1, substringLength);
                    powerBallNumber = (Convert.ToInt32(lottoNumber));
                }
            }

            return powerBallNumber;
        }

        private static Strike GetDrawStrikeNumbers(List<string> currentWeekStrike)
        {
            var lottoNumbers = new List<int>();
            foreach (var currentNode in currentWeekStrike)
            {
                if (currentNode.Contains("draw-result__ball"))
                {
                    var substringLength = currentNode.LastIndexOf("<", StringComparison.Ordinal) - currentNode.IndexOf(">", StringComparison.Ordinal - 1);
                    var lottoNumber = currentNode.Substring(currentNode.IndexOf(">", StringComparison.Ordinal) + 1, substringLength);
                    lottoNumbers.Add(Convert.ToInt32(lottoNumber));
                }
            }

            return new Strike()
            {
                BallOne = lottoNumbers[0],
                BallTwo = lottoNumbers[1],
                BallThree = lottoNumbers[2],
                BallFour = lottoNumbers[3]
            };
        }
    }
}
