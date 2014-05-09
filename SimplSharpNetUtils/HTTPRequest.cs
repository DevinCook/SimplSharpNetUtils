using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Net;
using Crestron.SimplSharp.Net.Http;

namespace SimplSharpNetUtils
{
    public class HTTPRequest
    {
        public String URL;
        public int Port = 80;
        public String User = "";
        public String Password = "";

        public delegate void errorHandler(SimplSharpString errMsg);
        public errorHandler OnError { get; set; }

        public delegate void responseHandler(SimplSharpString errMsg);
        public responseHandler OnResponse { get; set; }
        
        public int DoIt()
        {
            HttpClient client = new HttpClient();
            HttpClientRequest req = new HttpClientRequest();
            HttpClientResponse resp;

            try
            {
                client.KeepAlive = false;
                client.Port = Port;
                if (User.Length > 0)
                {
                    client.UserName = User;
                    client.Password = Password;
                }
                else
                {
                    client.UserName = "";
                    client.Password = "";
                }
                req.Url.Parse(URL);
                resp = client.Dispatch(req);

                if (OnResponse != null)
                    OnResponse(new SimplSharpString(resp.ContentString));
            }
            catch (Exception e)
            {
                if (OnError != null)
                    OnError(new SimplSharpString(e.ToString() + "\n\r" + e.StackTrace));

                return -1;
            }
       

            return 0;
        }
    }
}