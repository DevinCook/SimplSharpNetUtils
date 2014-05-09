/* HTTPGetter.cs
 *
 * Copy a file via HTTP to a local directory
 * 
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.Net.Http;

namespace SimplSharpNetUtils
{
    public class HttpGetter
    {
        String _ErrorMsg = "No Error";

        public String ErrorMsg
        {
            get { return _ErrorMsg; }
        }

        public HttpGetter() { }

        private void checkPath(String filename)
        {
            String dir = Path.GetDirectoryName(filename);
            if (!Directory.Exists(dir))
                Directory.Create(dir);
        }

        public uint Fetch(String url, String filename)
        {
            uint sz;

            try
            {
                checkPath(filename);
                HttpClient client = new HttpClient();
                int result = client.FgetFile(url, filename);
                if (result != 0)
                {
                    _ErrorMsg = "Transfer failed - " + result.ToString();
                    return 0;
                }
            }
            catch (Exception e)
            {
                _ErrorMsg = e.Message;
                return 0;
            }

            _ErrorMsg = "No Error";

            FileInfo fi = new FileInfo(filename);
            sz = (uint)fi.Length;

            fi = null;

            return sz;
        }
    }
}