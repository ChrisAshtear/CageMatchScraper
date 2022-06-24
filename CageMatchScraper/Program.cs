// See https://aka.ms/new-console-template for more information
using CageMatchScraper;

Scraper s = new Scraper();

//List<string> list = s.ParseHtml(response);
//var response2 = Scraper.GetWrestler(16006).Result;
var response2 = Scraper.GetEntry(RequestType.PROMOTION,2287);
Dictionary<string, string> data = s.ParseEntry (response2, "InformationBoxTitle", "InformationBoxContents");
WrestlingPromotion fed = new WrestlingPromotion(data, 2287);

var response3 = Scraper.GetEntry(RequestType.PROMOTION, 2287, PageType.RESULTS);
response3+= Scraper.GetEntry(RequestType.PROMOTION, 2287, PageType.RESULTS,1);
response3+= Scraper.GetEntry(RequestType.PROMOTION, 2287, PageType.RESULTS,2);
EventResults data2 = s.ParseList(response3,fed.fed_id,new TagInfo { htmlElement="div", className="QuickResults"}, new TagInfo { className= "QuickResultsHeader", htmlElement="div"}, new TagInfo { className = "MatchResults", htmlElement = "span" });

SendData send = new SendData("http://localhost:3001");
fed.sendData(send);
foreach(WrestlingEvent evt in data2.events.Values)
{
    evt.sendData(send);
}

foreach(Wrestler w in data2.wrestlers)
{
    var response4 = Scraper.GetEntry(RequestType.WRESTLER, w.wrestlerID, PageType.OVERVIEW);
    Dictionary<string, string> wdata = s.ParseEntry(response4, "InformationBoxTitle", "InformationBoxContents");
    w.stats = wdata;
    wdata.TryGetValue("Weight", out w.weight);
    wdata.TryGetValue("Height", out w.height);
    wdata.TryGetValue("Backgound in sports", out w.sportsBG);
    wdata.TryGetValue("Beginning of in-ring career", out w.inringstart);
    wdata.TryGetValue("In-ring experience", out w.experience);
    wdata.TryGetValue("Trainer", out w.trainer);
    wdata.TryGetValue("Nicknames", out w.nicknames);
    wdata.TryGetValue("Signature moves", out w.finisher);
    if(wdata.ContainsKey("Age"))
    {
        int.TryParse(wdata["Age"].Split(' ')[0], out w.age);
    }

    //DateTime debut;
    //need to support other date formats like day.month.year
    //bool gotDate = DateTime.TryParse(wdata["Beginning of in-ring career"], out debut);
    //if (!gotDate) { int.TryParse(wdata["Beginning of in-ring career"],out int year); debut = new DateTime(year, 1, 1); }
    //w.debut = debut;
    wdata.TryGetValue("Gender", out w.gender);
    wdata.TryGetValue("Birthplace", out w.birthplace);
    w.picture = s.GetImg(w.name.Replace(' ', '_'));

    w.sendData(send);
}
Rank r = new Rank();
r.startRank(data2);

foreach(TagTeam team in data2.tags)
{
    team.sendData(send);
}