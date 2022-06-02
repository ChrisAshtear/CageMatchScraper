// See https://aka.ms/new-console-template for more information
using CageMatchScraper;

Scraper s = new Scraper();

//List<string> list = s.ParseHtml(response);
//var response2 = Scraper.GetWrestler(16006).Result;
var response2 = Scraper.GetEntry(RequestType.PROMOTION,2287);
Dictionary<string, string> data = s.ParseEntry (response2, "InformationBoxTitle", "InformationBoxContents");
Console.WriteLine(response2);