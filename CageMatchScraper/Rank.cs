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

            //runRank(glickoRanksTag, e.events,RankType.tag);
            glickoRanksTag.Clear();
            foreach (TagTeam t in e.pureTags)
            {
                if(glickoRanksTag.ContainsKey(t.teamID))
                {
                    glickoRanksTag.Add(t.GetHashCode(), t);
                }
                else
                {
                    glickoRanksTag.Add(t.teamID, t);
                }
            }

            runRank(glickoRanksTag, e.events, RankType.puretag);

        }

        public enum RankType { singles, tag, puretag };

        public void runRank(Dictionary<int,I_Competitor> competitors, Dictionary<string,WrestlingEvent> events, RankType rtype = RankType.singles)
        {
            foreach (WrestlingEvent evt in events.Values.Reverse())
            {
                foreach (WrestlingMatch m in evt.matches)
                {
                    List<List<I_Competitor>> sides = new List<List<I_Competitor>>();
                    if (rtype == RankType.singles)
                    {
                        foreach (List<Wrestler> s in m.sidesWrestlers)
                        {
                            sides.Add(s.ToList<I_Competitor>());
                        }
                        if (m.sidesWrestlers[0].Count != 1 || m.sidesWrestlers[1].Count != 1) { continue; }
                    }
                    else if (rtype == RankType.tag)
                    {
                        foreach (List<TagTeam> s in m.sidesTeams)
                        {
                            sides.Add(s.ToList<I_Competitor>());
                        }
                        if (m.sidesWrestlers[0].Count != 2 || m.sidesWrestlers[1].Count != 2) { continue; }
                    }
                    else if (rtype == RankType.puretag)
                    {
                        foreach (List<TagTeam> s in m.sidesPureTeams)
                        {
                            sides.Add(s.ToList<I_Competitor>());
                        }
                        if(m.sidesWrestlers[0].Count !=2 || m.sidesWrestlers[1].Count != 2) { continue; }
                    }
                    if (sides[0].Count == 0) { continue; }
                    
                    if(m.sidesWrestlers[0].Count != m.sidesWrestlers[1].Count) { continue; }//uneven sides. handicap matches shouldnt count, and potentially error on scrape.
                    if (sides.Count > 4) { continue; }//count everything over as a battle royal.
                    List<I_Competitor> opponents = new List<I_Competitor>();
                    MatchResult result = MatchResult.Lose;
                    bool inMatch = false;
                    if (sides[0].Count != 1) { continue; }
                    for (int i = 0; i < sides.Count; i++)
                    {
                        if (sides[i].Count == 0) { continue; }
                        List<I_Competitor> wrestlers = sides[i];
                        I_Competitor w = sides[i][0];
                        if (w.objectID == 0) { continue; }
                        if (competitors.ContainsKey(w.objectID))
                        {
                            w = competitors[w.objectID];
                        }
                        else if(competitors.ContainsKey(w.GetHashCode()))
                        {
                            w = competitors[w.GetHashCode()];
                        }
                        else { Console.WriteLine("Cant find :" + w); continue; }

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
                if (rec.self.RatingDeviation < 400)//193 has a lot of results??
                {
                    Console.WriteLine($"{w.Name}: Wins:{rec.winCount},Losses:{rec.lossCount},Draws:{rec.draws} Glicko: {rec.self.GlickoRating}, Rating:{rec.self.Rating} - dev: {rec.self.RatingDeviation}");
                }
            }
        }

    }

    
}
