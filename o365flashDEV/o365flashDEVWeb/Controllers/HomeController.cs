using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using o365flashDEVWeb.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage.Blob;

namespace o365flashDEVWeb.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index()
        {

            //SPManager.CurrentHttpContext = HttpContext;
            //ListItemCollection items = SPManager.GetItemCollection("Nyhetslista");

            //string FileLeafRef = "peter_okt.jpg";
            //using (var fileStream = SPManager.GetImage(FileLeafRef))
            //{

            //    AzureManager.CreateBlob(fileStream, "ImageFromStream");
            //}

            return View();
        }
        [SharePointContextFilter]
        public ActionResult Publish()
        {//catch Ribbon action URL Parametr
            var spContext = SharePointContextProvider.Current.GetSharePointContext(HttpContext);
            if (spContext != null)
            {
                string listGuid = Request.QueryString["SPListId"];

                SPManager.CurrentHttpContext = HttpContext;
                ListItemCollection items = SPManager.GetItemsFromGuid(listGuid);

                List<ListItem> stebraList = new List<ListItem>();

                foreach (ListItem item in items)
                {
                    ListItem scannedItem = StringScanner.ScanningListItem(item);
                    stebraList.Add(scannedItem);
                }

                AzureManager.CreateTable(stebraList);
                ViewBag.Status = "Success. Newslist have been published to: " + AzureManager.tableName + " in Azure Storage";
            }

            else
            {
                ViewBag.Status = "Too many requests in queue. Please try again later.";
            }

            //done


            return View();

        }

        public ActionResult About()
        {
            //not yet implemented
            //IEnumerable<StebraEntity> news = AzureTableManager.LoadAllNews();

            return View();//news
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
