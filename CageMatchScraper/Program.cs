﻿// See https://aka.ms/new-console-template for more information
using CageMatchScraper;

Scraper s = new Scraper();

//List<string> list = s.ParseHtml(response);
//var response2 = Scraper.GetWrestler(16006).Result;
var response2 = Scraper.GetEntry(RequestType.PROMOTION,2287);
Dictionary<string, string> data = s.ParseEntry (response2, "InformationBoxTitle", "InformationBoxContents");
var response3 = Scraper.GetEntry(RequestType.PROMOTION, 2287, PageType.RESULTS);
Dictionary<string, string> data2 = s.ParseList(response3,new TagInfo { htmlElement="div", className="QuickResults"}, new TagInfo { className= "QuickResultsHeader", htmlElement="div"}, new TagInfo { className = "MatchResults", htmlElement = "span" });
Console.WriteLine(response2);