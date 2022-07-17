using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CageMatchScraper
{
    public static class API
    {
        public enum apiCall
        {
            ADDEVENT = 0,
            ADDFED = 1,
            ADDMATCH = 2,
            ADDPARTICIPANT = 3,
            ADDWORKER = 4,
            ADDTEAM = 5,
            ADDTEAM_MEMBER = 6,
            ADDWORKER_RECORD = 7,
            ADDIMAGE = 8,
            ADDTITLE = 9,
            ADDTITLE_REIGN=10,
            GETSCRAPESTATUS=11,
            SETSCRAPESTATUS=12,
        }

        private static string[] apicalls = {
            "addevent",
            "addfed",
            "addmatch",
            "addmatchparticipant",
            "addworker",
            "addteam",
            "addteammember",
            "addrecord",
            "addimage",
            "addtitle",
            "addtitlereign",
            "getscrapestatus",
            "setscrapestatus"
        };

        public static string Call(apiCall calltype)
        {
            return apicalls[(int)calltype];
        }
    }

}
