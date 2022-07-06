using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CageMatchScraper.DataObjects
{
    public interface I_Competitor
    {
        public string Name { get; }
        public int objectID { get; set; }

        public Record objRecord { get; set; }
    }
}
