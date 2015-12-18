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

            //Quickfix illegal chars "åäö" to htmlencoding
            title = title.Replace("å", "&aring;");
            title = title.Replace("Å", "&Aring;");
            title = title.Replace("ä", "&auml;");
            title = title.Replace("Ä", "&Auml;");
            title = title.Replace("ö", "&ouml;");
            title = title.Replace("Ö", "&Ouml;");

            string linkedinSharesEndPoint = "https://api.linkedin.com/v1/companies/10355329/shares?oauth2_access_token={0}";

            string accessToken = "AQXmrLhp2cUsaax3QtHE7k5YtSxMgyTAhzba-5aFYvREhVp7kvm4FxfkWVM_0_EFGGeZk6GryWDqCGdHbEnDfxSnuqschsQnGE5VSWYRi67rkLm-yhnpJSJXGdPhP6pp2k6VU5x6FZiK75E4u08RedrBcnyL61mF6Rubf6G7mQcSb10CFcQ&format=json HTTP / 1.1";

            var requestUrl = String.Format(linkedinSharesEndPoint, accessToken);
            var message = new
            {
                comment = "Apputveckling",
                content = new Dictionary<string, string>
        { { "title", title },
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
    }

}