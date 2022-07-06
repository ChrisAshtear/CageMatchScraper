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
            Dictionary<int, I_Competitor> glickoRanks = new Dictionary<int,I_Competitor>();
            Dictionary<int,I_Competitor> glickoRanksTag = new Dictionary<int,I_Competitor>();
            foreach(Wrestler w in e.wrestlers)
            {
                glickoRanks.Add(w.wrestlerID, w);
                //tag teams should have their own collective record.
                w.record.Add(RecordType.Singles, new Record());
                w.record.Add(RecordType.Tag, new Record());
                w.record.Add(RecordType.Trios, new Record());
            }
            runRank(glickoRanks, e.events);

            foreach(TagTeam t in e.tags)
            {
                glickoRanksTag.Add(t.teamID, t);
            }

            runRank(glickoRanksTag, e.events,true);
        }

        public void runRank(Dictionary<int,I_Competitor> competitors, Dictionary<string,WrestlingEvent> events, bool isTag=false)
        {
            foreach (WrestlingEvent evt in events.Values.Reverse())
            {
                foreach (WrestlingMatch m in evt.matches)
                {
                    List<List<I_Competitor>> sides = new List<List<I_Competitor>>();
                    if(!isTag)
                    {
                        foreach (List<Wrestler> s in m.sidesWrestlers)
                        {
                            sides.Add(s.ToList<I_Competitor>());
                        }
                    }
                    else
                    {
                        foreach (List<TagTeam> s in m.sidesTeams)
                        {
                            sides.Add(s.ToList<I_Competitor>());
                        }
                    }
                    if (sides[0].Count == 0) { continue; }
                    if (sides.Count > 2) { continue; }//triple threat or battle royal.
                    List<I_Competitor> opponents = new List<I_Competitor>();
                    MatchResult result = MatchResult.Lose;
                    bool inMatch = false;
                    if (sides[0].Count != 1) { continue; }//skip multi-man matches for now.
                    for (int i = 0; i < sides.Count; i++)
                    {
                        if (sides[i].Count == 0) { continue; }
                        List<I_Competitor> wrestlers = sides[i];
                        I_Competitor w = sides[i][0];
                        if (w.objectID == 0) { continue; }
                        w = competitors[w.objectID];
                        inMatch = true;
                        if (i == m.victor)
                        {
                            w.objRecord.winCount++;
                            w.objRecord.wins.Add(m);
                            result = MatchResult.Win;
                            //win
                        }
                        else if (m.victor != -1)
                        {
                            w.objRecord.lossCount++;
                            w.objRecord.losses.Add(m);
                            result = MatchResult.Lose;
                            //loss
                        }
                        else
                        {
                            result = MatchResult.Draw;
                            w.objRecord.draws++;
                        }
                        opponents.Clear();
                        foreach (List<Wrestler> opp in m.sidesWrestlers)
                        {

                            if (m.sidesWrestlers[i] != opp && sides[i][0].objectID != 0)
                            {
                                opponents.AddRange(opp);
                            }
                        }
                        w.objRecord.AddResult(opponents, result);
                        competitors[w.objectID] = w;
                    }

                }
                Console.WriteLine($"Rankings as of: {evt.name}");
                OutputRankings(competitors.Values.ToList(), ref competitors);
            }
            Console.WriteLine($"Final Rankings with");
            OutputRankings(competitors.Values.ToList(), ref competitors);
        }

        public void OutputRankings(List<I_Competitor> competitors, ref Dictionary<int, I_Competitor> rankings)
        {
            foreach (I_Competitor w in competitors)
            {
                Record rec = w.objRecord;
                //w.record.self = new Glicko2.GlickoPlayer();
                List<Glicko2.GlickoOpponent> opponents = new List<Glicko2.GlickoOpponent>();
                foreach (RecordItem r in rec.opponentsRank)
                {
                    if(rankings.ContainsKey(r.opponent.objectID))
                    { 
                        opponents.Add(new Glicko2.GlickoOpponent(rankings[r.opponent.objectID].objRecord.self, (double)Convert.ToInt16(r.result)/2)); 
                    }
                    else { Console.WriteLine("Couldnt find " + r.opponent.objectID); }
                }
                rec.self = Glicko2.GlickoCalculator.CalculateRanking(rec.self, opponents);
                rec.opponentsRank.Clear();//clear rankings
                if (rec.self.RatingDeviation < 200)//193 has a lot of results??
                {
                    Console.WriteLine($"{w.Name}: Wins:{rec.winCount},Losses:{rec.lossCount},Draws:{rec.draws} Glicko: {rec.self.GlickoRating}, Rating:{rec.self.Rating} - dev: {rec.self.RatingDeviation}");
                }
            }
        }

    }

    
}
