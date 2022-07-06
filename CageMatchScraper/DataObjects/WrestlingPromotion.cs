using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CageMatchScraper.DataObjects
{

    public class WrestlingPromotion : Object, IWebDataOut
    {
        public string name;
        public string initials;
        public int fed_id;
        public int formed;
        public string website;
        public List<Title> titles;


        public string POSTdata()
        {
            return $"fed_id={fed_id}&name={name}&initials={initials}&website={website}";
        }

        public bool sendData(SendData ins)
        {
            ins.sendData(API.apiCall.ADDFED, this);

            foreach(Title t in titles)
            {
                t.sendData(ins);
            }
            return true;
        }

        public WrestlingPromotion(Dictionary<string, string> values, int id)
        {
            name = values["Current name"];
            initials = values["Current abbreviation"];
            fed_id = id;
            formed = int.Parse(values["Active Time"].Split('-')[0]);
        }
    }

}
