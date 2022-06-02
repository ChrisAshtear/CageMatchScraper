using HtmlAgilityPack;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;



namespace CageMatchScraper
{
    public enum RequestType
    {
        EVENT = 1,
        WRESTLER = 2,
        PROMOTION = 8,
        TITLE = 5,
        TAGTEAM = 28
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

    public struct TagInfo
    {
        public string htmlElement;
        public string className;
    }

    public class TagTeam
    {
        public string name;
        public int teamID;
        public List<Wrestler> wrestlers = new List<Wrestler>();
    }

    public class Wrestler
    {
        public string name;
        public int wrestlerID;
    }

    public class WrestlingMatch
    {
        public List<List<Wrestler>> sidesWrestlers = new List<List<Wrestler>>();
        public List<List<Wrestler>> sidesManagers = new List<List<Wrestler>>();
        public List<List<TagTeam>> sidesTeams = new List<List<TagTeam>>();
        public string length;
        public int victor = 0;
        public string data;
    }

    public class WrestlingEvent
    {
        public string name;
        public string location;
        public string date;
        public int eventID;

        public List<WrestlingMatch> matches = new List<WrestlingMatch>();
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

        public string Between(string STR, string FirstString, string LastString="NONE")
        {
            string FinalString = "";
            try
            {
                int Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
                int Pos2 = STR.IndexOf(LastString, Pos1);
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
        public Dictionary<string, string> ParseList(string html, TagInfo eventTag, TagInfo listHeader, TagInfo listEntry)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var eventEntry = htmlDoc.DocumentNode.Descendants(eventTag.htmlElement)
                    .Where(node => node.GetAttributeValue("class", "").Equals(eventTag.className)).ToList();

            Dictionary<string,WrestlingEvent> events = new Dictionary<string,WrestlingEvent>();

            foreach(var item in eventEntry)
            {
                var name = item.Descendants(listHeader.htmlElement)
                    .Where(node => node.GetAttributeValue("class", "").Contains(listHeader.className)).ToList();

                WrestlingEvent evt = new WrestlingEvent();
                evt.name = Between(name[0].InnerHtml,">","<");
                evt.date = Between(name[0].InnerHtml, "(", ")");
                evt.location = Between(name[0].InnerHtml, "@");
                evt.eventID = int.Parse(Between(name[0].InnerHtml, "nr=", "\""));

                var values = item.Descendants(listEntry.htmlElement)
                        .Where(node => node.GetAttributeValue("class", "").Contains(listEntry.className)).ToList();

                foreach(var value in values)
                {
                    WrestlingMatch match = new WrestlingMatch();
                    match.data = value.InnerHtml;
                    String[] sides = value.InnerHtml.Split("defeat");
                    
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
                            team.name = Between(side, ">", "<");
                            List<Wrestler> teamMembers = ParseParticipants(teamEntry);
                            participants.AddRange(teamMembers);
                            team.wrestlers.AddRange(teamMembers);
                            otherMembers = Between(side, ") &");
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
                    // 'and' for 3 ways / tag team 3 ways.
                    // 'Three Way:' title of match at beginning - watch out for the : in time
                    //DQ- "ROH World Tag Team Title: FTR (Cash Wheeler & Dax Harwood) (c) vs. Roppongi Vice (Rocky Romero & Trent Beretta) - Double DQ (10:25)"
                    //Robyn Renegade defeats Vicky Dreamboat (3:31)  - no links for wrestlers 
                    //Steel Cage Match (Special Referee: MJF): Wardlow defeats Shawn Spears (6:53)
                    //Three Way: Swerve Strickland defeats Jungle Boy and Ricky Starks (9:36)
                    evt.matches.Add(match);
                }
                events.Add(evt.name,evt);
            }
            //managers sides not matching
            Dictionary<string, string> data = new Dictionary<string, string>();
            /*
            for (int i = 0; i < names.Count; i++)
            {
                var name = names[i];
                var value = values[i];
                data.Add(name.InnerHtml.Trim(':'), value.InnerHtml);
            }*/

            return data;

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
                participants.Add(w);
            }
            return participants;
        }
    }

    
}
