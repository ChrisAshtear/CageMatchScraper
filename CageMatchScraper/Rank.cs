using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CageMatchScraper.DataObjects;

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
                //tag teams should have their own collective record.
                w.record.Add(RecordType.Singles, new Record());
                w.record.Add(RecordType.Tag, new Record());
                w.record.Add(RecordType.Trios, new Record());
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
                            w.record[m.matchType].winCount++;
                            w.record[m.matchType].wins.Add(m);
                            won = true;
                            //win
                        }
                        else if(m.victor != -1)
                        {
                            w.record[m.matchType].lossCount++;
                            w.record[m.matchType].losses.Add(m);
                            won = false;
                            //loss
                        }
                        else
                        {
                            w.record[m.matchType].draws++;
                        }
                        opponents.Clear();
                        foreach (List<Wrestler> opp in m.sidesWrestlers)
                        {
                            if(m.sidesWrestlers[i] != opp && m.sidesWrestlers[i][0].wrestlerID != 0)
                            {
                                opponents.AddRange(opp);
                            }
                        }
                        w.record[m.matchType].AddResult(opponents, won);
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
                Record rec = w.record[RecordType.Singles];
                //w.record.self = new Glicko2.GlickoPlayer();
                List<Glicko2.GlickoOpponent> opponents = new List<Glicko2.GlickoOpponent>();
                foreach(RecordItem r in rec.opponentsRank)
                {
                    opponents.Add(new Glicko2.GlickoOpponent(rankings[r.opponent.wrestlerID].record[RecordType.Singles].self, Convert.ToInt16(r.win)));
                }
                rec.self = Glicko2.GlickoCalculator.CalculateRanking(rec.self, opponents);
                rec.opponentsRank.Clear();//clear rankings
                if (rec.self.RatingDeviation < 200)//193 has a lot of results??
                {
                    Console.WriteLine($"{w.name}: Wins:{rec.winCount},Losses:{rec.lossCount}, Glicko: {rec.self.GlickoRating}, Rating:{rec.self.Rating} - dev: {rec.self.RatingDeviation}");
                }
            }
        }
    }

    
}
