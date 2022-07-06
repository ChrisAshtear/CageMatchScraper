using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CageMatchScraper.DataObjects
{
    public class TitleReign : Object, IWebDataOut
    {
        public DateTime reignStart;
        public DateTime reignEnd;
        public int holder_id;
        public int length;
        public int title_id;

        public string POSTdata()
        {
            //need reignend
            return $"holder_id={holder_id}&title_id={title_id}&length={length}&reignstart={reignStart.ToString("yyyy-MM-dd")}";
        }

        public bool sendData(SendData ins)
        {
            ins.sendData(API.apiCall.ADDTITLE_REIGN, this);
            return true;
        }
    }

    public class Title : Object, IWebDataOut
    {
        public string name;
        public int title_id;
        public TitleReign currentReign;
        public List<TitleReign> reigns = new List<TitleReign>();
        public int promotion_id;
        public RecordType title_type;
        public Division division;

        public string POSTdata()
        {
            return $"name={name}&title_id={title_id}&fed_id={promotion_id}&division={division}&type={title_type.ToString().ToLower()}";
        }

        public bool sendData(SendData ins)
        {
            ins.sendData(API.apiCall.ADDTITLE, this);
            foreach(TitleReign r in reigns)
            {
                r.title_id = title_id;
                r.sendData(ins);
            }
            return true;
        }
    }
}
