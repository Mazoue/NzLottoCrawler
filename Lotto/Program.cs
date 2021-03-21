using HtmlAgilityPack;
using Lotto.Datamodels;
using Lotto.Services;
using System;
using System.Linq;
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

        static HtmlNodeCollection _monthlyDraws;

        static async Task Main(string[] args)
        {
            var lottoService = new LottoService();

            Console.WriteLine("Hello World!");
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
                    var drawResult = new DrawResult
                    {
                        //Step 3 - Get the Date
                        DrawDate = lottoService.GetDrawDate(draw),

                        //Step 4 - Get the Jackpot 
                        Jackpot = lottoService.GetJackpot(draw)
                    };

                    //Step 5 - Get the Lotto numbers
                    //drawResult.IndividualDraw = await lottoService.GetLottoNumbers(draw);
                    var x = await lottoService.GetLottoNumbers(draw);

                    PrintDrawResults(drawResult);
                }
            }

        }

        private static void PrintDrawResults(DrawResult currentDraw)
        {
            Console.WriteLine($"DrawDate: {currentDraw.DrawDate}");
            Console.WriteLine($"JackPot: {currentDraw.Jackpot}");
            var lottoString = $"{currentDraw.IndividualDraw.LottoNumbers.BallOne},{currentDraw.IndividualDraw.LottoNumbers.BallTwo},{currentDraw.IndividualDraw.LottoNumbers.BallThree},{currentDraw.IndividualDraw.LottoNumbers.BallFour},{currentDraw.IndividualDraw.LottoNumbers.BallFive},{currentDraw.IndividualDraw.LottoNumbers.BallSix}";
           
            Console.WriteLine($"Lotto Numbers: {lottoString}   Bonus Ball Number: {currentDraw.IndividualDraw.BonusBallNumber}");
        }

    }


}
