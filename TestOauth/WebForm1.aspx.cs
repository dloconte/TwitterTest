using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace TestOauth
{
    public partial class WebForm1 : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void SendTweet(object sender, EventArgs e)
        {
            string message = "Testing Programmatic Tweet #3. Come see new videos: https://youtube.com #ItWorks #jobs";

            string oAuthConsumerKey = ConfigurationManager.AppSettings["oAuthConsumerKey"];
            string oAuthConsumerSecret = ConfigurationManager.AppSettings["oAuthConsumerSecret"];
            string accessToken = ConfigurationManager.AppSettings["AccessToken"];
            string accessTokenSecret = ConfigurationManager.AppSettings["AccessTokenSecret"];
            string twitterUrl = "https://api.twitter.com/1.1/statuses/update.json";

            string signatureMethod = "HMAC-SHA1";
            string version = "1.0";
            string nonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
            string timestamp = GenerateTimeStamp(DateTime.Now);

            string baseFormat = "oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}" + "&oauth_timestamp={3}&oauth_token={4}&oauth_version={5}&status={6}";

            string baseString = String.Format(baseFormat, oAuthConsumerKey, nonce, signatureMethod, timestamp, accessToken, version, Uri.EscapeDataString(message));

            string oauth_signature = "";
            using (HMACSHA1 hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(Uri.EscapeDataString(oAuthConsumerSecret) + "&" + Uri.EscapeDataString(accessTokenSecret))))
            {
                oauth_signature = Convert.ToBase64String(hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes("POST&" + Uri.EscapeDataString(twitterUrl) + "&" + Uri.EscapeDataString(baseString))));
            }

            string authFormat = "OAuth oauth_consumer_key=\"{0}\", oauth_nonce=\"{1}\", oauth_signature=\"{2}\", oauth_signature_method=\"{3}\", " + "oauth_timestamp=\"{4}\", oauth_token=\"{5}\", " + "oauth_version=\"{6}\"";

            string authHeader = string.Format(authFormat, Uri.EscapeDataString(oAuthConsumerKey), Uri.EscapeDataString(nonce), Uri.EscapeDataString(oauth_signature), Uri.EscapeDataString(signatureMethod), Uri.EscapeDataString(timestamp), Uri.EscapeDataString(accessToken), Uri.EscapeDataString(version));

            HttpWebRequest authRequest = (HttpWebRequest)WebRequest.Create(twitterUrl);
            authRequest.Headers.Add("Authorization", authHeader);
            authRequest.Method = "POST";
            authRequest.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";

            using (Stream stream = authRequest.GetRequestStream())
            {
                byte[] content = ASCIIEncoding.ASCII.GetBytes("status=" + Uri.EscapeDataString(message));
                stream.Write(content, 0, content.Length);
            }

            var responseResult = "";

            try
            {
                WebResponse authResponse = authRequest.GetResponse();
                StreamReader reader = new StreamReader(authResponse.GetResponseStream());
                responseResult = reader.ReadToEnd().ToString();
                authResponse.Close();
                ResponseString.Text = "Tweet Successfully Sent! Tweet was: " + message;
            }
            catch (Exception ex)
            {
                responseResult = "Twitter Post Error: " + ex.Message.ToString() + ", authHeader: " + authHeader;
                ResponseString.Text = responseResult;
            }            
        }

        public string GenerateTimeStamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;

            return Convert.ToInt64(diff.TotalSeconds).ToString();
        }
    }
}