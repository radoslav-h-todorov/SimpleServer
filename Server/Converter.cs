namespace Server
{
    public class Converter
    {
        public static string GetcontentType(string extension)
        {
            switch (extension)
            {
                case ".avi": return "video/x-msvideo";
                case ".css": return "text/css";
                case ".doc": return "application/msword";
                case ".gif": return "image/gif";
                case ".htm": return "text/html";
                case ".html": return "text/html";
                case ".jpg": return "image/jpeg";
                case ".jpeg": return "image/jpeg";
                case ".js": return "application/x-javascript";
                case ".mp3": return "audio/mpeg";
                case ".png": return "image/png";
                case ".pdf": return "application/pdf";
                case ".ppt": return "application/vnd.ms-powerpoint";
                case ".zip": return "application/zip";
                case ".txt": return "text/plain";
                case ".asf": return "video/x-ms-asf";
                case ".asx": return "video/x-ms-asf";
                case ".bin": return "application/octet-stream";
                case ".cco": return "application/x-cocoa";
                case ".crt": return "application/x-x509-ca-cert";
                case ".deb": return "application/octet-stream";
                case ".der": return "application/x-x509-ca-cert";
                case ".dll": return "application/octet-stream";
                case ".dmg": return "application/octet-stream";
                case ".ear": return "application/java-archive";
                case ".eot": return "application/octet-stream";
                case ".exe": return "application/octet-stream";
                case ".flv": return "video/x-flv";
                case ".hqx": return "application/mac-binhex40";
                case ".htc": return "text/x-component";
                case ".ico": return "image/x-icon";
                case ".img": return "application/octet-stream";
                case ".iso": return "application/octet-stream";
                case ".jar": return "application/java-archive";
                case ".jardiff": return "application/x-java-archive-diff";
                case ".jng": return "image/x-jng";
                case ".jnlp": return "application/x-java-jnlp-file";
                case ".mml": return "text/mathml";
                case ".mng": return "video/x-mng";
                case ".mov": return "video/quicktime";
                case ".mpeg": return "video/mpeg";
                case ".mpg": return "video/mpeg";
                case ".msi": return "application/octet-stream";
                case ".msm": return "application/octet-stream";
                case ".msp": return "application/octet-stream";
                case ".pdb": return "application/x-pilot";
                case ".pem": return "application/x-x509-ca-cert";
                case ".pl": return "application/x-perl";
                case ".pm": return "application/x-perl";
                case ".prc": return "application/x-pilot";
                case ".ra": return "audio/x-realaudio";
                case ".rar": return "application/x-rar-compressed";
                case ".rpm": return "application/x-redhat-package-manager";
                case ".rss": return "text/xml";
                case ".run": return "application/x-makeself";
                case ".sea": return "application/x-sea";
                case ".shtml": return "text/html";
                case ".sit": return "application/x-stuffit";
                case ".swf": return "application/x-shockwave-flash";
                case ".tcl": return "application/x-tcl";
                case ".tk": return "application/x-tcl";
                case ".war": return "application/java-archive";
                case ".wbmp": return "image/vnd.wap.wbmp";
                case ".wmv": return "video/x-ms-wmv";
                case ".xml": return "text/xml";
                case ".xpi": return "application/x-xpinstall";
                default: return "application/octet-stream";
            }
        }
    }
}
