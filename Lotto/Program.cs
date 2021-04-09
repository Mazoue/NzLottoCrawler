using HtmlAgilityPack;
using Lotto.Datamodels;
using Lotto.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Lotto
{
    class Program
    {

        static HtmlNodeCollection _monthlyDraws;

        static async Task Main(string[] args)
        {
            var lottoService = new LottoService();

            var drawResults = new List<DrawResult>();

            var baseUrl = "http://lottoresults.co.nz";
            var baseArchiveUrl = "/lotto/archive";

            //Step 1 - Get URLs of each previous month
            var archiveUrls = await lottoService.GetArchives(string.Concat(baseUrl, baseArchiveUrl));
            foreach (var archiveUrl in archiveUrls)
            {
                //Step 2 - Foreach previous month get all the draws for that month
                _monthlyDraws = await lottoService.GetMonthlyDraws(string.Concat(baseUrl, archiveUrl));

                // Foreach draw 
                // Get draw date
                // Get Jackpot
                // Get Lotto Numbers
                // Strike Numbers?
                foreach (var draw in _monthlyDraws)
                {
                    //Step 5 - Get the Lotto numbers
                    var results = await lottoService.GetLottoNumbers(draw);

                    if (results != null)
                    {
                        drawResults.AddRange(results);
                    }
                }

            }

            foreach (var drawResult in drawResults)
            {
                await PrintDrawResults(drawResult);
            }

            Console.WriteLine("Finished");

        }

        private static async Task PrintDrawResults(DrawResult currentDraw)
        {
            await using var outputFile = new StreamWriter(Path.Combine("D:\\Temp\\", "LottoNumbers.txt"), true);
            await outputFile.WriteLineAsync($"{currentDraw.IndividualDraw.BallOne},{currentDraw.IndividualDraw.BallTwo},{currentDraw.IndividualDraw.BallThree},{currentDraw.IndividualDraw.BallFour},{currentDraw.IndividualDraw.BallFive},{currentDraw.IndividualDraw.BallSix},{currentDraw.IndividualDraw.BonusBall},{currentDraw.PowerBallNumber}");
        }


    }


}
