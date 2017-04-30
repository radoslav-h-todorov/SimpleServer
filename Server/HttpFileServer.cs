using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace Server
{
    public class HttpFileServer : IDisposable
    {
        private readonly string _rootPath;
        private const int BufferSize = 1024 * 512; //512KB
        private HttpListener _http;
        private readonly Thread _serverThread;

        public HttpFileServer(string rootPath, int port, bool sslEnabled)
        {
            _rootPath = rootPath;
            _serverThread = new Thread(() => Listen(port, sslEnabled));
            _serverThread.Start();
        }

        private void Listen(int port, bool sslEnabled)
        {
            var serverUrl = BuildServerUrl(port, sslEnabled);

            _http = new HttpListener();
            _http.Prefixes.Add(serverUrl);
            _http.Start();
            _http.BeginGetContext(RequestWait, null);

            Console.WriteLine("Server started: " + serverUrl + " @ " + _rootPath);
        }

        public void Dispose()
        {
            _serverThread.Abort();
            _http.Stop();
        }

        private void RequestWait(IAsyncResult ar)
        {
            if (!_http.IsListening)
            {
                return;
            }

            HttpListenerContext context = _http.EndGetContext(ar);
            _http.BeginGetContext(RequestWait, null);

            if (context.Request.HttpMethod == "POST")
            {
                string file = SaveFile(context.Request.ContentEncoding, GetBoundary(context.Request.ContentType), context.Request.InputStream);

                Console.WriteLine("Upload file: " + file);
                Return200(context);
            }
            else
            {
                string url = TuneUrl(context.Request.RawUrl);

                Console.WriteLine("Reqeust file: " + url);

                string fullPath = string.IsNullOrEmpty(url) ? _rootPath : Path.Combine(_rootPath, url);

                if (Directory.Exists(fullPath))
                {
                    ReturnDirContents(context, fullPath);
                }
                else if (File.Exists(fullPath))
                {
                    ReturnFile(context, fullPath);
                }
                else
                {
                    Return404(context);
                }
            }
        }

        private string SaveFile(Encoding enc, String boundary, Stream input)
        {
            Byte[] boundaryBytes = enc.GetBytes(boundary);
            Int32 boundaryLen = boundaryBytes.Length;
            Byte[] buffer = new Byte[1024];
            Int32 len = input.Read(buffer, 0, 1024);
            Int32 startPos;

            // Find start boundary
            while (true)
            {
                if (len == 0)
                {
                    throw new Exception("Start Boundaray Not Found");
                }

                startPos = IndexOf(buffer, len, boundaryBytes);
                if (startPos >= 0)
                {
                    break;
                }
                else
                {
                    Array.Copy(buffer, len - boundaryLen, buffer, 0, boundaryLen);
                    len = input.Read(buffer, boundaryLen, 1024 - boundaryLen);
                }
            }

            // Skip four lines (Boundary, Content-Disposition, Content-Type, and a blank)
            for (Int32 i = 0; i < 4; i++)
            {
                while (true)
                {
                    if (len == 0)
                    {
                        throw new Exception("Preamble not Found.");
                    }

                    startPos = Array.IndexOf(buffer, enc.GetBytes("\n")[0], startPos);
                    if (startPos >= 0)
                    {
                        startPos++;
                        break;
                    }
                    else
                    {
                        len = input.Read(buffer, 0, 1024);
                    }
                }
            }

            // Analyze the headers to get the original filename
            byte[] headersBytes = buffer.Take(startPos).ToArray();
            string[] headersText = enc.GetString(headersBytes).Split(new[] { '\r', '\n' });
            string fileName = "file";
            Regex re = new Regex(@"filename="".*""");
            foreach (string line in headersText)
            {
                Match filenameMatch = re.Match(line);

                if (filenameMatch.Success)
                {
                    fileName = filenameMatch.Value.Trim().Substring(10).TrimEnd('"').TrimEnd(';');
                    break;
                }
            }
            string fullPath = Path.Combine(_rootPath, DateTime.Now.ToUniversalTime().ToString("yyyyMMddHHmmss") + "-" + Path.GetFileName(fileName));
            
            // Wrrite the file
            Array.Copy(buffer, startPos, buffer, 0, len - startPos);
            len = len - startPos;
            using (FileStream output = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
            {
                while (true)
                {
                    Int32 endPos = IndexOf(buffer, len, boundaryBytes);
                    if (endPos >= 0)
                    {
                        if (endPos > 0) output.Write(buffer, 0, endPos - 2);
                        break;
                    }
                    else if (len <= boundaryLen)
                    {
                        throw new Exception("End Boundaray Not Found");
                    }
                    else
                    {
                        output.Write(buffer, 0, len - boundaryLen);
                        Array.Copy(buffer, len - boundaryLen, buffer, 0, boundaryLen);
                        len = input.Read(buffer, boundaryLen, 1024 - boundaryLen) + boundaryLen;
                    }
                }
            }

            return fullPath;
        }

        private static String GetBoundary(String ctype)
        {
            return "--" + ctype.Split(';')[1].Split('=')[1];
        }

        private static Int32 IndexOf(Byte[] buffer, Int32 len, Byte[] boundaryBytes)
        {
            for (Int32 i = 0; i <= len - boundaryBytes.Length; i++)
            {
                Boolean match = true;
                for (Int32 j = 0; j < boundaryBytes.Length && match; j++)
                {
                    match = buffer[i + j] == boundaryBytes[j];
                }

                if (match)
                {
                    return i;
                }
            }

            return -1;
        }

        private void ReturnDirContents(HttpListenerContext context, string dirPath)
        {
            context.Response.ContentType = "text/html";
            context.Response.ContentEncoding = Encoding.UTF8;
            using (var sw = new StreamWriter(context.Response.OutputStream))
            {
                sw.WriteLine("<html>");
                sw.WriteLine("<head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head>");
                sw.WriteLine("<body><ul>");

                string[] dirs = Directory.GetDirectories(dirPath);
                foreach (var d in dirs)
                {
                    var link = d.Replace(_rootPath, "").Replace('\\', '/');
                    sw.WriteLine("<li>&lt;DIR&gt; <a href=\"" + link + "\">" + Path.GetFileName(d) + "</a></li>");
                }

                string[] files = Directory.GetFiles(dirPath);
                foreach (var f in files)
                {
                    var link = f.Replace(_rootPath, "").Replace('\\', '/');
                    sw.WriteLine("<li><a href=\"" + link + "\">" + Path.GetFileName(f) + "</a></li>");
                }

                sw.WriteLine("</ul></body></html>");
            }
            context.Response.OutputStream.Close();
        }

        private static void ReturnFile(HttpListenerContext context, string filePath)
        {
            context.Response.ContentType = Converter.GetcontentType(Path.GetExtension(filePath));
            var buffer = new byte[BufferSize];
            using (var fs = File.OpenRead(filePath))
            {
                context.Response.ContentLength64 = fs.Length;
                int read;
                while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    context.Response.OutputStream.Write(buffer, 0, read);
                }
            }

            context.Response.OutputStream.Close();
        }

        private static void Return500(HttpListenerContext context, Exception exception)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "text/html";
            context.Response.ContentEncoding = Encoding.UTF8;

            using (StreamWriter sw = new StreamWriter(context.Response.OutputStream, Encoding.UTF8))
            {
                sw.WriteLine("<html>");
                sw.WriteLine("<head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head>");
                sw.WriteLine("<body>");
                sw.WriteLine(exception.ToString());
                sw.WriteLine("</body></html>");
            }

            context.Response.OutputStream.Close();
        }

        private static void Return404(HttpListenerContext context)
        {
            context.Response.StatusCode = 404;
            context.Response.Close();
        }

        private static void Return200(HttpListenerContext context)
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/html";
            context.Response.ContentEncoding = Encoding.UTF8;
            using (StreamWriter sw = new StreamWriter(context.Response.OutputStream, Encoding.UTF8))
            {
                sw.WriteLine("<html>");
                sw.WriteLine("<head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head>");
                sw.WriteLine("<body>");
                sw.WriteLine("File Uploaded");
                sw.WriteLine("</body></html>");
            }

            context.Response.Close();
        }

        private static string TuneUrl(string url)
        {
            url = url.Replace('/', '\\');
            url = HttpUtility.UrlDecode(url, Encoding.UTF8);
            url = url.Substring(1);
            return url;
        }

        private static string BuildServerUrl(int port, bool sslEnabled)
        {
            return sslEnabled ? "https" : "http" + "://localhost:" + port + "/";
        }
    }
}
