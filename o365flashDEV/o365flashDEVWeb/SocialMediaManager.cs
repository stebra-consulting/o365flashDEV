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
            string linkedinSharesEndPoint = "https://api.linkedin.com/v1/companies/10355329/shares?oauth2_access_token={0}";

            string accessToken = "AQWVdIHVNPUnyLHE4mqsGgcoJnauh0ChrATeq7iesnW4WrABtQC_2vRE2o6i3NBd61Zj1BST8yX2xuTyaFs33o07T - 9OmVEVeLiRWIj3xQ - 6JBzMsYJW9D45Uq2safJJJhBSKVDjoqKGFRnda0W5TZ6qEClnA2iaONmIACBmF - cpRKsvtn8&format=json HTTP / 1.1";//"AQVfZEE04LluteLtvO06zY91Olv3RZIEjOS9FR4Ue93HimNhm_uj3mvhvoCUrOFDvxFp5S2HIibGDq0Ls4_ljeDW1z387O413uJbMuYCtnrV - 2fxF2C_POu55FZaB5qDtiIPncqxAIrXuEcF8BRJiexHOuLYwDlPGHOUcLSYtNUl0sE7Kw0&format=json HTTP/1.1";

            var requestUrl = String.Format(linkedinSharesEndPoint, accessToken);
            var message = new
            {
                comment = "Testing out the posting on LinkedIn",
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

            }


            var response = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(responseJson);
            return response.ContainsKey("updateKey");
        }
    }

}