using System;
using System.Collections.Generic;
using System.Text;

namespace Memory
{
    public class ScoreManager
    {
        public string Player { get; set; }

        public string Name1 { get; set; }
        public string Name2 { get; set; }
        public string Name3 { get; set; }
        public string Name4 { get; set; }
        public string Name5 { get; set; }

        public int Score1 { get; set; }
        public int Score2 { get; set; }
        public int Score3 { get; set; }
        public int Score4 { get; set; }
        public int Score5 { get; set; }

        public void addGameScore(int score, string name)
        {
            // decides what the top 5 highest scores are and 
            // adds them to the list.

            // do stuff here

            if (score < Score1)
            {
                Name5 = Name4;
                Name4 = Name3;
                Name3 = Name2;
                Name2 = Name1;
                Name1 = name;

                Score5 = Score4;
                Score4 = Score3;
                Score3 = Score2;
                Score2 = Score1;
                Score1 = score;
            }
            else if (score < Score2)
            {
                Name5 = Name4;
                Name4 = Name3;
                Name3 = Name2;
                Name2 = name;

                Score5 = Score4;
                Score4 = Score3;
                Score3 = Score2;
                Score2 = score;
            }
            else if (score < Score3)
            {
                Name5 = Name4;
                Name4 = Name3;
                Name3 = name;

                Score5 = Score4;
                Score4 = Score3;
                Score3 = score;
            }
            else if (score < Score4)
            {
                Name5 = Name4;
                Name4 = name;

                Score5 = Score4;
                Score4 = score;
            }
            else if (score < Score5)
            {
                Score5 = score;
                Name5 = name;
            } // if else

        } // addGameScore()
    }
}
