using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CageMatchScraper
{
    public class Rank
    {
        public void startRank(EventResults e)
        {
            Dictionary<int,Wrestler> glickoRanks = new Dictionary<int,Wrestler>();
            foreach(Wrestler w in e.wrestlers)
            {
                glickoRanks.Add(w.wrestlerID, w);
            }
            foreach (WrestlingEvent evt in e.events.Values.Reverse())
            {
                foreach(WrestlingMatch m in evt.matches)
                {
                    if (m.sidesWrestlers.Count > 2) { continue; }//triple threat or battle royal.
                    List<Wrestler> opponents = new List<Wrestler>();
                    bool won = false;
                    bool inMatch = false;
                    if(m.sidesWrestlers[0].Count !=1) { continue; }//skip multi-man matches for now.
                    for (int i = 0; i < m.sidesWrestlers.Count; i++)
                    {
                        if (m.sidesWrestlers[i].Count == 0) { continue; }
                        List<Wrestler> wrestlers = m.sidesWrestlers[i];
                        Wrestler w = m.sidesWrestlers[i][0];
                        if (w.wrestlerID == 0) { continue; }
                        w = glickoRanks[w.wrestlerID];
                        inMatch = true;
                        if (i == m.victor)
                        {
                            w.record.winCount++;
                            w.record.wins.Add(m);
                            won = true;
                            //win
                        }
                        else if(m.victor != -1)
                        {
                            w.record.lossCount++;
                            w.record.losses.Add(m);
                            won = false;
                            //loss
                        }
                        else
                        {
                            w.record.draws++;
                        }
                        opponents.Clear();
                        foreach (List<Wrestler> opp in m.sidesWrestlers)
                        {
                            if(m.sidesWrestlers[i] != opp && m.sidesWrestlers[i][0].wrestlerID != 0)
                            {
                                opponents.AddRange(opp);
                            }
                        }
                        w.record.AddResult(opponents, won);
                        glickoRanks[w.wrestlerID] = w;
                    }
                        
                }
                Console.WriteLine($"Rankings as of: {evt.name}");
                OutputRankings(e,ref glickoRanks);
            }
                
            Console.WriteLine($"Final Rankings with");
            OutputRankings(e, ref glickoRanks);
        }

        public void OutputRankings(EventResults e,ref Dictionary<int,Wrestler> rankings)
        {
            foreach (Wrestler w in e.wrestlers)
            {
                //w.record.self = new Glicko2.GlickoPlayer();
                List<Glicko2.GlickoOpponent> opponents = new List<Glicko2.GlickoOpponent>();
                foreach(RecordItem r in w.record.opponentsRank)
                {
                    opponents.Add(new Glicko2.GlickoOpponent(rankings[r.opponent.wrestlerID].record.self, Convert.ToInt16(r.win)));
                }
                w.record.self = Glicko2.GlickoCalculator.CalculateRanking(w.record.self, opponents);
                w.record.opponentsRank.Clear();//clear rankings
                if (w.record.self.RatingDeviation < 200)//193 has a lot of results??
                {
                    Console.WriteLine($"{w.name}: Wins:{w.record.winCount},Losses:{w.record.lossCount}, Glicko: {w.record.self.GlickoRating}, Rating:{w.record.self.Rating} - dev: {w.record.self.RatingDeviation}");
                }
            }
        }
    }

    
}
