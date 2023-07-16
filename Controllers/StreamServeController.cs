using Microsoft.AspNetCore.Mvc;
using StreamApi.Models.ReprintInvoice;
using HtmlAgilityPack;
using DinkToPdf;
using DinkToPdf.Contracts;
using System;
using StreamApi.Models.Authorization;
using StreamApi.Models.General;

namespace StreamApi.Controllers
{
    [ApiController]
    [Route("streamServeRestService/streamServe")]
    public class StreamServeController : Controller
    {
        private IConverter _converter;
        private readonly ILogger<StreamServeController> logger;
        private readonly IConfiguration Configuration;

        public StreamServeController(IConverter converter,ILogger<StreamServeController> logger, IConfiguration configuration)
        {
            this.logger = logger;
            Configuration = configuration;
            _converter = converter;
        }//constructore
       

        
        [Route("reprintInvoice")]
        [HttpPost]
        [BasicAuthorization]
        public ActionResult ReprintInvoice(List<InvoiceParameter> ReprintInvoiceRequest)
        {
            /*string fileName = "S040560479.xml";
        string sourcePath = @"D:\WaseemBilling\Input";
        string targetPath = @"D:\WaseemBilling\output";
        string newFileName = "renamedS040560479.txt";


        // Use Path class to manipulate file and directory paths.
        string sourceFile = Path.Combine(sourcePath, fileName);
        string destFile = Path.Combine(targetPath, newFileName);

        // To copy a folder's contents to a new location:
        // Create a new target folder.
        // If the directory already exists, this method does not create a new directory.
        Directory.CreateDirectory(targetPath);

        // To copy a file to another location and
        // overwrite the destination file if it already exists.
        System.IO.File.Copy(sourceFile, destFile, true);*/

            logger.LogCritical("Hi");

            var htmlDoc = new HtmlDocument();
            string filePath = @"D:\WaseemBilling\outputHtml\OldHtmlTransform.html";
            htmlDoc.Load(filePath);

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings =
                {

                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    Margins = new MarginSettings() { Top = 5, Bottom = 5, Left = 5, Right = 5 },
                    Out = @"D:\WaseemBilling\outputPdf\test.pdf",
                },

                Objects =
               {
                    new ObjectSettings()
                    {
                        //PagesCount = true,
                        //HtmlContent = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. In consectetur mauris eget ultrices  iaculis. Ut     
                        //                    odio viverra, molestie lectus nec, venenatis turpis.",
                        HtmlContent = htmlDoc.Text,//htmlDoc.DocumentNode.OuterHtml,
                        WebSettings = {  DefaultEncoding = "utf-8",},
                        //HeaderSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = true, Spacing = 2.812 },
                    }//ObjectSettings
                }//Objects
            };

            byte[] pdf = _converter.Convert(doc);

            return new GeneralResponse(StatusCodes.Status200OK, new ReprintInvoiceResolved("0", "OK", pdf));

            //return new GeneralResponse(StatusCodes.Status400BadRequest, new GeneralResponseError(ErrorCode.FAIL, "The Palestinian Id is null or empty."));

        }//ReprintInvoice
            

    }//class
}//namespace
