using HtmlAgilityPack;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;
using Glicko2;
using CageMatchScraper.DataObjects;

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
        public Dictionary<string, WrestlingEvent> events = new Dictionary<string, WrestlingEvent>();
        public void AddWrestlers(IEnumerable<Wrestler> wrestlers)
        {
            foreach (Wrestler w in wrestlers)
            {
                if (this.wrestlers.FindIndex(o => o.wrestlerID == w.wrestlerID) == -1)
                {
                    this.wrestlers.Add(w);
                }
            }

        }
        public void AddTags(IEnumerable<TagTeam> tagteams)//support teams with diff members.
        {
            foreach (TagTeam t in tagteams)
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
        OVERVIEW = -1,
        NEWS = 2,
        EVENTS = 4,
        EVENTSTATS = 19,
        RESULTS = 8,
        TITLES = 9,
        ROSTER = 15,
        ALLTIMEROSTER = 16,
        MATCHGUIDE = 7,
        WINLOSS = 17,
        PROMOS = 6,
        TOURNAMENTS = 11,
        RIVALRIES = 14,
        RATINGS = 98
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
            ADDTEAM_MEMBER = 6,
            ADDWORKER_RECORD = 7,
            ADDIMAGE = 8,
        }

        private static string[] apicalls = {
            "addevent",
            "addfed",
            "addmatch",
            "addmatchparticipant",
            "addworker",
            "addteam",
            "addteammember",
            "addrecord",
            "addimage"
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

    public class Scraper
    {
        private static string baseURL = "https://www.cagematch.net/?";// id=2&nr=16006";
        private static string imgURL = "https://prowrestling.fandom.com/wiki/";
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

        public static string GetEntry(RequestType type, int promotionID, PageType pageNum = PageType.OVERVIEW, int resultPage=0)
        {
            string rpg = "";
            if (resultPage > 0) { rpg = $"&s={resultPage * 100}"; }
            string url = baseURL + "id="+(int)type+"&nr=" + promotionID+rpg;
            if(pageNum >= 0) { url += "&page=" + (int)pageNum; }
            return CallUrl(url).Result;
        }

        public byte[] GetImg(string wrestlerName)
        {  
            try
            {
                string url = imgURL + wrestlerName;
                string result = CallUrl(url).Result;
                List<HtmlAgilityPack.HtmlNode> nodes = GetHtmlElements(result, "img", "pi-image-thumbnail");
                if (nodes.Count == 0) { return null; }
                string imgurl = Between(nodes[0].OuterHtml, "src=\"", "\"");
                var webClient = new WebClient();
                byte[] imageBytes = webClient.DownloadData(imgurl);
                return imageBytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                byte[] nullimg = new byte[4];
                return nullimg;
            }
        }

        public List<HtmlAgilityPack.HtmlNode> GetHtmlElements(string html, string elementName, string className)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            var names = htmlDoc.DocumentNode.Descendants(elementName)
                    .Where(node => node.GetAttributeValue("class", "").Contains(className)).ToList();
            return names;
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

        public List<Title> ParseTitle(string html)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            var rows = htmlDoc.DocumentNode.Descendants("tr")
                    .Where(node => node.GetAttributeValue("class", "").Contains("TRow")).ToList();

            Dictionary<string, string> data = new Dictionary<string, string>();
            List<Title> titles = new List<Title>();
            for (int i = 0; i < rows.Count; i++)
            {

                var entry = rows[i].Descendants("td").Where(node => node.GetAttributeValue("class", "").Contains("TCol")).ToList();
                Title t = new Title();
                TitleReign reign = new TitleReign();
                for (int x=0;x<entry.Count; x++)
                {
                   
                    if (x == 1) 
                    {
                        string field = entry[x].InnerHtml;
                        t.title_id = int.Parse(Between(field, "nr=", "\""));
                        t.name = Between(field, ">", "<"); 
                    }
                    if(x==2)
                    {
                        string field = entry[x].InnerHtml;
                        reign.holder_id = int.Parse(Between(field, "nr=", "&"));
                        
                    }
                    if(x==3)
                    {
                        string field = entry[x].InnerHtml;
                        
                    }
                }
                t.currentReign = reign;
                titles.Add(t);
            }

            return titles;

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
        public EventResults ParseEvents(string html, int fedID, TagInfo eventTag, TagInfo listHeader, TagInfo listEntry)
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
                    match.textdesc = value.InnerText;
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
                        TagTeam team = new TagTeam();
                        
                        string teamEntry = Between(side, "(", ")");
                        string otherMembers = side;
                        if (side.Contains("(") && teamEntry.Length > 5 && !teamEntry.Contains("w/"))// tag team, or match length if the entry is short(<5characters).
                        {
                            //add TagTeam name to a match description?
                            //string teamEntry = Between(side, "(", ")");
                            
                            team.name = Between(side, ">", "</a> (",true);
                            //matches with multiple teams dont parse right.
                            //redo this and eliminate items piece by piece.
                            int typeID = 0;
                            string src = Between(side, "<", "</a> (", true);
                            string text = Between(src, "id=", "&");
                            int.TryParse(text, out typeID);
                            int.TryParse(Between(src, "nr=", "&"), out team.teamID);

                            if (typeID == (int)RequestType.STABLE) { team.isStable = true; }

                            List<Wrestler> teamMembers = ParseParticipants(teamEntry);
                            participants.AddRange(teamMembers);
                            team.wrestlers.AddRange(teamMembers);
                            otherMembers = Between(side, ") &");
                            if(otherMembers == "") { otherMembers = Between(side, "), "); }
                            if(otherMembers == "") 
                            {
                                otherMembers = Between(side, "", "("); 
                            }
                            teams.Add(team);
                            
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
                        match.sidesTeams.Add(teams);
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
                    match.SetMatchType();
                    match.SetDivision();
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
                    bool failedScrape = !match.VerifyScrape();
                    if (failedScrape) { Console.WriteLine("Failed Scrape: " + match.parseddesc); }
                }
                Console.WriteLine(evt);
                if(!events.events.ContainsKey(evt.name))
                {
                    events.events.Add(evt.name, evt);
                }
                
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
