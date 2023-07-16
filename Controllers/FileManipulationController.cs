using DinkToPdf;
using DinkToPdf.Contracts;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using StreamApi.Models.Authorization;
using StreamApi.Models.General;
using StreamApi.Models.FileManipulation;
using System.Reflection;


namespace StreamApi.Controllers
{
    [ApiController]
    [Route("file-manipulation")]
    public class FileManipulationController : Controller
    {
        private IConverter _converter;        
        private readonly IConfiguration Configuration;
        private readonly ILogger<FileManipulationController> logger;

        public FileManipulationController(IConverter converter, ILogger<FileManipulationController> logger,IConfiguration configuration)
        {
            this.logger = logger;
            Configuration = configuration;
            _converter = converter;
        }//constructore


        [Route("html-to-pdf")]
        [HttpPost]
        [BasicAuthorization]
        public ActionResult HtmlToPdf(FileDataRequest FileDataRequest)
        {
            try 
            {
                logger.LogInformation("... Start HtmlToPdf ...");
                
                    logger.LogInformation("InputFilePath: {InputFilePath}, OutputFilePath: {OutputFilePath}, InvoiceNo: {InvoiceNo}, BillingAccountId: {BillingAccountId}, SequenceNo: {SequenceNo}, MwNotificationType: {MwNotificationType}", FileDataRequest.InputFilePath, FileDataRequest.OutputFilePath, FileDataRequest.InvoiceNo, FileDataRequest.BillingAccountId, FileDataRequest.SequenceNo, FileDataRequest.MwNotificationType);
                GeneralResponse? GeneralResponse = CheckHtmlToPdfInput(FileDataRequest);
                if (GeneralResponse != null)
                {
                    GeneralResponseError GeneralResponseError = (GeneralResponseError) GeneralResponse.Value;
                    logger.LogError("StatusCode: {StatusCode}, ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}, ", GeneralResponse.StatusCode, GeneralResponseError?.ErrorCode, GeneralResponseError?.ErrorMessage);
                    return GeneralResponse;
                }//if
                    

                var htmlDoc = new HtmlDocument();
                string filePath = FileDataRequest.InputFilePath;//@"D:\WaseemBilling\outputHtml\OldHtmlTransform.html";
                htmlDoc.Load(filePath);

                var doc = new HtmlToPdfDocument()
                {
                    GlobalSettings =
                {

                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    Margins = new MarginSettings() { Top = 5, Bottom = 5, Left = 5, Right = 5 },
                    Out = FileDataRequest.OutputFilePath//@"D:\WaseemBilling\outputPdf\test.pdf",
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

                logger.LogInformation("Html document created");

                byte[] pdf = _converter.Convert(doc);
                //string Base64String Convert.ToBase64String(pdf);

                logger.LogInformation("convert done.");

                string pdfFileName = Configuration["EmptyPdfName"];

                if(!string.IsNullOrEmpty(doc.GlobalSettings.Out))
                {
                    int startIndex = doc.GlobalSettings.Out.LastIndexOf('\\')+1;
                    if(startIndex == -1)
                        startIndex = doc.GlobalSettings.Out.LastIndexOf('/')+1;

                    pdfFileName = doc.GlobalSettings.Out[startIndex..];
                }//if

                if (!FileDataRequest.MwNotificationType.ToLower().Equals("none"))
                {
                    logger.LogInformation("... call  SendMwNotification ...");
                    Thread thread = new(() => SendMwNotification(FileDataRequest.InvoiceNo /*InvoiceNo*/, FileDataRequest.BillingAccountId /*BillingAccountId*/, FileDataRequest.SequenceNo /*SequenceNo*/, pdfFileName/*PdfName*/, FileDataRequest.MwNotificationType/*NotificationType*/));
                    thread.Start();
                }//if

                logger.LogInformation("... End HtmlToPdf ...");

                return new GeneralResponse(StatusCodes.Status200OK, new FileManipulationResolved { FileInByte = pdf });

            }//try
            catch (Exception ex)
            {
                string Error = string.Format("FATAL error occurred in: {0} --> {1}, Details: {2}.", GetType().Name, MethodBase.GetCurrentMethod()?.Name, ex.Message);

                logger.LogCritical(Error);

                return new GeneralResponse(StatusCodes.Status500InternalServerError, new GeneralResponseError(ErrorCode.FATAL, Error));
            }//catch
        }//HtmlToPdf

        private GeneralResponse? CheckHtmlToPdfInput(FileDataRequest FileDataRequest)
        {
            if (string.IsNullOrEmpty(FileDataRequest.InputFilePath))
            {
                string Error = string.Format("error occurred in: {0} --> {1}, Details: InputFilePath Is Null Or Empty.", GetType().Name, MethodBase.GetCurrentMethod()?.Name);
                return new GeneralResponse(StatusCodes.Status400BadRequest, new GeneralResponseError(ErrorCode.FAIL, Error));
            }//if
            if (string.IsNullOrEmpty(FileDataRequest.MwNotificationType))
            {
                string Error = string.Format("error occurred in: {0} --> {1}, Details: MwNotificationType Is Null Or Empty.", GetType().Name, MethodBase.GetCurrentMethod()?.Name);
                return new GeneralResponse(StatusCodes.Status400BadRequest, new GeneralResponseError(ErrorCode.FAIL, Error));
            }//if
            if (!FileDataRequest.MwNotificationType.ToLower().Equals("normal") && !FileDataRequest.MwNotificationType.ToLower().Equals("cds") && !FileDataRequest.MwNotificationType.ToLower().Equals("none"))
            {
                string Error = string.Format("error occurred in: {0} --> {1}, Details: MwNotificationType must be Normal, CDS, or None.", GetType().Name, MethodBase.GetCurrentMethod()?.Name);
                return new GeneralResponse(StatusCodes.Status400BadRequest, new GeneralResponseError(ErrorCode.FAIL, Error));
            }//if
            //if (string.IsNullOrEmpty(FileDataRequest.SequenceNo) && FileDataRequest.MwNotificationType.ToLower().Equals("normal"))
            //{
            //    string Error = string.Format("error occurred in: {0} --> {1}, Details: SequenceNo Is Null Or Empty.", GetType().Name, MethodBase.GetCurrentMethod()?.Name);
            //    return new GeneralResponse(StatusCodes.Status400BadRequest, new GeneralResponseError(ErrorCode.FAIL, Error));
            //}//if
            //if (string.IsNullOrEmpty(FileDataRequest.BillingAccountId))
            //{
            //    string Error = string.Format("error occurred in: {0} --> {1}, Details: BillingAccountId Is Null Or Empty.", GetType().Name, MethodBase.GetCurrentMethod()?.Name);
            //    return new GeneralResponse(StatusCodes.Status400BadRequest, new GeneralResponseError(ErrorCode.FAIL, Error));
            //}//if
            if (string.IsNullOrEmpty(FileDataRequest.InvoiceNo))
            {
                string Error = string.Format("error occurred in: {0} --> {1}, Details: InvoiceNo Is Null Or Empty.", GetType().Name, MethodBase.GetCurrentMethod()?.Name);
                return new GeneralResponse(StatusCodes.Status400BadRequest, new GeneralResponseError(ErrorCode.FAIL, Error));
            }//if

            return null;
        }//CheckHtmlToPdfInput

        private GeneralErrorMessage SendMwNotification(string InvoiceNo, string BillingAccountId, string SequenceNo,string PdfName,string NotificationType)
        {
            try
            {
                logger.LogInformation("... start SendMwNotification ...");

                logger.LogInformation("InvoiceNo: {InvoiceNo}, BillingAccountId: {BillingAccountId}, SequenceNo: {SequenceNo}, PdfName: {PdfName}, NotificationType: {NotificationType}", InvoiceNo, BillingAccountId, SequenceNo, PdfName, NotificationType);

                string requestData = "notifyInvoiceGeneration?data=" + InvoiceNo + "," + BillingAccountId + "," + Configuration["ApiAddresses:StreamServeUrl"] + PdfName + "," + SequenceNo;

                if(NotificationType.ToLower().Equals("cds"))
                    requestData = "notifyCDS?data=" + InvoiceNo + "," + Configuration["ApiAddresses:StreamServeUrl"] + PdfName + "," + BillingAccountId;

                using HttpClient client = new();

                //client.DefaultRequestHeaders.Add("Authorization", Configuration["HeaderAuth:MwLookupId"]);

                client.BaseAddress = new Uri(Configuration["ApiAddresses:MwNotification"]);

                HttpResponseMessage resp = client.PostAsJsonAsync(requestData, "").Result;

                if (resp.IsSuccessStatusCode)
                {
                    MwNotificationResponse apiResponse = resp.Content.ReadAsAsync<MwNotificationResponse>().Result;

                    if (apiResponse == null)
                        return new GeneralErrorMessage(true /*TerminateProccess*/, StatusCodes.Status502BadGateway /*ResponseStatus*/, ErrorCode.FATAL /*ErrorCode*/, "invalid response from MW server." /*ErrorMessage*/, null /*ResponseObject*/);

                    string MwResponseCode = apiResponse.resp ?? "";

                    if (MwResponseCode.Equals("0"))
                    {
                        logger.LogInformation("... End SendMwNotification ...");
                        return new GeneralErrorMessage(StatusCodes.Status200OK /*ResponseStatus*/, null /*ResponseObject*/);
                    }//if
                    else
                    {
                        string Error = "Invalid MW api response, ResponseCode: " + apiResponse.resp + ", ResponseMessage: " + apiResponse.respMessage;
                        logger.LogError(Error);
                        return new GeneralErrorMessage(true /*TerminateProccess*/, StatusCodes.Status502BadGateway /*ResponseStatus*/, ErrorCode.FAIL /*ErrorCode*/, Error /*ErrorMessage*/, null /*ResponseObject*/);
                    }//else
                }//if
                else
                {
                    string Error = "Error in calling MW api with response status code: " + resp.StatusCode;
                    logger.LogError(Error);
                    return new GeneralErrorMessage(true /*TerminateProccess*/, StatusCodes.Status502BadGateway /*ResponseStatus*/, ErrorCode.FATAL /*ErrorCode*/, Error /*ErrorMessage*/, null /*ResponseObject*/);                    
                }//else

            }//try
            catch (Exception ex)
            {
                string Error = string.Format("FATAL error occurred in: {0} --> {1}, Details: {2}.", GetType().Name, MethodBase.GetCurrentMethod()?.Name, ex.Message);

                logger.LogCritical(Error);

                if (ex.Message.IndexOf("connection") > -1)
                    return new GeneralErrorMessage(true /*TerminateProccess*/, StatusCodes.Status504GatewayTimeout /*ResponseStatus*/, ErrorCode.FATAL /*ErrorCode*/, "Error in calling MW api with response status code: " + Error /*ErrorMessage*/, null /*ResponseObject*/);

                return new GeneralErrorMessage(true /*TerminateProccess*/, StatusCodes.Status500InternalServerError /*ResponseStatus*/, ErrorCode.FATAL /*ErrorCode*/, "Error in calling MW api with response status code: " + Error /*ErrorMessage*/, null /*ResponseObject*/);
            }//catch
        }//SendMwNotification
    }//class
}//namespace
