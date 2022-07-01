using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CageMatchScraper.DataObjects
{
    public class TitleReign
    {
        public DateOnly reignStart;
        public DateOnly reignEnd;
        public int holder_id;
    }

    public class Title
    {
        public string name;
        public int title_id;
        public TitleReign currentReign;
        public List<TitleReign> reigns;
        public int promotion_id;
        public RecordType title_type;
    }
}
