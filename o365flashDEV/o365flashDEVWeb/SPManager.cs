﻿using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using o365flashDEVWeb.Models;

namespace o365flashDEVWeb
{
    public static class SPManager
    {
        //It is required to set this property in order to use SPManager
        public static HttpContextBase CurrentHttpContext { get; set; }
        public static SharePointContext cachedSpContext { get; set; }

        public static ListItemCollection GetItemsFromGuid(string listGuid)
        {

            ListItemCollection items = null;

            var spContext = SharePointContextProvider.Current.GetSharePointContext(CurrentHttpContext);
            using (var clientContext = spContext.CreateUserClientContextForSPHost())
            {
                if (clientContext != null)
                {
                    listGuid = listGuid.Replace("{", "").Replace("}", "");
                    Guid guid = new Guid(listGuid);

                    List list = clientContext.Web.Lists.GetById(guid);
                    clientContext.Load(list);
                    clientContext.ExecuteQuery();

                    CamlQuery camlQuery = new CamlQuery();
                    camlQuery.ViewXml = @"
                                                <View>
                                                    <Query>
                                                        <Where>
                                                            <IsNotNull>
                                                                <FieldRef Name='Title' />
                                                            </IsNotNull>
                                                        </Where>
                                                    </Query>
                                                </View>";
                    items = list.GetItems(camlQuery);

                    clientContext.Load(items);
                    clientContext.ExecuteQuery();
                }
            }
            return items;
        }
        public static ListItemCollection GetItemsFromListName(string listName)
        {
            ListItemCollection items = null;
            var spContext = SharePointContextProvider.Current.GetSharePointContext(CurrentHttpContext);
            using (var clientContext = spContext.CreateUserClientContextForSPHost())
            {
                if (clientContext != null)
                {
                    List list = clientContext.Web.Lists.GetByTitle(listName);
                    clientContext.Load(list);
                    clientContext.ExecuteQuery();

                    CamlQuery camlQuery = new CamlQuery();
                    camlQuery.ViewXml = @"
                                        <View>
                                            <Query>
                                                <Where>
                                                    <IsNotNull>
                                                        <FieldRef Name='Title' />
                                                    </IsNotNull>
                                                </Where>
                                            </Query>
                                        </View>";
                    items = list.GetItems(camlQuery);

                    clientContext.Load(items);
                    clientContext.ExecuteQuery();
                }
            }
            return items;
        }

        public static System.IO.Stream ImportImage(string fileLeafRef)
        {
            System.IO.Stream stream = null;

            var spContext = SharePointContextProvider.Current.GetSharePointContext(CurrentHttpContext);

            using (var clientContext = spContext.CreateUserClientContextForSPHost())
            {
                if (clientContext != null)
                {
                    List photoList = clientContext.Web.GetList("/sites/SD1/SiteAssets/");

                    CamlQuery camlQuery = new CamlQuery();
                    camlQuery.ViewXml = @"<View Scope='Recursive'>
                                            <Query>
                                                <Where>
                                                    <Eq>
                                                      <FieldRef Name='FileLeafRef'></FieldRef>
                                                      <Value Type='Text'>" + fileLeafRef + @"</Value>
                                                     </Eq>
                                                 </Where>
                                             </Query>
                                         </View>";
                    ListItemCollection photoCollection = photoList.GetItems(camlQuery);

                    clientContext.Load(photoCollection);

                    clientContext.ExecuteQuery();

                    if (photoCollection.LongCount() == 0)
                    {
                        //the image is public
                        return null;
                    }

                    ListItem item = photoCollection.ElementAt(0);

                    File file = item.File;

                    ClientResult<System.IO.Stream> data = file.OpenBinaryStream();

                    clientContext.Load(file);

                    clientContext.ExecuteQuery();

                    stream = data.Value;

                    //Wait for stream to get Executed by SharePoint
                    bool isLong = false;
                    while (isLong == false) { isLong = data.Value.Length is long; }
                    //Wait for stream to get Executed by SharePoint

                }
                return stream;
            }

        }

        public static void ToSocialMedia(List<StebraEntity> stebraList, string socialMediaName)
        {
            //ListItemCollection items = null;

            //var spContext = SharePointContextProvider.Current.GetSharePointContext(CurrentHttpContext);

            if (cachedSpContext == null) { cachedSpContext = SharePointContextProvider.Current.GetSharePointContext(CurrentHttpContext); goto Start; }
            else goto Skip;
            Start:
            using (var clientContext = cachedSpContext.CreateUserClientContextForSPHost())
            {
                if (clientContext != null)
                {
                    ListItemCollection items = null;
                    List list = clientContext.Web.Lists.GetByTitle("Nyhetslista");
                    clientContext.Load(list);
                    clientContext.ExecuteQuery();

                    CamlQuery camlQuery = new CamlQuery();
                    camlQuery.ViewXml = @"
                                            <View>
                                                <Query>
                                                    <Where>
                                                        <IsNotNull>
                                                           <FieldRef Name='Publicera' />
                                                        </IsNotNull>
                                                    </Where>
                                                </Query>
                                            </View>";
                    items = list.GetItems(camlQuery);

                    clientContext.Load(items);

                    clientContext.ExecuteQuery();

                    foreach (var item in items)
                    {
                        foreach (var entity in stebraList)
                        {
                            if (item.Id == entity.SPListItemID)
                            {
                                clientContext.Load(item);

                                var publiceradArray = (string[])(item["Publicerad"]);

                                if (publiceradArray != null)
                                {
                                    bool publicerad = publiceradArray.Contains("Publicerad till " + socialMediaName);

                                    var publiceraArray = (string[])(item["Publicera"]);
                                    bool publicera = publiceraArray.Contains("Publicera till " + socialMediaName);

                                    if (publicerad == false && publicera == true)
                                    {
                                        //publicera till linkedin med bild, title, och länk
                                        string title = item["Title"].ToString();

                                        string link = "http://newsflashon.azurewebsites.net/Home/Nyheter/" + UrlManager.MakeURLFriendly(title);

                                        string imgUrl = entity.Image;

                                        bool postSuccess = false;
                                        try
                                        {
                                            if (socialMediaName == "Facebook")
                                            {
                                                postSuccess = SocialMediaManager.postToFacebook(title, link, imgUrl);
                                            }
                                            if (socialMediaName == "Linkedin")
                                            {
                                                postSuccess = SocialMediaManager.PostToLinkedIn(title, link, imgUrl);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            string msg = e.Message;
                                            
                                        }
                                        finally
                                        {
#if DEBUG
                                            postSuccess = false;
#endif
                                            //change status of publicerad to publicerad till linkedin
                                            if (postSuccess)
                                            {
                                                var newPub = new string[3];
                                                if (publiceradArray.Contains("Inte Publicerad"))
                                                {
                                                    newPub = new[] { ("Publicerad till " + socialMediaName) };
                                                }
                                                else if (!publiceradArray.Contains("Publicerad till " + socialMediaName))
                                                {
                                                    string mynewstring = "";
                                                    foreach (string choice in publiceradArray)
                                                    {
                                                        mynewstring += choice + ",";
                                                    }
                                                    mynewstring = mynewstring + "Publicerad till " + socialMediaName;
                                                    newPub = mynewstring.Split(',');
                                                }

                                                item["Publicerad"] = newPub;
                                                item.Update();
                                            }
                                        }
                                    }

                                }
                            }
                        }
                        clientContext.ExecuteQuery();
                    }
                }
            }
            Skip:
            cachedSpContext = null;
        }

        public static string firstImage(string bodyAndArticle)
        {
            string imgUrl = "";
            if (imgUrl == "none")
            {
                string[] parts = bodyAndArticle.Split(' ');//split the string into pieces to find the src
                foreach (string part in parts)
                {
                    if (part.Contains("src"))//check for src
                    {
                        string[] partsOfSrc = part.Split('"');
                        imgUrl = partsOfSrc[1];
                        break;
                    }
                }


            }
            return imgUrl;
        }
    }
}