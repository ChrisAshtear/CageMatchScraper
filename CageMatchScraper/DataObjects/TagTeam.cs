using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CageMatchScraper.DataObjects
{
    public class TagTeam : Object, IWebDataOut,I_Competitor
    {
        public string name;
        public int teamID;
        public List<Wrestler> wrestlers = new List<Wrestler>();
        public bool isStable = false;
        public Record record = new Record();

        public int objectID { get { return teamID; } set { teamID = value; } }
        public string Name { get { return name; } }

        public Record objRecord { get { return record; } set { record = value; } }

        public TagTeam()
        {
            record.isTeam = true;
        }

        public string POSTdata()
        {
            return $"name={name}&team_id={teamID}&stable={Convert.ToInt16(isStable)}";
        }

        public bool sendData(SendData ins)
        {
            string matchID = ins.sendData(API.apiCall.ADDTEAM, this).ToString();//return m_id
            foreach (Wrestler wrestler in wrestlers)
            {
                string postDat = wrestler.POSTdata();
                postDat += $"&team_id={teamID}&current_member=1";
                ins.sendData(API.apiCall.ADDTEAM_MEMBER, this, postDat);
            }
            return true;
        }

        public override String ToString()
        {
            string members = "";
            foreach (Wrestler w in wrestlers)
            {
                members += w.name + ",";
            }
            members.TrimEnd(',');
            return $"{name} : {members})";
        }

        public bool IsMember(Wrestler w)
        {
            if (wrestlers.FindIndex(o => o.wrestlerID == w.wrestlerID) != -1)
            {
                return true;
            }
            return false;
        }
    }

}
