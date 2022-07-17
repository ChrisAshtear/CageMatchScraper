// See https://aka.ms/new-console-template for more information
using CageMatchScraper;
using CageMatchScraper.DataObjects;
SendData send = new SendData("http://localhost:3001");


Scraper s = new Scraper();
string stat = s.GetScrapeStatus(send);

var response2 = Scraper.GetEntry(RequestType.PROMOTION,2287);
Dictionary<string, string> data = s.ParseEntry (response2, "InformationBoxTitle", "InformationBoxContents");
WrestlingPromotion fed = new WrestlingPromotion(data, 2287);

var response3 = Scraper.GetEntry(RequestType.PROMOTION, 2287, PageType.RESULTS);
response3+= Scraper.GetEntry(RequestType.PROMOTION, 2287, PageType.RESULTS,1);
response3+= Scraper.GetEntry(RequestType.PROMOTION, 2287, PageType.RESULTS,2);
EventResults data2 = s.ParseEvents(response3,fed.fed_id,new TagInfo { htmlElement="div", className="QuickResults"}, new TagInfo { className= "QuickResultsHeader", htmlElement="div"}, new TagInfo { className = "MatchResults", htmlElement = "span" });
var response5 = Scraper.GetEntry(RequestType.PROMOTION, 2287, PageType.TITLES);
fed.titles = s.ParseTitle(response5,2287);



fed.sendData(send);
/*foreach(WrestlingEvent evt in data2.events.Values)
{
    evt.sendData(send);
}*/
Rank r = new Rank();
r.startRank(data2);



foreach (Wrestler w in data2.wrestlers)
{
    if (!s.needsScrape("workers", w.objectID)){ continue; }
    Console.WriteLine(w.name + " needs scrape");
    //Lookup scrapestatus here and if complete, skip.
    //maybe get a full array of all scrapestatus in memory before this

    var response4 = Scraper.GetEntry(RequestType.WRESTLER, w.wrestlerID, PageType.OVERVIEW);
    Dictionary<string, string> wdata = s.ParseEntry(response4, "InformationBoxTitle", "InformationBoxContents");
    w.stats = wdata;
    wdata.TryGetValue("Weight", out w.weight);
    wdata.TryGetValue("Height", out w.height);
    wdata.TryGetValue("Backgound in sports", out w.sportsBG);
    wdata.TryGetValue("Beginning of in-ring career", out w.inringstart);
    wdata.TryGetValue("In-ring experience", out w.experience);
    //wdata.TryGetValue("Trainer", out w.trainer);
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
    bool gotgender = wdata.TryGetValue("Gender", out w.gender);
    bool gotbirthp = wdata.TryGetValue("Birthplace", out w.birthplace);
    //w.picture = s.GetImg(w.name.Replace(' ', '_'));
    if(gotbirthp && gotgender) { w.scrapestatus = ScrapeStatus.complete; }
    if (w.wrestlerID != 0) { w.sendData(send); }
}


foreach(TagTeam team in data2.tags)
{
    team.sendData(send);
}