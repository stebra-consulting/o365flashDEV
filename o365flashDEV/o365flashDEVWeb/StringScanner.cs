using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.SharePoint.Client;

namespace o365flashDEVWeb
{
    public class StringScanner
    {

        public static ListItem ScanningListItem(ListItem item)
        {
            //scan body, article for src
            string body = item["Body"].ToString();
            string article = item["Article"].ToString();
            body = ScanForSrc(body);
            article = ScanForSrc(article);

            ListItem newItem = item;
            newItem["Body"] = body;
            newItem["Article"] = article;
            newItem["Datum"] = item["Datum"].ToString();
            newItem["Title"] = item["Title"].ToString();

            return newItem;//return the updated item;
        }

        private static string ScanForSrc(String column)
        {
            string newColumn = column;
            string[] partsOfColumn = column.Split(' ');                     //split the column into pieces to find the src
            foreach (string part in partsOfColumn)
            {
                if (part.Contains("src"))                                   //check for src
                {
                    string[] partsOfSrc = part.Split('"');                   //split on src="url/filename"   part 0 = src=, part 1 = url/filename
                    string url = partsOfSrc[1];                              //part 1 = the url
                    string[] partsOfUrl = url.Split('/');                    //splits the url in to pieces example from sites/sd1/ = sites, sd1...
                    string fileName = partsOfUrl[partsOfUrl.Length - 1];     //last part of the url = filename 

                    //return a file when throwing in filename into SPManager
                    //returns null when image is www/public
                    System.IO.Stream importedFile = SPManager.ImportImage(fileName);   

                    if (importedFile != null) //dont azure when www/public
                    {
                        string azureUrl = AzureManager.ExportImage(importedFile, fileName);
                        //return a url when throwing in a file into AzureManager
                        if (azureUrl != null)
                        {
                            newColumn = ReplaceURLInColumn(url, azureUrl, newColumn);
                        }
                    }

                }
            }
            return newColumn;
        }

        public static string ReplaceURLInColumn(string oldUrl, string azureUrl, string column)
        {
            string newcolumn = column.Replace(oldUrl, azureUrl);
            return newcolumn;
        }


    }
}
