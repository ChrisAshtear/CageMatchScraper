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
        public ScrapeStatus scrapestatus = ScrapeStatus.missing;

        public int objectID { get { return teamID; } set { teamID = value; } }
        public string Name { get { return GetName(); } }

        public bool IsSimpleTagTeam { get { return wrestlers.Count == 2; } }
        public Record objRecord { get { return record; } set { record = value; } }

        public TagTeam()
        {
            record.isTeam = true;
        }

        public string GetName()
        {
            if (name != "" && name != null) { return name; }
            else
            {
                string names = "";
                foreach(Wrestler w in wrestlers)
                {
                    names += w.name + "&";
                }
                names = names.TrimEnd('&');
                return names;
            }
        }

        public string POSTdata()
        {
            return $"name={name}&team_id={teamID}&stable={Convert.ToInt16(isStable)}";
        }

        public string POSTrecord(RecordType rec)
        {
            return $"worker_id={this.teamID}&division={wrestlers[0].gender}&score={record.self.GlickoRating}&record_type={rec.ToString().ToLower()}&rating_deviation={record.self.GlickoRatingDeviation}&rating={record.self.Rating}&wins={record.winCount}&losses={record.lossCount}&draws={record.draws}";
        }

        public string POSTstatus()
        {
            return $"obj_id={teamID}&table=teams&status={scrapestatus}";
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

            MultipartFormDataContent formRecord = ins.POSTtoFormData(POSTrecord(RecordType.Tag));
            HttpResponseMessage res = ins.SendFormData(API.apiCall.ADDWORKER_RECORD, formRecord);
            Console.WriteLine(res.Content.ToString());

            return true;
        }

        public override int GetHashCode()
        {
            int hashcode = 42;
            int multp = 0;
            foreach(Wrestler w in wrestlers)
            {
                multp += w.wrestlerID;
            }
            hashcode *= multp;
            return hashcode;
        }

        public override bool Equals(object other)
        {
            return other is TagTeam p && (p.GetHashCode()).Equals(GetHashCode());
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
