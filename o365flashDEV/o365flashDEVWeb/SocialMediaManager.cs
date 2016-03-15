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
        public static bool PostToLinkedIn(string title, string submittedUrl, string submittedImageUrl)
        {

            string companyId = "10355329";
            string linkedinSharesEndPoint = "https://api.linkedin.com/v1/companies/" + companyId + "/shares?oauth2_access_token={0}";

            string accessToken = "AQXmrLhp2cUsaax3QtHE7k5YtSxMgyTAhzba-5aFYvREhVp7kvm4FxfkWVM_0_EFGGeZk6GryWDqCGdHbEnDfxSnuqschsQnGE5VSWYRi67rkLm-yhnpJSJXGdPhP6pp2k6VU5x6FZiK75E4u08RedrBcnyL61mF6Rubf6G7mQcSb10CFcQ&format=json HTTP / 1.1";

            var requestUrl = String.Format(linkedinSharesEndPoint, accessToken);
            var message = new
            {
                comment = "Apputveckling",
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
        public static void loginToFacebook()
        {
            var fb = new FacebookClient();
            var loginUrl = fb.GetLoginUrl(new
            {

                client_id = "1706235779661312",

                redirect_uri = "https://o365flash.azurewebsites.net/home/redirect",
#if debug
                redirect_uri = "https://localhost:44300/home/redirect",
#endif
                response_type = "code",

                scope = "email,user_likes,publish_actions,manage_pages, publish_pages" // Add other permissions as needed

            });
            HttpContext.Current.Response.Redirect(loginUrl.AbsoluteUri);  // User not connected, ask them to sign in again
        }

        public static void getAccessToken()
        {
            if (HttpContext.Current.Request.QueryString["code"] != null)
            {
                string accessCode = HttpContext.Current.Request.QueryString["code"].ToString();

                var fb = new FacebookClient();

                // throws OAuthException 
                dynamic result = fb.Post("oauth/access_token", new
                {

                    client_id = "1706235779661312",

                    client_secret = "557e810bcb102d201f1f52d7a107785e",

                    redirect_uri = "https://o365flash.azurewebsites.net/home/redirect",
#if debug
                    redirect_uri = "https://localhost:44300/home/redirect",
#endif

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