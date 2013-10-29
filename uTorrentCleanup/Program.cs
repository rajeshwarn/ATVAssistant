using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace uTorrentCleanup
{
    /// <summary>
    /// This utility is meant to be called by uTorrent after a torrent 
    /// changes state.  It will see if the given torrent has finished, and if
    /// it has it will remove both the torrent and the data
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            //  Parse commandline options
            Options options = new Options();
            if(CommandLine.Parser.Default.ParseArguments(args, options))
            {
                //  If the torrent is finished, then we cleanup.  
                //  Otherwise, just ignore it.
                if(options.TorrentState == (int)TorrentState.Finished)
                { 
                    //  Get the location of the uTorrent web API (including user/password)
                    string baseUrl = ConfigurationManager.AppSettings["uTorrentAPIUrl"];
                    string user = ConfigurationManager.AppSettings["uTorrentUser"];
                    string password = ConfigurationManager.AppSettings["uTorrentPass"];

                    //  Call and get the token
                    CookieAwareWebClient webClient = new CookieAwareWebClient() { Credentials = new NetworkCredential(user, password) };
                    Uri tokenUri = new Uri(new Uri(baseUrl), "token.html");
                    string apiToken = webClient.DownloadString(tokenUri);
                    apiToken = apiToken.Split('>')[2].Split('<')[0];

                    //  Call the API to remove the torrent & data for the given torrent hash
                    Uri operationUri = new Uri(new Uri(baseUrl), string.Format("?action=removedata&token={0}&hash={1}", apiToken, options.TorrentHash));
                    string operationResult = webClient.DownloadString(operationUri);
                }
            }
        }
    }

    /// <summary>
    /// Cookie aware web client
    /// </summary>
    public class CookieAwareWebClient : WebClient
    {
        private readonly CookieContainer m_container = new CookieContainer();

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            HttpWebRequest webRequest = request as HttpWebRequest;
            if(webRequest != null)
            {
                webRequest.CookieContainer = m_container;
            }
            return request;
        }
    }
}
