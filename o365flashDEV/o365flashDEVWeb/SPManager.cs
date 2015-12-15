using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace o365flashDEVWeb
{
    public static class SPManager
    {
        //It is required to set this property in order to use SPManager
        public static HttpContextBase CurrentHttpContext { get; set; }

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

    }
}