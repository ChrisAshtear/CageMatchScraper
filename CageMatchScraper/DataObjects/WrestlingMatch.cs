using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CageMatchScraper.DataObjects
{
    public class WrestlingMatch : Object, IWebDataOut
    {
        public List<List<Wrestler>> sidesWrestlers = new List<List<Wrestler>>();
        public List<List<Wrestler>> sidesManagers = new List<List<Wrestler>>();
        public List<List<TagTeam>> sidesTeams = new List<List<TagTeam>>();
        public List<List<TagTeam>> sidesPureTeams = new List<List<TagTeam>>();
        public string length;
        public int victor = 0;
        public string data;
        public string textdesc;
        public string parseddesc;
        public string title;
        public string result = "Normal";
        public int fed_id;
        public int event_id;
        public RecordType matchType;
        public Division division;
        public ScrapeStatus scrapestatus = ScrapeStatus.missing;

        public bool VerifyScrape()
        {
            string test = "";
            for (int i = 0; i < sidesWrestlers.Count; i++)
            {
                int participantCtr = 0;
                bool singlesMemberBeforeTag = false;
                foreach (Wrestler wrestler in sidesWrestlers[i])
                {
                    bool tagMember = false;
                    foreach (TagTeam t in sidesTeams[i])
                    {
                        tagMember = t.IsMember(wrestler);
                    }
                    if (tagMember) { continue; }
                    string str = AddMemberToText(wrestler, participantCtr, sidesWrestlers[i].Count);
                    test += str + wrestler.name;
                    participantCtr++;
                    singlesMemberBeforeTag = true;
                }

                foreach (TagTeam t in sidesTeams[i])
                {
                    if (singlesMemberBeforeTag) { test += " & "; }
                    test += t.name + " (";
                    participantCtr = 0;
                    foreach (Wrestler w in t.wrestlers)
                    {
                        string str = AddMemberToText(w, participantCtr, t.wrestlers.Count);
                        test += str + w.name;
                        participantCtr++;
                    }
                    test += ")";
                }

                if (sidesManagers[i].Count > 0)
                {
                    test += " (w/";
                    participantCtr = 0;
                    foreach (Wrestler wrestler in sidesManagers[i])
                    {
                        string str = AddMemberToText(wrestler, participantCtr, sidesManagers[i].Count);
                        test += str + wrestler.name;
                        participantCtr++;
                    }
                    test += ")";
                }

                if (sidesWrestlers[0].Count < 2 && i != sidesWrestlers.Count - 1) { test += " defeats "; }
                else if (i != sidesWrestlers.Count - 1)
                {
                    test += " defeat ";
                }

            }
            if (length != null) { test += $" ({length})"; }
            parseddesc = test;
            if (test == textdesc) { return true; }
            else { return false; }
        }

        public void SetMatchType()
        {
            if (sidesWrestlers[0].Count > 3) { matchType = RecordType.Special; return; }
            matchType = (RecordType)sidesWrestlers[0].Count;
        }

        public void SetDivision()
        {
            if (sidesWrestlers[0].Count==0) { Console.WriteLine("Invalid match"); return; }
            if (sidesWrestlers[0][0].gender == "female") { division = Division.Women; return; }
            if (sidesWrestlers[0][0].gender == "male") { division = Division.Men; return; }
        }


        private string AddMemberToText(Wrestler w, int ctr, int length)
        {
            string str = "";
            if (ctr > 0)
            {
                str = ", ";
                if (ctr == length - 1)
                {
                    str = " & ";
                }
            }

            return str;
        }

        public string POSTdata()
        {
            return $"title={title}&fed_id={fed_id}&result={result}&length={length}&victor={victor}&event_id={event_id}";
        }

        public string POSTstatus(string matchID)
        {
            return $"obj_id={matchID}&table=matches&status={scrapestatus}";
        }

        public bool sendData(SendData ins)
        {
            string matchID = ins.sendData(API.apiCall.ADDMATCH, this).ToString();//return m_id
            for (int i = 0; i < sidesWrestlers.Count; i++)
            {
                foreach (Wrestler wrestler in sidesWrestlers[i])
                {
                    string postDat = wrestler.POSTdata();
                    postDat += $"&side={i}&match_id={matchID}&is_participant=1";
                    ins.sendData(API.apiCall.ADDPARTICIPANT, this, postDat);
                }
                foreach (Wrestler wrestler in sidesManagers[i])
                {
                    string postDat = wrestler.POSTdata();
                    postDat += $"&side={i}&match_id={matchID}&is_participant=0";
                    ins.sendData(API.apiCall.ADDPARTICIPANT, this, postDat);
                }
            }
            return true;
        }

        public override String ToString()
        {
            string sides = "";
            foreach (List<Wrestler> ws in sidesWrestlers)
            {
                foreach (Wrestler w in ws)
                {
                    sides = w.name + ",";
                }
                sides.TrimEnd(',');
                sides += " vs. ";
            }
            return $"{title} : {sides})";
        }
    }

}
