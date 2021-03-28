using System;
using System.Collections.Generic;
using System.Text;

namespace Lotto.Datamodels
{
    public class DrawResult
    {
        public DateTime? DrawDate { get; set; }
        public string Jackpot { get; set; }
        public LottoNumbers IndividualDraw { get; set; }
        public Strike Strike { get; set; }        
        public int PowerBallNumber { get; set; }

        public DrawResult()
        { 
            
        }
    }
}
