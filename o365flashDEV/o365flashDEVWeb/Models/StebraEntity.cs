using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage.Table;

namespace o365flashDEVWeb.Models
{
    public class StebraEntity : TableEntity
    {
        public StebraEntity()
        { }
        public StebraEntity(string StebraType, string NewsEntry,
            string NewsDescription, string NewsArticle, string NewsDate, string NewsBody)
        {
            //hold properties that mirrors listitem-columns
            this.PartitionKey = StebraType;
            this.RowKey = NewsEntry;
            this.Title = NewsEntry;
            this.Description = NewsDescription;
            this.Article = NewsArticle;
            this.Body = NewsBody;
            this.Image = "none";

            //removing hours from datestamp
            DateTime dateWithHours = Convert.ToDateTime(NewsDate); // 3/13/2015 7:00:00 AM

            string dateNoHourse = dateWithHours.ToString("MM:dd:yyyy"); // 3:13:2015

            this.Date = dateNoHourse.Replace(':', '/'); // 3/13/2015
            //removing hours from datestamp

            string mmddyyyy_hhmmss = NewsDate;
            string mmddyyyy = mmddyyyy_hhmmss.Split(' ')[0];
            string mm = mmddyyyy.Split('/')[0];
            if (mm.Length == 1)
            {
                mm = "0" + mm;
            }
            string dd = mmddyyyy.Split('/')[1];
            if (dd.Length == 1)
            {
                dd = "0" + dd;
            }
            string yyyy = mmddyyyy.Split('/')[2];
            int yyyymmdd = int.Parse(yyyy + mm + dd);
            this.IntDate = yyyymmdd;//int Date property as yyyymmdd for sort/query against list of this object

            firstImage(Body+Article);

        }

        public void firstImage(string bodyAndArticle)
        {

            if (this.Image == "none")
            {
                string[] parts = bodyAndArticle.Split(' ');//split the string into pieces to find the src
                foreach (string part in parts)
                {
                    if (part.Contains("src"))//check for src
                    {
                        string[] partsOfSrc = part.Split('"');
                        this.Image = partsOfSrc[1];
                        break;     
                    }
                }
            }
        }
            
        public string Description { get; set; }
        public string Article { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Date { get; set; }
        public int IntDate { get; set; }
        public string Image { get; set; }
    }
}