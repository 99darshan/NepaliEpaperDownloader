using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace EpaperDownloader
{
    class Epaper
    {
        // fields
        private string _year;
        private string _month;
        private string _monthShortName;
        private string _day;
        private string _paperName;

        //properties
        public string PaperName
        {
            get
            {
                return _paperName;
            }
            set
            {
                // map the full name of the paper received from comboBox to the name of the paper
                // that can be used in the URLs for downloading it
                Dictionary<string, string> paperNameDict = new Dictionary<string, string>
                {
                    { "Kantipur", "kantipur"},
                    {"The Kathmandu Post","the-kathmandu-post"},
                    {"Saptahik", "saptahik"},
                    {"Nepal", "nepal" },
                    { "Nari", "nari"},
                    {"Nagarik", "nagarik" },
                    {"Shukrabar", "shukrabar" },
                    {"Republica", "republica" }
                };


                _paperName = paperNameDict[value];
            }
        }

        public DateTime IssueDate { get; set; }
        public string DownloadPath { get; set; }
        public bool IsIssueDateValid { get; set; } = true; // tracks the validity of dates of monthly, weekly or bimontly papers
        public string InvalidDateMsg { get; set; } = "Sorry!! ePaper Issue is Not available for this date.";
        // stores all the links to download the pdf or images for epaper and the corresponding file name 
        public List<Dictionary<string,string>> DownloadLinkAndFileName { get; set; }

        // constructor
        public Epaper(string paperName, DateTime issue, string path)
        {
            PaperName = paperName;
            IssueDate = issue;
            DownloadPath = path;

            _monthShortName = IssueDate.ToString("MMM"); // returns abbreviated month e.g. Dec
            _year = Convert.ToString(IssueDate.Year);
            _month = IssueDate.Month.ToString("00"); // specifying 00 returns 05 instead of 5 
            _day = IssueDate.Day.ToString("00");
        }

        // checks the paper name and calls the respective download function
        public void PaperDownloadInfo()
        {
            switch (PaperName)
            {
                case "kantipur": case "the-kathmandu-post":
                    KantipurNetworkInfo();
                    break;

                case "saptahik": // only published on Fridays
                    if (IssueDate.DayOfWeek != DayOfWeek.Friday)
                    {
                        InvalidDateMsg = "Please Enter a valid Date! \n Saptahik is only Published on Friday!!";
                        IsIssueDateValid = false;
                    }
                    else KantipurNetworkInfo();
                    break;
                  
                case "nepal": // only published on Sundays
                    if (IssueDate.DayOfWeek != DayOfWeek.Sunday)
                    {
                        InvalidDateMsg = "Please Enter a valid Date! \n Nepal is only Published on Sunday!!";
                        IsIssueDateValid = false;
                    }
                    else KantipurNetworkInfo();
                    break;
                case "nari":
                    // TODO check date
                    KantipurNetworkInfo();
                    break;

                case "nagarik":  case "republica":
                    NagarikNewsNetworkInfo();
                    break;

                case "shukrabar":
                    if (IssueDate.DayOfWeek != DayOfWeek.Friday)
                    {
                        InvalidDateMsg = "Please Enter a valid Date! \n Shukrabar is only published on Friday";
                        IsIssueDateValid = false;
                    }
                    else NagarikNewsNetworkInfo();
                    break;
            }
        }

        // Kantipur Network Papers kantipur, ktm post, nepal, nari, saptahik
        public void KantipurNetworkInfo()
        {
            // Download link of ekantipur example
            // http://epaper.ekantipur.com/epaper/kantipur/2017-05-26/2017-05-26.pdf
            string downloadLink = "http://epaper.ekantipur.com/epaper/" + PaperName + "/" + _year + "-" + _month + "-" + _day + "/" + _year + "-" + _month + "-" + _day + ".pdf";

            DownloadLinkAndFileName = new List<Dictionary<string, string>>();

            DownloadLinkAndFileName.Add(new Dictionary<string, string>() {
                {"dlLink", downloadLink },
                {"filePathName",DownloadPath+"\\" + PaperName + "-" + _year + "-" + _month + "-" + _day + ".pdf" }
            });

        }

        // Papers of Nagrik News Network nagarik, republica, shukrabar weekly
        public void NagarikNewsNetworkInfo()
        {
            string linkToEpaperImages="";
            if (PaperName == "nagarik" || PaperName == "shukrabar")
            {
                // change numbers for 01 to get full res image for other pages
                // nagarik
                //http://nagarikplus.nagariknews.com/images/flippingbook/2017_May_27/nagarik/ng_zoom_01.jpg
                // shukrabar weekly
                // http://nagarikplus.nagariknews.com/images/flippingbook/2017_May_26/shukrabar/sh_zoom_01.jpg

                // These links hold all the links for the images of epaper of that particular day
                // http://nagarikplus.nagariknews.com/images/flippingbook/2017_May_26/shukrabar/
                // http://nagarikplus.nagariknews.com/images/flippingbook/2017_May_27/nagarik/

                linkToEpaperImages = "http://nagarikplus.nagariknews.com/images/flippingbook/" + _year + "_" + _monthShortName +
                    "_" + _day + "/" + PaperName + "/";
            }

            if (PaperName == "republica")
            {
                // download link example
                // http://e.myrepublica.com/images/flippingbook/2017_May_27/republica/rp_zoom_01.jpg
                // http://e.myrepublica.com/images/flippingbook/2017_May_27/republica/ has all epaper image links for that date

                linkToEpaperImages = "http://e.myrepublica.com/images/flippingbook/" + _year + "_" + _monthShortName +
                    "_" + _day + "/" + PaperName + "/";
            }

            // pathName,
            // download each image files in the root directory of the project
            // and later use those images to create pdf and save that pdf in the DownloadPath specified by the user
            DirectoryInfo dir = new DirectoryInfo(".\\" + PaperName);
            dir.Create();
            string path = dir + "\\" + _year + "-" + _month + "-" + _day;

            // scrape links to all epaper images from a webpage
            HtmlWeb web = new HtmlWeb();  // This class is from HTML agility pack nuget package
            HtmlDocument doc = web.Load(linkToEpaperImages);

            // select all <a> tags from the html doc using Xpath
            var aTag = doc.DocumentNode.SelectNodes("//a");

            // store only those links which have the word "zoom" keyword in the scrapted href values for a Tags
            DownloadLinkAndFileName = new List<Dictionary<string, string>>();
            int fileNameId = 0; // used to give unique fileNames for different Images

            if (aTag != null)
            {
                foreach (var tag in aTag)
                {
                    string hrefAtttr = tag.Attributes["href"].Value; // get the value of href attribute, e.g. ng_01.jpg or ng_zoom_01.jpg
                    if (hrefAtttr.Substring(0).Contains("zoom"))
                    {
                        fileNameId++;
                        DownloadLinkAndFileName.Add(new Dictionary<string, string> {
                            { "dlLink", linkToEpaperImages + hrefAtttr},
                            { "filePathName", path+"___"+fileNameId+".jpg"}
                        });
                    }
                }
            }

        }

    }
}
