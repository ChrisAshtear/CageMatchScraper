using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CageMatchScraper
{
    public class SendData
    {
        private string url;
        public SendData(string baseURL)
        {
            url = baseURL;
        }

        public string sendData(API.apiCall calltype, IWebDataOut webOut, string postDat="", byte[] extraData=null)
        {
            string address = url + "/" + API.Call(calltype);

            string postData = webOut.POSTdata();
            if(postDat != "") { postData = postDat; }
            Console.WriteLine("\n" + postData + "\n");
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            byte[] final = byteArray;
            if(extraData!=null)
            {
                final = byteArray.Concat(extraData).ToArray();
            }
            return sendWebData(address, byteArray);
        }

        public HttpResponseMessage SendFormData(API.apiCall calltype, MultipartFormDataContent formdata)
        {
            string address = url + "/" + API.Call(calltype);
            HttpClient client = new HttpClient();
            
            client.DefaultRequestHeaders.Host = new Uri(address).Host;
            var response = client.PostAsync(address, formdata);
            response.Wait();

            // Get the stream containing content returned by the server.  
            // The using block ensures the stream is automatically closed.
            // Open the stream using a StreamReader for easy access.  
            StreamReader reader = new StreamReader(response.Result.Content.ReadAsStream());
            // Read the content.  
            string responseFromServer = reader.ReadToEnd();
            // Display the content.  
            Console.WriteLine(responseFromServer);


            return response.Result;
        }

        private string sendWebData(string url, byte[] byteArray)
        {
            try
            {
                WebRequest request = WebRequest.Create(url);
                // Set the Method property of the request to POST.  
                request.Method = "POST";

                // Set the ContentType property of the WebRequest.  
                request.ContentType = "application/x-www-form-urlencoded";
                // Set the ContentLength property of the WebRequest.  
                request.ContentLength = byteArray.Length;
                request.Headers.Add("Content-Transfer-Encoding: base64");
                // Get the request stream.  
                Stream dataStream = request.GetRequestStream();
                // Write the data to the request stream.  
                dataStream.Write(byteArray, 0, byteArray.Length);
                // Close the Stream object.  
                dataStream.Close();

                // Get the response.  
                WebResponse response = request.GetResponse();
                // Display the status.  
                string responseFromServer;
                Console.WriteLine(((HttpWebResponse)response).StatusDescription);
                // Get the stream containing content returned by the server.  
                // The using block ensures the stream is automatically closed.
                using (dataStream = response.GetResponseStream())
                {
                    // Open the stream using a StreamReader for easy access.  
                    StreamReader reader = new StreamReader(dataStream);
                    // Read the content.  
                    responseFromServer = reader.ReadToEnd();
                    // Display the content.  
                    Console.WriteLine(responseFromServer);
                }

                // Close the response.  
                response.Close();
                return responseFromServer;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message + e.StackTrace + e.Source);
                return "error";
            }
        }
    }
}
