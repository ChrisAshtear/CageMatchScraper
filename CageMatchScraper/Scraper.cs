using HtmlAgilityPack;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;



namespace CageMatchScraper
{
    public interface IWebDataOut
    {
        public string POSTdata();

        public bool sendData(SendData ins);
    }
    public class EventResults
    {
        public List<Wrestler> wrestlers = new List<Wrestler>();
        public List<TagTeam> tags = new List<TagTeam>();
        public Dictionary<string,WrestlingEvent> events = new Dictionary<string,WrestlingEvent>();
        public void AddWrestlers(IEnumerable<Wrestler> wrestlers)
        {
            foreach(Wrestler w in wrestlers)
            {
                if (this.wrestlers.FindIndex(o=> o.wrestlerID == w.wrestlerID) == -1)
                {
                    this.wrestlers.Add(w);
                }
            }
            
        }
        public void AddTags(IEnumerable<TagTeam> tagteams)//support teams with diff members.
        {
            foreach(TagTeam t in tagteams)
            {
                if (tags.FindIndex(o => o.teamID == t.teamID) == -1)
                {
                    tags.Add(t);
                }
            }
        }
    }

    public enum RequestType
    {
        EVENT = 1,
        WRESTLER = 2,
        PROMOTION = 8,
        TITLE = 5,
        TAGTEAM = 28,
        STABLE = 29
    }
    
    public enum PageType//These are pages specific to promotion. Wrestler pages are different. 4 is Matches, Tournaments is 16.
    {
        OVERVIEW=-1,
        NEWS=2,
        EVENTS=4,
        EVENTSTATS=19,
        RESULTS=8,
        TITLES=9,
        ROSTER=15,
        ALLTIMEROSTER=16,
        MATCHGUIDE=7,
        WINLOSS=17,
        PROMOS=6,
        TOURNAMENTS=11,
        RIVALRIES=14,
        RATINGS=98
    }

    

    public static class API
    {
        public enum apiCall
        {
            ADDEVENT = 0,
            ADDFED = 1,
            ADDMATCH = 2,
            ADDPARTICIPANT = 3,
            ADDWORKER = 4,
            ADDTEAM = 5,
            ADDTEAM_MEMBER = 6
        }

        private static string[] apicalls = { 
            "addevent",
            "addfed",
            "addmatch",
            "addmatchparticipant",
            "addworker",
            "addteam",
            "addteammember"
        };

        public static string Call(apiCall calltype)
        {
            return apicalls[(int)calltype];
        }
    }


    public struct TagInfo
    {
        public string htmlElement;
        public string className;
    }

    public class TagTeam : Object , IWebDataOut
    {
        public string name;
        public int teamID;
        public List<Wrestler> wrestlers = new List<Wrestler>();
        public bool isStable = false;

        public string POSTdata()
        {
            return $"name={name}&team_id={teamID}&stable={Convert.ToInt16(isStable)}";
        }

        public bool sendData(SendData ins)
        {
            string matchID = ins.sendData(API.apiCall.ADDTEAM, this).ToString();//return m_id
            foreach (Wrestler wrestler in wrestlers)
            {
                string postDat = wrestler.POSTdata();
                postDat += $"&team_id={teamID}&current_member=1";
                ins.sendData(API.apiCall.ADDTEAM_MEMBER, this, postDat);
            }
            return true;
        }

        public override String ToString()
        {
            string members="";
            foreach (Wrestler w in wrestlers)
            {
                members += w.name + ",";
            }
            members.TrimEnd(',');
            return $"{name} : {members})";
        }
    }

    public class Wrestler : Object,IWebDataOut
    {
        public string name;
        public int wrestlerID;

        public string POSTdata()
        {
            return $"name={name}&worker_id={wrestlerID}";
        }

        public bool sendData(SendData ins)
        {
            ins.sendData(API.apiCall.ADDWORKER, this);
            return true;
        }

        //support nonparticipant.
        public override String ToString()
        {
            return $"{name}";
        }
    }

    public class WrestlingMatch : Object,IWebDataOut
    {
        public List<List<Wrestler>> sidesWrestlers = new List<List<Wrestler>>();
        public List<List<Wrestler>> sidesManagers = new List<List<Wrestler>>();
        public List<List<TagTeam>> sidesTeams = new List<List<TagTeam>>();
        public string length;
        public int victor = 0;
        public string data;
        public string title;
        public string result = "Normal";
        public int fed_id;
        public int event_id;

        public string POSTdata()
        {
            return $"title={title}&fed_id={fed_id}&result={result}&length={length}&victor={victor}&event_id={event_id}";
        }

        public bool sendData(SendData ins)
        {
            string matchID = ins.sendData(API.apiCall.ADDMATCH, this).ToString();//return m_id
            for(int i=0;i<sidesWrestlers.Count;i++)
            {
                foreach (Wrestler wrestler in sidesWrestlers[i])
                {
                    string postDat = wrestler.POSTdata();
                    postDat += $"&side={i}&is_participant=1";
                    ins.sendData(API.apiCall.ADDPARTICIPANT, this, postDat);
                }
                foreach (Wrestler wrestler in sidesManagers[i])
                {
                    string postDat = wrestler.POSTdata();
                    postDat += $"&side={i}&is_participant=0";
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
                foreach(Wrestler w in ws)
                {
                    sides = w.name + ",";
                }
                sides.TrimEnd(',');
                sides += " vs. ";
            }
            return $"{title} : {sides})";
        }
    }

    public class WrestlingEvent : Object, IWebDataOut
    {
        public string name;
        public string location;
        public string date;
        public string arena;
        public int eventID;
        public int fed_id;

        public List<WrestlingMatch> matches = new List<WrestlingMatch>();

        public string POSTdata()
        {
            return $"e_id={eventID}&name={name}&fed_id={fed_id}&date={date}&arena={arena}&location={location}";
        }

        public bool sendData(SendData ins)
        {
            ins.sendData(API.apiCall.ADDEVENT, this);
            foreach(WrestlingMatch match in matches)
            {
                match.fed_id = fed_id;
                match.event_id = eventID; 
                match.sendData(ins);
            }
            return true;
        }

        public override String ToString()
        {
            return $"{name} : {date} @ {location}";
        }
    }

    public class WrestlingPromotion : Object, IWebDataOut
    {
        public string name;
        public string initials;
        public int fed_id;
        public int formed;
        public string website;
        

        public string POSTdata()
        {
            return $"fed_id={fed_id}&name={name}&initials={initials}&site={website}";
        }

        public bool sendData(SendData ins)
        {
            ins.sendData(API.apiCall.ADDFED, this);
            return true;
        }

        public WrestlingPromotion(Dictionary<string,string> values, int id)
        {
            name = values["Current name"];
            initials = values["Current abbreviation"];
            fed_id = id;
            formed = int.Parse(values["Active Time"].Split('-')[0]);
        }
    }

    public class Scraper
    {
        private static string baseURL = "https://www.cagematch.net/?";// id=2&nr=16006";
        private string url;
        public Scraper()
        {
        }

        public static async Task<string> CallUrl(string fullUrl)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Host = new Uri(fullUrl).Host;
            var response = client.GetStringAsync(fullUrl);
            return await response;
        }

        public static string GetEntry(RequestType type, int promotionID, PageType pageNum = PageType.OVERVIEW)
        {
            string url = baseURL + "id="+(int)type+"&nr=" + promotionID;
            if(pageNum >= 0) { url += "&page=" + (int)pageNum; }
            return CallUrl(url).Result;
        }

        public Dictionary<string, string> ParseEntry(string html, string className, string classValue)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            var names = htmlDoc.DocumentNode.Descendants("div")
                    .Where(node => node.GetAttributeValue("class", "").Contains(className)).ToList();

            var values = htmlDoc.DocumentNode.Descendants("div")
                    .Where(node => node.GetAttributeValue("class", "").Contains(classValue)).ToList();


            Dictionary<string,string> data  = new Dictionary<string, string>();

            for(int i = 0; i < names.Count; i++)
            {
                var name = names[i];
                var value = values[i];
                data.Add(name.InnerHtml.Trim(':'), value.InnerHtml);
            }

            return data;

        }

        public string Between(string STR, string FirstString, string LastString="NONE",bool lastIndex = false)
        {
            string FinalString = "";
            try
            {
                int Pos1,Pos2; 
                if(lastIndex)
                {
                    
                    Pos2 = STR.LastIndexOf(LastString);
                    Pos1 = STR.Substring(0, Pos2).LastIndexOf(FirstString) + FirstString.Length;
                }
                else
                {
                    Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
                    Pos2 = STR.IndexOf(LastString, Pos1);
                }
                if (LastString == "NONE") { Pos2 = STR.Length; }
                if (Pos1 - FirstString.Length == -1 || Pos2 == -1) { return ""; }
                if (Pos2 - Pos1 < 0) { return ""; }
                FinalString = STR.Substring(Pos1, Pos2 - Pos1);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + STR);
                FinalString = "";
            }
            
            return FinalString;
        }
        //do unit test for parse list.
        public EventResults ParseList(string html, int fedID, TagInfo eventTag, TagInfo listHeader, TagInfo listEntry)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var eventEntry = htmlDoc.DocumentNode.Descendants(eventTag.htmlElement)
                    .Where(node => node.GetAttributeValue("class", "").Equals(eventTag.className)).ToList();

            EventResults events = new EventResults();

            foreach(var item in eventEntry)
            {
                var name = item.Descendants(listHeader.htmlElement)
                    .Where(node => node.GetAttributeValue("class", "").Contains(listHeader.className)).ToList();

                WrestlingEvent evt = new WrestlingEvent();
                evt.name = Between(name[0].InnerHtml,">","<");
                evt.date = Between(name[0].InnerHtml, "(", ")");
                evt.location = Between(name[0].InnerHtml, "@");
                evt.eventID = int.Parse(Between(name[0].InnerHtml, "nr=", "\""));
                evt.fed_id = fedID;
                var values = item.Descendants(listEntry.htmlElement)
                        .Where(node => node.GetAttributeValue("class", "").Contains(listEntry.className)).ToList();

                foreach(var value in values)
                {
                    WrestlingMatch match = new WrestlingMatch();
                    match.data = value.InnerHtml;
                    string[] nameandtime = value.InnerHtml.Split(':');
                    for(int i=0;i<nameandtime.Length;i++)
                    {
                        string str = nameandtime[i];
                        if(nameandtime.Length>1)
                        {
                            if(i<1 && nameandtime.Length>2){ match.title = str; }
                            else { match.length = Between(value.InnerHtml,"(",")",true); }
                        }
                    }
                    List<string> sides = value.InnerHtml.Split("defeat").ToList();
                    if (sides.Count < 2)
                    {
                        sides = value.InnerHtml.Split("vs.").ToList(); match.victor = -1;//no winner }//draw/dq
                        match.result = Between(value.InnerHtml, "- ", " (");//match result//no return if no time.
                    }

                    List<string> multiSideCheck = value.InnerHtml.Split(" and ").ToList();
                    if (multiSideCheck.Count > 1) 
                    {
                        multiSideCheck.RemoveAt(0);//remove duplicate sides already counted.
                        for (int i = 0; i < sides.Count;i++)// this extra side will be in the previous sides, remove.
                        {
                            int cutoff = sides[i].IndexOf(" and ");
                            if (cutoff != -1) { sides[i] = sides[i].Substring(0, cutoff); }
                        }
                        sides.AddRange(multiSideCheck.ToList()); 
                    }

                    foreach(string side in sides)
                    {
                        List<Wrestler> participants = new List<Wrestler>();
                        List<Wrestler> nonparticipants = new List<Wrestler>();
                        List<TagTeam> teams = new List<TagTeam>();

                        string teamEntry = Between(side, "(", ")");
                        string otherMembers = side;
                        if (side.Contains("(") && teamEntry.Length > 5 && !teamEntry.Contains("w/"))// tag team, or match length if the entry is short(<5characters).
                        {
                            //add TagTeam name to a match description?
                            //string teamEntry = Between(side, "(", ")");
                            TagTeam team = new TagTeam();
                            team.name = Between(side, ">", "</a> (",true);

                            List<Wrestler> teamMembers = ParseParticipants(teamEntry);
                            participants.AddRange(teamMembers);
                            team.wrestlers.AddRange(teamMembers);
                            otherMembers = Between(side, ") &");
                            if(otherMembers == "") { otherMembers = Between(side, "), "); }
                            if(otherMembers == "") 
                            {
                                otherMembers = Between(side, "", "("); 
                            }
                            int.TryParse(Between(side, "nr=", "&"), out team.teamID);
                            teams.Add(team);
                            match.sidesTeams.Add(teams);
                        }
                        string managers = Between(side, "(w/", ")");
                        if(managers.Length > 0) { nonparticipants = ParseParticipants(managers); }
                        if (otherMembers.Length >3)
                        {
                            List<Wrestler> singleMembers = ParseParticipants(otherMembers);
                            participants.AddRange(singleMembers);
                        }
                        
                        match.sidesManagers.Add(nonparticipants);
                        foreach(Wrestler w in nonparticipants)
                        {
                            Wrestler? f = participants.Find(x => x.wrestlerID == w.wrestlerID);
                            if (f != null)
                            {
                                participants.Remove(f);
                            }
                        }
                        match.sidesWrestlers.Add(participants);
                    }
                    //match title: <span class= "MatchType"> - doesnt exist for every match.
                    //only shows ENTERTAINMENT - maybe its not escaping the string for post.
                    //pro wrestling wiki for pictures.
                    //(19.01.2022) AEW Dark: Elevation #47 - TV-Show @ Entertainment & Sports Arena in Washington, District Of Columbia, USA
                    // 'Three Way:' title of match at beginning - watch out for the : in time
                    //DQ- "ROH World Tag Team Title: FTR (Cash Wheeler & Dax Harwood) (c) vs. Roppongi Vice (Rocky Romero & Trent Beretta) - Double DQ (10:25)"
                    //Robyn Renegade defeats Vicky Dreamboat (3:31)  - no links for wrestlers **** if a wrestler has no link they dont show up.
                    //Steel Cage Match (Special Referee: MJF): Wardlow defeats Shawn Spears (6:53)
                    //Three Way: Swerve Strickland defeats Jungle Boy and Ricky Starks (9:36)
                    evt.matches.Add(match);
                    foreach(List<Wrestler> w in match.sidesWrestlers)
                    {
                        events.AddWrestlers(w);
                    }
                    foreach (List<Wrestler> w in match.sidesManagers)
                    {
                        events.AddWrestlers(w);
                    }
                    foreach( List<TagTeam> teams in match.sidesTeams)
                    {
                        events.AddTags(teams);
                    }
                }
                Console.WriteLine(evt);
                events.events.Add(evt.name,evt);
            }
            
            return events;

        }
        List<Wrestler> ParseParticipants(string html)
        {
            string[] wrestlers = html.Split(new string[] { " & ", "," },StringSplitOptions.None);
            List<Wrestler> participants = new List<Wrestler>();
            foreach (string wrestler in wrestlers)
            {
                if(wrestler.Length == 0) { continue; }
                Wrestler w = new Wrestler();
                w.name = Between(wrestler, ">", "<");
                int.TryParse(Between(wrestler, "nr=", "&"),out w.wrestlerID);
                int.TryParse(Between(wrestler, "id=", "&"), out int typeID);
                if(typeID == (int)RequestType.TAGTEAM || typeID == (int)RequestType.STABLE) { continue; }//this is a team/stable not a wrestler.
                participants.Add(w);
            }
            return participants;
        }
    }

    
}
