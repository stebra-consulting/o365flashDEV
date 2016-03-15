using Facebook;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;

namespace o365flashDEVWeb
{
    public static class SocialMediaManager
    {
        public static int postCount = 0;
        public static void loginToLinkedIn()
        {
            var oauth2_Url = "https://www.linkedin.com/uas/oauth2/";
            var responseUrl = oauth2_Url + "authorization?response_type=code&";
            var client_id = "client_id=77e1h1reaupexq";
            var redirect_uri = "redirect_uri=https://o365flash.azurewebsites.net/home/linkedin";
#if DEBUG
            redirect_uri = "redirect_uri=https://localhost:44300/home/linkedin";
#endif

            var state = "state=234234de";
            var scope = "scope=r_basicprofile%20r_emailaddress%20rw_company_admin%20w_share";
            HttpContext.Current.Response.Redirect(responseUrl + client_id + "&" + redirect_uri + "&" + state + "&" + scope);
        }
        public static void liGetAccessToken()
        {
            if (HttpContext.Current.Request.QueryString["code"] != null)
            {
                string code = HttpContext.Current.Request.QueryString["code"].ToString();

                var oauth2_Url = "https://www.linkedin.com/uas/oauth2/";
                var responseUrl = oauth2_Url + "accessToken?";

                using (WebClient client = new WebClient())
                { 


                    var redirect_uri = "https://o365flash.azurewebsites.net/home/linkedin";
#if DEBUG
                    redirect_uri = "https://localhost:44300/home/linkedin";
#endif
                    byte[] response =
                    client.UploadValues(responseUrl, new NameValueCollection()
                    {
           { "grant_type", "authorization_code" },
           { "code", code },
           { "redirect_uri", redirect_uri },
           { "client_id", "77e1h1reaupexq" },
           { "client_secret", "XuPzNYv5zpgR99UV" }
                    });

                    var result = System.Text.Encoding.UTF8.GetString(response);
                    //{ "access_token":"AQXod6RHyu0dNhs0lrwydK-FBF0u_wlnRNPvgAvSM1y1utBFR6xU-v_RazENrcfGcsNS8ZAg1xBZuKKoaMlx8wqV3oyHR3KIBRPwFomqRskeXsBRbIPxsP3mJnuRlJZECQYL923iGom4fYjWmdlx27B92lcd53DyLoe3fosJa0F0FQ8HZcM","expires_in":5172521}

                    string[] words = result.Split('"');
                    var token = words[3];
                    HttpContext.Current.Session["AccessToken"] = token;
                }
                


            }
            else if (HttpContext.Current.Request.QueryString["error"] != null)
            {
                // Notify the user as you like
                string error = HttpContext.Current.Request.QueryString["error"];
                string errorResponse = HttpContext.Current.Request.QueryString["error_reason"];
                string errorDescription = HttpContext.Current.Request.QueryString["error_description"];


            }
        }


        public static bool PostToLinkedIn(string title, string submittedUrl, string submittedImageUrl)
        {

            postCount++;
            var comment = "Senaste nytt! " + postCount.ToString(); //postCount for dev purposes, can we post 2 post in same application call?

            string companyId = "2414183";
            string linkedinSharesEndPoint = "https://api.linkedin.com/v1/companies/" + companyId + "/shares?oauth2_access_token={0}";

            string accessToken = HttpContext.Current.Session["AccessToken"].ToString() + "&format=json HTTP / 1.1";

            var requestUrl = String.Format(linkedinSharesEndPoint, accessToken);
            var message = new
            {
                comment = comment,
                content = new Dictionary<string, string>
        {
            { "title", title },
            { "submitted-url", submittedUrl },
            {"submitted-image-url" , submittedImageUrl}
        },
                visibility = new
                {
                    code = "anyone"
                }
            };

            var requestJson = new JavaScriptSerializer().Serialize(message);

            var client = new WebClient();
            var requestHeaders = new NameValueCollection
    {
        { "Content-Type", "application/json" },
        { "x-li-format", "json" }
    };
            client.Headers.Add(requestHeaders);

            var responseJson = "";
            try
            {
                responseJson = client.UploadString(requestUrl, "POST", requestJson);
            }

            catch (WebException e)
            { //breakpoint here to troubleshoot 'e' object.
                //e.g 400 Bad Request
            }


            var response = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(responseJson);
            return response.ContainsKey("updateKey");
        }

















        //FACEBOOK
        public static void loginToFacebook()
        {
            var redirect_uri = "https://o365flash.azurewebsites.net/home/facebook";
#if DEBUG
            redirect_uri = "https://localhost:44300/home/facebook";
#endif
            var fb = new FacebookClient();
            var loginUrl = fb.GetLoginUrl(new
            {

                client_id = "1706235779661312",

                redirect_uri = redirect_uri,

                response_type = "code",

                scope = "email,user_likes,publish_actions,manage_pages, publish_pages" // Add other permissions as needed

            });
            HttpContext.Current.Response.Redirect(loginUrl.AbsoluteUri);  // User not connected, ask them to sign in again
        }

        public static void fbGetAccessToken()
        {
            if (HttpContext.Current.Request.QueryString["code"] != null)
            {
                string accessCode = HttpContext.Current.Request.QueryString["code"].ToString();

                var fb = new FacebookClient();
                var redirect_uri = "https://o365flash.azurewebsites.net/home/facebook";
#if DEBUG
                redirect_uri = "https://localhost:44300/home/facebook";
#endif
                // throws OAuthException 
                dynamic result = fb.Post("oauth/access_token", new
                {

                    client_id = "1706235779661312",

                    client_secret = "557e810bcb102d201f1f52d7a107785e",

                    redirect_uri = redirect_uri,
                    
                    code = accessCode

                });

                var accessToken = result.access_token;
                HttpContext.Current.Session["AccessToken"] = result.access_token;


            }
            else if (HttpContext.Current.Request.QueryString["error"] != null)
            {
                // Notify the user as you like
                string error = HttpContext.Current.Request.QueryString["error"];
                string errorResponse = HttpContext.Current.Request.QueryString["error_reason"];
                string errorDescription = HttpContext.Current.Request.QueryString["error_description"];


            }
        }

        public static bool postToFacebook(string title, string submittedUrl, string submittedImageUrl)
        {

            var fb = new FacebookClient();
            // update the facebook client with the access token 
            dynamic accessToken = HttpContext.Current.Session["AccessToken"];
            fb.AccessToken = accessToken;

            string pageAccessToken = "";
            JsonObject jsonResponse = fb.Get("me/accounts") as JsonObject;
            foreach (var account in (JsonArray)jsonResponse["data"])
            {
                string accountName = (string)(((JsonObject)account)["name"]);

                if (accountName == "stebra")
                {
                    pageAccessToken = (string)(((JsonObject)account)["access_token"]);
                    break;
                }
            }
            var client = new FacebookClient(pageAccessToken);
            Dictionary<string, object> fbParams = new Dictionary<string, object>();
            fbParams["message"] = "Senaste nytt!";

            fbParams["link"] = submittedUrl;
            fbParams["picture"] = submittedImageUrl;
            fbParams["name"] = title;
            fbParams["caption"] = "Stebra.se";
            fbParams["description"] = "​";

            var publishedResponse = client.Post("/stebraconsulting/feed", fbParams);
            if (publishedResponse != null)
            { return true; }
            else
            { return false; }
        }
    }

}