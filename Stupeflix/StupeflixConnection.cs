using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading;

namespace Stupeflix
{
    public class StupeflixConnection
    {
        protected static String HEADER_CONTENT_TYPE = "Content-Type";
        protected String host;
        protected String service;
        protected bool debug;
        protected Dictionary<string, string> responseHeaders = new Dictionary<string, string>();
        protected int responseCode = -1;
        protected byte[] bodyBytes;


        public StupeflixConnection(String host, String service, bool debug)
        {
            this.host = host;
            this.service = service;
            this.debug = debug;
        }


        public void execute(String method, String uri, String filename, String body, Dictionary<string, string> headers)
        {
            bool isSecure = false;
            var url = new Uri(string.Format("{0}://{1}:{2}/{3}", (isSecure ? "https" : "http"), this.host, 80, this.service + uri));

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;

            if (headers != null)
            {
                foreach (var itm in headers)
                {
                    if (!(itm.Key == "Content-Length" || itm.Key == "Content-Type"))
                        request.Headers.Add(itm.Key, itm.Value);
                }


                if (headers.ContainsKey("Content-Length"))
                    request.ContentLength = Convert.ToInt32(headers["Content-Length"]);
                if (headers.ContainsKey("Content-Type"))
                    request.ContentType = headers["Content-Type"];
            }

            if (request.Method == "POST" || request.Method == "PUT")
            {
                if (body != null)
                {
                    using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
                    {
                        writer.Write(body);
                        writer.Close();
                    }


                }
                else if (filename != null)
                {

                    using (FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                    {
                        byte[] buffer = new byte[4096];
                        int bytesRead = 0;
                        while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            request.GetRequestStream().Write(buffer, 0, bytesRead);
                        }
                        fileStream.Close();
                    }
                }

            }

            var response = (HttpWebResponse)request.GetResponse();
            foreach (var itm in response.Headers.AllKeys)
            {
                this.responseHeaders.Add(itm, response.Headers[itm]);
            }

            var resStrem = response.GetResponseStream();
            
            using (MemoryStream memStream = new MemoryStream())
            {
                byte[] buffer = new byte[2048];

                int bytesRead = 0;
                do
                {
                    bytesRead = resStrem.Read(buffer, 0, buffer.Length);
                    memStream.Write(buffer, 0, bytesRead);
                } while (bytesRead != 0);

                resStrem.Close();
                resStrem.Dispose();
                resStrem = null;

               

                bodyBytes = memStream.ToArray();
            }


            if (method == "GET" && filename != null)
            {
                File.WriteAllBytes(filename, bodyBytes);
            }

            responseCode = (int)response.StatusCode;
        }



        static byte[] readInputStream(Stream stream)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }

        }

        public int getResponseCode()
        {
            return this.responseCode;
        }

        public Dictionary<string, string> getResponseHeaders()
        {
            return responseHeaders;
        }

        public byte[] getResponseBodyBytes()
        {
            if (this.bodyBytes != null)
            {
                return this.bodyBytes;
            }
            else
            {
                return new byte[0];
            }
        }

    }
}
