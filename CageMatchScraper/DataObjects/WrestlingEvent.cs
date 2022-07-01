using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CageMatchScraper.DataObjects
{
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
            foreach (WrestlingMatch match in matches)
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

}
