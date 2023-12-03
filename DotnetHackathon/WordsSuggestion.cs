using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetHackathon
{
    public class WordsSuggestion
    {
        public string Word { get; set; }
        int difficulty;
        public int Difficulty { get { return difficulty; } set { difficulty=Math.Clamp(value, 1, 3); } }
        public bool Use { get;set; }
        public WordsSuggestion(string word,int difficulty, bool use) {
        Word= word;
            Difficulty= difficulty;
            Use= use;
        }
    }
}
