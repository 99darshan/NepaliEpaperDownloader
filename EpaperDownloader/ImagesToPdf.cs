using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace EpaperDownloader
{
    class ImagesToPdf
    {
        public List<string> ImageFilePath { get; set; }
        public Epaper EpaperInstance { get; set; }

        public ImagesToPdf(List<string> inpImagePaths, Epaper epaper)
        {
            ImageFilePath = inpImagePaths;
            EpaperInstance = epaper;
        }

        public void GeneratePdf()
        {

            // incase there is on output directory
            DirectoryInfo dir = new DirectoryInfo(EpaperInstance.DownloadPath);
            if (!dir.Exists) dir.Create();

            string outputPathPlusFileName = EpaperInstance.DownloadPath + "\\" + EpaperInstance.PaperName + "-" + EpaperInstance.IssueDate.Year + "-"
                + EpaperInstance.IssueDate.Month + "-" + EpaperInstance.IssueDate.Day + ".pdf";

            // create file stream for output file in open mode
            FileStream fs = new FileStream(outputPathPlusFileName, FileMode.Append);
            // using iTextsharp Classes
            // specify output docuement property, instantiate pdfWriter and open the doc
            Document doc = new Document(PageSize.A4, 0f, 0f, 0f, 0f);

            PdfWriter writer = PdfWriter.GetInstance(doc, fs);
            doc.Open();

            float maxWidth = doc.PageSize.Width - doc.LeftMargin - doc.RightMargin;
            float maxHeight = doc.PageSize.Height - doc.TopMargin - doc.BottomMargin;

            foreach (string imageFileName in ImageFilePath)
            {
                // read the image file
                FileStream imageStream = new FileStream(imageFileName, FileMode.Open);

                var image = Image.GetInstance(imageStream);

                // scale and add image to the pdf doc
                if (image.Height > maxHeight || image.Width > maxWidth) image.ScaleToFit(maxWidth, maxHeight);
                doc.Add(image);

                imageStream.Close();
            }
            doc.Close();

            // TODO delete all the images from the dir once the pdf is created 
            // saves up the disk space
            foreach (string imageFileName in ImageFilePath)
            {
                File.Delete(imageFileName);
            }
        }
    }
}
