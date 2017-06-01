using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpaperDownloader
{
    class Epaper
    {
        // fields
        private string _year;
        private string _month;
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
                    { "Nari", "nari"}

                };

                _paperName = paperNameDict[value];
            }
        }

        public DateTime IssueDate { get; set; }
        public string DownloadPath { get; set; }
        public string DownloadMsg { get; set; }

        // constructor
        public Epaper(string paperName, DateTime issue, string path)
        {
            PaperName = paperName;
            IssueDate = issue;
            DownloadPath = path;

            _year = Convert.ToString(IssueDate.Year);
            _month = IssueDate.Month.ToString("00"); // specifying 00 returns 05 instead of 5 
            _day = IssueDate.Day.ToString("00");
        }

        // checks the paper name and calls the respective download function
        public void DownloadPaper()
        {
            switch (PaperName)
            {
                case "kantipur":
                case "the-kathmandu-post":
                case "saptahik":
                case "nepal":
                case "nari":
                    DownloadKantipurNetwork();
                    break;
            }
        }

        public void DownloadKantipurNetwork()
        {
            // check for the validity of input date for weekly and monthly papers and bi-monthly papers
            bool isIssueDateValid = true; 

            // saptahik is published weekly on friday
            if (PaperName == "saptahik" && IssueDate.DayOfWeek != DayOfWeek.Friday)
            {
                DownloadMsg = "Please Enter a valid Date! \n Saptahik is only Published on Friday!!";
                isIssueDateValid = false;
            }

            // nepal is published weekly on Sunday
            if (PaperName == "nepal" && IssueDate.DayOfWeek != DayOfWeek.Sunday)
            {
                DownloadMsg = "Please Enter a valid Date! \n Nepal is only Published on Sunday!!";
                isIssueDateValid = false;
            }


            // nari is published every fortnight
            //if (PaperName == "nari")
            //{
            //    // TODO
            //    isValidDate = false;
            //}


            // Download link of ekantipur example
            // http://epaper.ekantipur.com/epaper/kantipur/2017-05-26/2017-05-26.pdf
            string downloadLink = "http://epaper.ekantipur.com/epaper/" + PaperName + "/" + year + "-" + month + "-" + day + "/" + year + "-" + month + "-" + day + ".pdf";
            Uri downloadUri = new Uri(downloadLink);


            string pathWithFileName = DownloadPath + "\\" + PaperName + "-" + year + "-" + month + "-" + day + ".pdf";

            if (isIssueDateValid) Downloader.Download(downloadUri, pathWithFileName);
        }

    }
}
