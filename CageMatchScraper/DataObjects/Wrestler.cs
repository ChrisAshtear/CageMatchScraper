using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CageMatchScraper.DataObjects
{
    public class Wrestler : Object, IWebDataOut,I_Competitor
    {
        public string name;
        public int wrestlerID;
        public Dictionary<RecordType, Record> record = new Dictionary<RecordType, Record>();
        public Dictionary<string, string> stats = new Dictionary<string, string>();
        public int age;
        public string birthplace;
        public string gender;
        public string height;
        public string weight;
        public string sportsBG;
        public string inringstart;
        public string experience;
        public string style;
        public string nicknames;
        public string trainer;
        public string finisher;
        public DateTime debut;
        public byte[] picture;

        public int objectID { get { return wrestlerID; }  set { wrestlerID = value; } }
        public string Name { get { return name; }}

        public Record objRecord { get { return record[RecordType.Singles]; } set { record[RecordType.Singles] = value; } }

        public string POSTdata()
        {
            //byte array needs to be built, just doing a tostring results in System[byte] or something not useful
            return $"name={name}&worker_id={wrestlerID}";
        }

        public string POSTdataAll()
        {
            //byte array needs to be built, just doing a tostring results in System[byte] or something not useful
            return $"name={name}&worker_id={wrestlerID}&birthplace={birthplace}&style={style}&nicknames={nicknames}&sportsBG={sportsBG}&experience={experience}&inringstart={inringstart}&trainer={trainer}&finisher={finisher}&height={height}&weight={weight}&age={age}&gender={gender}";
        }

        public string POSTrecord(RecordType rec)
        {
            Record record = this.record[rec];
            return $"worker_id={this.wrestlerID}&division={gender}&score={record.self.GlickoRating}&record_type={rec.ToString().ToLower()}&rating_deviation={record.self.GlickoRatingDeviation}&rating={record.self.Rating}&wins={record.winCount}&losses={record.lossCount}&draws={record.draws}";
        }

        public string POSTpic()
        {
            return $"obj_id={wrestlerID}&objtype=worker&pictype=fullbody";
        }

        public bool sendData(SendData ins)
        {
            //ins.sendData(API.apiCall.ADDWORKER, this,"",picture);
            MultipartFormDataContent form = ins.POSTtoFormData(POSTdataAll());
            HttpResponseMessage res = ins.SendFormData(API.apiCall.ADDWORKER, form);
            Console.WriteLine(res.Content.ToString());

            MultipartFormDataContent formPic = ins.POSTtoFormData(POSTpic());

            if (this.picture == null)
            {
                this.picture = new byte[4];
            }
            ByteArrayContent pic = new ByteArrayContent(picture, 0, picture.Length);
            pic.Headers.ContentType = MediaTypeHeaderValue.Parse("plain/text");

            formPic.Add(pic, "picdata");
            res = ins.SendFormData(API.apiCall.ADDIMAGE, formPic);
            Console.WriteLine(res.Content.ToString());

            MultipartFormDataContent formRecord = ins.POSTtoFormData(POSTrecord(RecordType.Singles));
            res = ins.SendFormData(API.apiCall.ADDWORKER_RECORD, formRecord);
            Console.WriteLine(res.Content.ToString());
            //send picture
            //send data
            //send record.


            return true;
        }

        //support nonparticipant.
        public override String ToString()
        {
            return $"{name}";
        }
    }

}
