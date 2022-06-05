// See https://aka.ms/new-console-template for more information
using CageMatchScraper;

Scraper s = new Scraper();

//List<string> list = s.ParseHtml(response);
//var response2 = Scraper.GetWrestler(16006).Result;
var response2 = Scraper.GetEntry(RequestType.PROMOTION,2287);
Dictionary<string, string> data = s.ParseEntry (response2, "InformationBoxTitle", "InformationBoxContents");
WrestlingPromotion fed = new WrestlingPromotion(data, 2287);

var response3 = Scraper.GetEntry(RequestType.PROMOTION, 2287, PageType.RESULTS);
EventResults data2 = s.ParseList(response3,fed.fed_id,new TagInfo { htmlElement="div", className="QuickResults"}, new TagInfo { className= "QuickResultsHeader", htmlElement="div"}, new TagInfo { className = "MatchResults", htmlElement = "span" });

SendData send = new SendData("http://localhost:3001");
fed.sendData(send);
/*foreach(WrestlingEvent evt in data2.events.Values)
{
    evt.sendData(send);
}
foreach(Wrestler w in data2.wrestlers)
{
    w.sendData(send);
}*/
foreach(TagTeam team in data2.tags)
{
    team.sendData(send);
}