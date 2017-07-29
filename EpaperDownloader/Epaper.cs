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

        #region Fields
        // fields
        private string _documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
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
                    {"Republica", "republica" },
                    { "Annapurna Post", "annapurnapost" },
                    { "The Himalayan Times","thehimalayantimes" },
                    { "Rajdhani", "rajdhani"}
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
#endregion 
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
                case "nari":
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
                //case "nari":
                // NOTE: this code breaks becuase nari doesn't have cosistent publications
                //    // 2017/04/14 has a issue of nari
                //    // every multiple of 15 days before and after this days should have an issue of nari
                //    DateTime dt = new DateTime(2017, 04, 14);
                //    int daysDiff = Math.Abs((int)dt.Subtract(IssueDate).TotalDays);
                //    if (daysDiff % 15 == 0) KantipurNetworkInfo();
                //    else
                //    {
                //        InvalidDateMsg = "Please Enter a valid Date! \n Nari is published every 15 days!!";
                //        IsIssueDateValid = false;
                //    }
                //    break;

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
                case "rajdhani":
                    RajdhaniInfo();
                    break;
                case "annapurnapost":
                    //AnnapurnaPostInfo();
                    break;
                case "thehimalayantimes":
                    HimalayanTimesInfo();
                    break;
            }
        }

#region download kantipur Network
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
        #endregion

#region download Nagarik Network papers
        // Papers of Nagrik News Network nagarik, republica, shukrabar weekly
        public void NagarikNewsNetworkInfo()
        {
            
            string linkToEpaperImages="";
            // Handles use of july instead of jul in the link
            if (_month == "07") _monthShortName = "July";
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
            // download each image files in the myDocuments\paperName
            // and later use those images to create pdf and save that pdf in the DownloadPath specified by the user
            DirectoryInfo dir = new DirectoryInfo(_documentsPath+"\\" + PaperName);
            if(!dir.Exists) dir.Create();
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
        #endregion

        #region download Annapurna Network papers

        /**
         * 
         * NOTE: the below code for annapurna post doesn't work swiftly 
         * 
         * can't map the selected date to the date id
         * http://annapurnapost.com/epaper/detail/387
         * 
         * in above link the date 2017, 06, 04 should map to id like 387
         * 
         * TODO: think how to map date to date id to get the link despite of inconsistancy in annapurna posts uploads
         * 
         * 
         */



        //public void AnnapurnaPostInfo()
        //{
        //    // BUG ALERT: TODO, 381 and 382 issue have the same paper for jesth 15
        //    // so any dates chosen before that will not return a epaper of correct date

        //    // http://annapurnapost.com/epaper/detail/387 example link that holds the link to image files in <figure> tag
        //    // map the user input date to the date id like 387 in the above link
        //    // 2017/06/14 has the date id of 387, known by manual checking of above link
        //    DateTime dateLinkedWith387id = new DateTime(2017, 06, 04);
        //    TimeSpan span = IssueDate.Subtract(dateLinkedWith387id);
        //    int dateDiff = (int)span.TotalDays; // dateDiff +ve if IssueDate is greater than 2017/05/27 is -ve is IssueDate is less than 2016/09/16

        //    int dateId = (int)Math.Abs((decimal)387 + dateDiff);

        //    string linkToEpaperImages = "http://annapurnapost.com/epaper/detail/" + dateId.ToString();

        //    DownloadLinkAndFileName = new List<Dictionary<string, string>>();
        //    // pathName,
        //    // download each image files in the myDocuments/Annapurnapost dir
        //    // and later use those images to create pdf and save that pdf in the DownloadPath specified by the user
        //    DirectoryInfo dir = new DirectoryInfo(_documentsPath+"\\" + PaperName);
        //    if (!dir.Exists) dir.Create();
        //    string path = dir + "\\" + _year + "-" + _month + "-" + _day;
        //    int fileNameId = 0;

        //    // use Html agility package to extract link pdf link from <iFrame> tag 
        //    HtmlWeb web = new HtmlWeb();
        //    HtmlDocument doc = web.Load(linkToEpaperImages);

        //    // select all a tags that are children of figure tag
        //    var figureTag = doc.DocumentNode.SelectNodes("//figure/a");

        //    if (figureTag != null)
        //    {
        //        foreach (var tag in figureTag)
        //        {
        //            fileNameId++;
        //            string hrefAttr = tag.Attributes["href"].Value;
        //            DownloadLinkAndFileName.Add(new Dictionary<string, string> {
        //                { "dlLink", hrefAttr },
        //                { "filePathName", path + "___" + fileNameId + ".jpg"}
        //            });

        //        }
        //    }

        //}


        public void HimalayanTimesInfo()
        {
            // http://epaper.thehimalayantimes.com/index.php?pagedate=2017-6-1 example link for epaper
            // view source --> div with class flipbook holds all the link to epaper images in the img tags 
            // ^^ changed to below
            // get links of all the images for an isuue in low res
            // it is in pagescrollThumb class

            string linkToEpaperImages = "http://epaper.thehimalayantimes.com/index.php?pagedate=" + _year + "-" + IssueDate.Month.ToString() + "-" + IssueDate.Day.ToString();
            DownloadLinkAndFileName = new List<Dictionary<string, string>>();

            // pathName,
            // download each image files in the myDocuments/himalayantimes dir
            // and later use those images to create pdf and save that pdf in the DownloadPath specified by the user
            DirectoryInfo dir = new DirectoryInfo(_documentsPath+"\\" + PaperName);
            if(!dir.Exists) dir.Create();
            string path = dir + "\\" + _year + "-" + _month + "-" + _day;
            int fileNameId = 0;

            // use Html agility package to extract link pdf link from <iFrame> tag 
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(linkToEpaperImages);
            
            // get links of all the images for an isuue in low res
            // it is in pagescrollThumb class
            var imgTagsInPagescrollThumbClass = doc.DocumentNode.SelectNodes("//div[contains(concat(' ',normalize-space(@class),' '),'pagescrollThumb')]//img");

            //var imgTagsInFlipbookClass = doc.DocumentNode.SelectNodes("//div[contains(@class,'flipbook')]//img");

            if (imgTagsInPagescrollThumbClass != null)
            {
                foreach (var tag in imgTagsInPagescrollThumbClass)
                {
                    string srcAttr = tag.Attributes["src"].Value; // eg.. epaperimages//04062017//04062017-md-hr-1ss.jpg
                    // remove the "ss" epaperimages//04062017//04062017-md-hr-1ss.jpg to get the high res image
                    string highResImg = srcAttr.Remove(srcAttr.Length - 6, 2);
                    if (highResImg.Substring(0).Contains("epaperimages"))
                    {
                        fileNameId++;
                        DownloadLinkAndFileName.Add(new Dictionary<string, string>{
                            { "dlLink", "http://epaper.thehimalayantimes.com/"+highResImg },
                            { "filePathName", path + "___" + fileNameId + ".jpg"}
                        });
                    }
                }
            }
        }
        #endregion

#region download rajhdani ePaper, not shown in the dropDown since they don't provide recent date epapers
        // Rajdhani stoppped publishing epaper from 2016/09/16
        // so we are removing this item from comboxboxItem in the UI
        public void RajdhaniInfo()
        {
            // http://rajdhani.com.np/epaper/898 example link that holds the link to pdf file in <iFrame> tag
            // map the user input date to the date id like 898 in the above link
            DateTime dateLinkedWith898id = new DateTime(2016, 09, 16);
            TimeSpan span = IssueDate.Subtract(dateLinkedWith898id);
            int dateDiff = span.Days; // dateDiff +ve if IssueDate is greater than 2016/09/16 is -ve is IssueDate is less than 2016/09/16

            int dateId = (int)Math.Abs((decimal)898 + dateDiff);

            string linkToPdf = "http://rajdhani.com.np/epaper/" + dateId.ToString();

            // use Html agility package to extract link pdf link from <iFrame> tag 
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(linkToPdf);
            DownloadLinkAndFileName = new List<Dictionary<string, string>>();

            // select all iFrame tag using Xpath
            var iFrameTag = doc.DocumentNode.SelectNodes("//iframe");

            if (iFrameTag != null)
            {
                foreach (var tag in iFrameTag)
                {
                    string srcAttr = tag.Attributes["src"].Value;
                    if (srcAttr.Substring(0).Contains(".pdf"))
                    {
                        DownloadLinkAndFileName.Add(new Dictionary<string, string> {
                            { "dlLink", srcAttr },
                            { "filePathName", DownloadPath+"\\" + PaperName + "-" + _year + "-" + _month + "-" + _day + ".pdf"}
                        });
                    }
                }
            }

        }
#endregion 

    }
}
