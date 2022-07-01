using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glicko2;

namespace CageMatchScraper.DataObjects
{

    public class Record
    {
        public int wrestlerID;
        public bool isTeam;
        public GlickoPlayer self = new GlickoPlayer();
        public List<WrestlingMatch> wins = new List<WrestlingMatch>();
        public List<WrestlingMatch> losses = new List<WrestlingMatch>();
        public List<RecordItem> opponentsRank = new List<RecordItem>();
        public int score;
        public int winCount;
        public int lossCount;
        public int draws;

        public void AddResult(List<Wrestler> opponents, bool win)
        {
            foreach (Wrestler w in opponents)
            {
                opponentsRank.Add(new RecordItem { opponent = w, win = win });
            }
        }
    }

    public class RecordItem
    {
        public Wrestler opponent;
        public bool win;
    }

    public enum RecordType { Singles = 1, Tag = 2, Trios = 3, Special = 5 };
    public enum Division { Men, Women, Mixed };

}
