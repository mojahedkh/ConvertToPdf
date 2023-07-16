using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using StreamApi.Models.Authorization;
using StreamApi.Models.General;
using StreamApi.Models.InvoiceOperations.InvoiceToXml;
using System.Reflection;
using System.Xml;

namespace StreamApi.Controllers
{
    [ApiController]
    [Route("invoice-operations")]
    public class InvoiceOperationsController : Controller
    {
        private readonly IConfiguration Configuration;
        private readonly ILogger<FileManipulationController> logger;
        public InvoiceOperationsController(ILogger<FileManipulationController> logger, IConfiguration configuration)
        {
            this.logger = logger;
            Configuration = configuration;
        }//constructore

        [Route("invoice-to-xml")]
        [HttpPost]
        [BasicAuthorization]
        public ActionResult InvoiceToXml(InvoiceDataRequest InvoiceDataRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(InvoiceDataRequest.InvoicesString))
                {
                    string Error = string.Format("error occurred in: {0} --> {1}, Details: InvoicesString Is Null or Empty.", GetType().Name, MethodBase.GetCurrentMethod()?.Name);
                    return new GeneralResponse(StatusCodes.Status400BadRequest, new GeneralResponseError(ErrorCode.FAIL, Error));
                }//if
                
                if(InvoiceDataRequest.InvoicesString.EndsWith(";"))
                    InvoiceDataRequest.InvoicesString = InvoiceDataRequest.InvoicesString[..^1];

                List<InvoiceCategory> InvoiceCategoryList = InvoiceStringListToObjects(InvoiceDataRequest.InvoicesString.Split(";").ToList());

                if (InvoiceCategoryList.Count == 0)
                    return new GeneralResponse(StatusCodes.Status400BadRequest, new InvoiceToXmlResolved { ErrorCode = ErrorCode.FAIL, ErrorMessage = "Please specify at least one invoice" });

                if (InvoiceCategoryList[0].InvoiceCycle.CompareTo(new DateTime(2018/*year*/, 12/*Month*/, 01/*Day*/)) < 0)
                    return LoadInvoice(InvoiceCategoryList);

                if (InvoiceCategoryList[0].InvoiceCycle.CompareTo(new DateTime(2020/*year*/, 02/*Month*/, 01/*Day*/)) < 0)
                    return CopyInvoice(InvoiceCategoryList, Configuration["SystemPaths:InvoiceXmlOutPathVer2"] /*InvoiceXmlOutPath*/);

                if (InvoiceCategoryList[0].InvoiceCycle.CompareTo(new DateTime(2020/*year*/, 06/*Month*/, 01/*Day*/)) < 0)
                    return CopyInvoice(InvoiceCategoryList, Configuration["SystemPaths:InvoiceXmlOutPathVer3"] /*InvoiceXmlOutPath*/);

                if (InvoiceCategoryList[0].InvoiceCycle.CompareTo(new DateTime(2021/*year*/, 12/*Month*/, 30/*Day*/)) < 0)
                    return CopyInvoice(InvoiceCategoryList, Configuration["SystemPaths:InvoiceXmlOutPathVer4"] /*InvoiceXmlOutPath*/);

                if (InvoiceCategoryList[0].InvoiceCycle.CompareTo(new DateTime(2022/*year*/, 05/*Month*/, 01/*Day*/)) < 0)
                    return CopyInvoice(InvoiceCategoryList, Configuration["SystemPaths:InvoiceXmlOutPathVer5"] /*InvoiceXmlOutPath*/);

                return CopyInvoice(InvoiceCategoryList, Configuration["SystemPaths:InvoiceXmlOutPathVer6"] /*InvoiceXmlOutPath*/);

            }//try
            catch (Exception ex)
            {
                string Error = string.Format("FATAL error occurred in: {0} --> {1}, Details: {2}.", GetType().Name, MethodBase.GetCurrentMethod()?.Name, ex.Message);

                logger.LogCritical(Error);

                return new GeneralResponse(StatusCodes.Status500InternalServerError, new GeneralResponseError(ErrorCode.FATAL, Error));
            }//catch
            
        }//InvoiceToXml

        private GeneralResponse LoadInvoice(List<InvoiceCategory> InvoiceCategoryList)
        {
            List<InvoiceXml> InvoiceXmlList = LoadDbInvoicesData(InvoiceCategoryList);

            if (InvoiceXmlList.Count == 0)
            {
                string Error = string.Format("error occurred in: {0} --> {1}, Details: No any invoice found.", GetType().Name, MethodBase.GetCurrentMethod()?.Name);
                return new GeneralResponse(StatusCodes.Status400BadRequest, new GeneralResponseError(ErrorCode.FAIL, Error));
            }//if

            if (InvoiceXmlList.Count > 1)
            {
                string folderFullName = Configuration["SystemPaths:InvoiceXmlOutPathVer1"] +"\\"+ InvoiceXmlList[0].InvoiceNo + "_" + InvoiceXmlList[0].BillingRequestId + "_" + InvoiceXmlList.Count;
                Directory.CreateDirectory(folderFullName);

                foreach (InvoiceXml InvoiceXml in InvoiceXmlList)
                    WriteXmlInvoice(folderFullName + "\\" + InvoiceXml.InvoiceNo + ".xml", InvoiceXml);
            }//if
            else
            {
                string fileFullName = Configuration["SystemPaths:InvoiceXmlOutPathVer1"] + "\\" + InvoiceXmlList[0].InvoiceNo + "_" + InvoiceXmlList[0].BillingRequestId + ".xml";
                WriteXmlInvoice(fileFullName, InvoiceXmlList[0]);
            }//else

            if (InvoiceCategoryList.Count > InvoiceXmlList.Count)
                return new GeneralResponse(StatusCodes.Status200OK, new InvoiceToXmlResolved { ErrorCode = ErrorCode.WARNING, ErrorMessage = "Please Note one or more invoices not exists." });

            return new GeneralResponse(StatusCodes.Status200OK, new InvoiceToXmlResolved { });
        }//LoadInvoice

        private GeneralResponse CopyInvoice(List<InvoiceCategory> InvoiceCategoryList, string InvoiceXmlOutPath)
        {
            int invoiceCount = 0;

            if (InvoiceCategoryList.Count > 1)
            {
                string folderDestFullName = InvoiceXmlOutPath + "\\" +InvoiceCategoryList[0].InvoiceNo + "_" + InvoiceCategoryList[0].BillingRequestId + "_" + InvoiceCategoryList.Count;
                Directory.CreateDirectory(folderDestFullName);

                foreach (InvoiceCategory InvoiceCategory in InvoiceCategoryList)
                {
                    string sourceFile = Path.Combine(Configuration["SystemPaths:InvoiceXmlInputPath"]+ "\\" + InvoiceCategory.InvoiceCycleAsString, InvoiceCategory.InvoiceNo + ".xml");

                    System.IO.File.Copy(sourceFile, folderDestFullName + "\\" + InvoiceCategory.InvoiceNo + ".xml", true);
                    invoiceCount++;
                }//foreach

            }//if
            else
            {
                string fileDestFullName = InvoiceXmlOutPath + "\\" + InvoiceCategoryList[0].InvoiceNo + "_" + InvoiceCategoryList[0].BillingRequestId + ".xml";
                string sourceFile = Path.Combine(Configuration["SystemPaths:InvoiceXmlInputPath"] + "\\" + InvoiceCategoryList[0].InvoiceCycleAsString, InvoiceCategoryList[0].InvoiceNo + ".xml");
                System.IO.File.Copy(sourceFile, fileDestFullName, true);
                invoiceCount++;
            }//else

            if (InvoiceCategoryList.Count > invoiceCount)
                return new GeneralResponse(StatusCodes.Status200OK, new InvoiceToXmlResolved { ErrorCode = ErrorCode.WARNING, ErrorMessage = "Please Note one or more invoices not exists." });

            return new GeneralResponse(StatusCodes.Status200OK, new InvoiceToXmlResolved { });
        }//CopyInvoice

        private List<InvoiceCategory> InvoiceStringListToObjects(List<string> InvoicesStringList)
        {
            // S_20220501_coll-cust0000429940-20220501,EBill-7a6cdcc4-c3cf-4b3f-97eb-b3e65e2e1176
            //S_20220501_R013044821,EBill-145b7f24-792b-46bf-b0c9-c30e3e3a0849

            List<InvoiceCategory> InvoiceCategoryList = new();

            foreach (string InvoicesString in InvoicesStringList)
            {
                string[] InvoiceCategory = InvoicesString.Split("_");
                string[] InvoiceNoWithRequestId = InvoiceCategory[2].Split(",");

                InvoiceCategoryList.Add(new InvoiceCategory
                {
                    InvoiceType = InvoiceCategory[0],//2022 02 01
                    InvoiceCycle = new DateTime(int.Parse(InvoiceCategory[1][..4])/*year*/, int.Parse(InvoiceCategory[1].Substring(4, 2))/*Month*/, int.Parse(InvoiceCategory[1].Substring(6, 2))/*Day*/),
                    InvoiceCycleAsString = InvoiceCategory[1],
                    InvoiceNo = InvoiceNoWithRequestId[0],
                    BillingRequestId = InvoiceNoWithRequestId[1],
                });
            }//foreach

            return InvoiceCategoryList;
        }//InvoiceStringToObject

        private void WriteXmlInvoice(string XmlFullPathFileName,InvoiceXml XMLInvoice)
        {
            XmlWriterSettings settings = new()
            {
                Indent = true
            };
            
            XmlWriter writer = XmlWriter.Create(XmlFullPathFileName, settings);

            writer.WriteStartDocument();

            writer.WriteComment("This file is generated by system (StreamApi).");

            writer.WriteStartElement("InvoicesList");
            writer.WriteStartElement("Invoice");

            /************ start **********/

            AddXmlElement(writer, "AccountNo", XMLInvoice.AccountNo);
            AddXmlElement(writer, "InvoiceNo", XMLInvoice.InvoiceNo);
            AddXmlElement(writer, "MobileNumber", XMLInvoice.MobileNumber);
            AddXmlElement(writer, "CustomerName", XMLInvoice.CustomerName);
            AddXmlElement(writer, "Address1", XMLInvoice.Address1);
            AddXmlElement(writer, "Address2", XMLInvoice.Address2);
            AddXmlElement(writer, "Address3", XMLInvoice.Address3);
            AddXmlElement(writer, "Address4", XMLInvoice.Address4);
            AddXmlElement(writer, "ADNumber", XMLInvoice.ADNumber);
            AddXmlElement(writer, "InvoicePeriod", XMLInvoice.InvoicePeriod);
            AddXmlElement(writer, "SubscriberClassification", XMLInvoice.SubscriberClassification);
            AddXmlElement(writer, "Email", XMLInvoice.Email);
            AddXmlElement(writer, "DueDate", XMLInvoice.DueDate);
            AddXmlElement(writer, "MonthlyPackageFee", XMLInvoice.MonthlyPackageFee.ToString());
            AddXmlElement(writer, "ValueAddedServicesFees", XMLInvoice.ValueAddedServicesFees.ToString());
            AddXmlElement(writer, "CallsToJawwalAndPaltel", XMLInvoice.CallsToJawwalAndPaltel.ToString());
            AddXmlElement(writer, "CallsToOtherLocalNetworks", XMLInvoice.CallsToOtherLocalNetworks.ToString());
            AddXmlElement(writer, "CallsToIsraeliNetworks", XMLInvoice.CallsToIsraeliNetworks.ToString());
            AddXmlElement(writer, "MobileInternet", XMLInvoice.MobileInternet.ToString());
            AddXmlElement(writer, "InternationalCalls", XMLInvoice.InternationalCalls.ToString());
            AddXmlElement(writer, "NationalRoaming", XMLInvoice.NationalRoaming.ToString());
            AddXmlElement(writer, "InternationalRoaming", XMLInvoice.InternationalRoaming.ToString());
            AddXmlElement(writer, "OneNetwork", XMLInvoice.OneNetwork.ToString());
            AddXmlElement(writer, "Messages", XMLInvoice.Messages.ToString());
            AddXmlElement(writer, "CallsToShortCodes", XMLInvoice.CallsToShortCodes.ToString());
            AddXmlElement(writer, "OtherCharges", XMLInvoice.OtherCharges.ToString());
            AddXmlElement(writer, "HandsetInstallmentsReceivables", XMLInvoice.HandsetInstallmentsReceivables.ToString());
            AddXmlElement(writer, "Discount", XMLInvoice.Discount.ToString());
            AddXmlElement(writer, "Total", XMLInvoice.Total.ToString());
            AddXmlElement(writer, "ValueAddedTax", XMLInvoice.ValueAddedTax.ToString());
            AddXmlElement(writer, "GrandTotal", XMLInvoice.GrandTotal.ToString());
            AddXmlElement(writer, "DueInvoices", XMLInvoice.DueInvoices.ToString());
            AddXmlElement(writer, "TotalDueAmount", XMLInvoice.TotalDueAmount.ToString());

            /************  End ***********/


            writer.WriteEndElement();
            writer.WriteEndElement();

            writer.WriteEndDocument();
            writer.Flush();
            writer.Close();

        }//WriteXmlInvoice

        private void AddXmlElement(XmlWriter writer, string name, string? value)
        {
            if (value == null)
                return;
            writer.WriteStartElement(name);
            writer.WriteAttributeString("value", value);
            writer.WriteEndElement();
        }//AddXmlElement

        private List<InvoiceXml> LoadDbInvoicesData(List<InvoiceCategory> InvoiceCategoryList)
        {
            List<InvoiceXml> InvoiceXmlList = new();
            using OracleConnection connection = new(Configuration["ConnectionStrings:BSCS"]);
            try
            {
                connection.Open();
                try
                {
                    foreach (InvoiceCategory InvoiceCategory in InvoiceCategoryList)
                    {
                        InvoiceXml? invoiceXml = LoadInvoice(connection, InvoiceCategory);
                        if(invoiceXml != null)
                        InvoiceXmlList.Add(invoiceXml);
                    }//foreach

                    return InvoiceXmlList;
                }//try
                finally
                {
                    connection.Close();
                    connection.Dispose();
                }//finally
            }//try
            catch (Exception ex)
            {
                logger.LogCritical(string.Format("FATAL error occured in Execute LoadDbInvoicesData, Details: {0}.", ex));
                throw;
            }//catch
        }//LoadDbInvoicesData

        InvoiceXml? LoadInvoice(OracleConnection connection, InvoiceCategory InvoiceCategory)
        {
            try
            {
                logger.LogInformation(string.Format("start load of invoiceNo: '{0}'", InvoiceCategory.InvoiceNo));
                OracleCommand command = connection.CreateCommand();
                command.Parameters.Clear();

                command.CommandText = "select * from stream_serv_appl.rpi_lines where INVOICE_NO = :invoiceNo";
                command.Parameters.Add("invoiceNo", OracleDbType.Varchar2).Value = InvoiceCategory.InvoiceNo;

                InvoiceXml XMLInvoice = new()
                {
                    ADNumber = "562451310",//static value for all invoices
                    InvoiceType = InvoiceCategory.InvoiceType,
                    InvoiceCycle = InvoiceCategory.InvoiceCycle,
                    BillingRequestId = InvoiceCategory.BillingRequestId,
                    InvoiceNo = InvoiceCategory.InvoiceNo
                };

                using (OracleDataReader reader = command.ExecuteReader())
                {
                    bool invoiceFound = false;
                    while (reader.Read())
                    {
                        invoiceFound = true;
                        string orderNo = reader["ORDER_"].ToString()+"";
                        string value = reader["LINE_"].ToString()+"";
                        string[] valueArray = value.Split("|");
                        if(orderNo.Equals("0"))
                        {
                            //XMLInvoice.InvoiceNo = valueArray[13];
                            XMLInvoice.InvoicePeriod = valueArray[18];                            
                            XMLInvoice.DueDate = valueArray[19];
                        }//if
                        if (orderNo.Equals("1"))
                        {
                            XMLInvoice.SubscriberClassification = valueArray[93];
                            XMLInvoice.AccountNo = valueArray[8];
                        }//if                        
                        if (orderNo.Equals("4"))
                        {
                            XMLInvoice.CustomerName = valueArray[7];
                            XMLInvoice.Address1 = valueArray[9];
                            XMLInvoice.Address2 = valueArray[10];
                            XMLInvoice.Address3 = valueArray[11];
                            XMLInvoice.Address4 = valueArray[12];
                            XMLInvoice.Email = valueArray[20];
                        }//if
                        if (orderNo.Equals("5"))
                        {
                            XMLInvoice.MobileNumber = valueArray[1];
                        }//if
                        if (orderNo.Equals("6"))
                        {
                            string serviceCode = valueArray[1];
                            double serviceValue = double.Parse(valueArray[2]);
                            string serviceType = LookupType(connection, serviceCode);

                            XMLInvoice = AddValueAsMapedServiceCode(serviceType, serviceValue, XMLInvoice);

                            XMLInvoice.ValueAddedTax += double.Parse(valueArray[4]);

                            if (serviceValue > 0)
                                XMLInvoice.Total += serviceValue;
                        }//if
                        if (orderNo.Equals("7"))
                        {
                            XMLInvoice.DueInvoices = double.Parse(valueArray[1]);
                        }//if
                    }//while
                    if (!invoiceFound)
                        return null;

                    XMLInvoice.Total += XMLInvoice.Discount;

                    XMLInvoice.GrandTotal = XMLInvoice.Total + XMLInvoice.ValueAddedTax;
                    XMLInvoice.TotalDueAmount = XMLInvoice.GrandTotal + XMLInvoice.DueInvoices;
                }//using

                logger.LogInformation(string.Format("end load of invoiceNo: '{0}'", InvoiceCategory.InvoiceNo));

                return RoundAllValueOfInvoice(XMLInvoice);

            }//try
            catch (Exception ex)
            {
                logger.LogError(string.Format("Error in load invoiceNo: '{0}' with ex: '{1}'", InvoiceCategory.InvoiceNo, ex.Message));
                logger.LogInformation(string.Format("InnerException Message: '{0}'", ex.InnerException));
                logger.LogInformation(string.Format("StackTrace Message: '{0}'", ex.StackTrace));
                throw;
            }//catch
        }//LoadInvoice

        private InvoiceXml RoundAllValueOfInvoice (InvoiceXml XMLInvoice)
        {            
            XMLInvoice.MonthlyPackageFee = Math.Round(XMLInvoice.MonthlyPackageFee, 2);
            XMLInvoice.ValueAddedServicesFees = Math.Round(XMLInvoice.ValueAddedServicesFees, 2);
            XMLInvoice.CallsToJawwalAndPaltel = Math.Round(XMLInvoice.CallsToJawwalAndPaltel, 2);
            XMLInvoice.CallsToOtherLocalNetworks = Math.Round(XMLInvoice.CallsToOtherLocalNetworks, 2);
            XMLInvoice.CallsToIsraeliNetworks = Math.Round(XMLInvoice.CallsToIsraeliNetworks, 2);
            XMLInvoice.MobileInternet = Math.Round(XMLInvoice.MobileInternet, 2);
            XMLInvoice.InternationalCalls = Math.Round(XMLInvoice.InternationalCalls, 2);
            XMLInvoice.NationalRoaming = Math.Round(XMLInvoice.NationalRoaming, 2);
            XMLInvoice.InternationalRoaming = Math.Round(XMLInvoice.InternationalRoaming, 2);
            XMLInvoice.OneNetwork = Math.Round(XMLInvoice.OneNetwork, 2);
            XMLInvoice.Messages = Math.Round(XMLInvoice.Messages, 2);
            XMLInvoice.CallsToShortCodes = Math.Round(XMLInvoice.CallsToShortCodes, 2);
            XMLInvoice.OtherCharges = Math.Round(XMLInvoice.OtherCharges, 2);
            XMLInvoice.HandsetInstallmentsReceivables = Math.Round(XMLInvoice.HandsetInstallmentsReceivables, 2);
            XMLInvoice.Discount = Math.Round(XMLInvoice.Discount, 2);
            XMLInvoice.Total = Math.Round(XMLInvoice.Total, 2);
            XMLInvoice.ValueAddedTax = Math.Round(XMLInvoice.ValueAddedTax, 2);
            XMLInvoice.GrandTotal = Math.Round(XMLInvoice.GrandTotal, 2);
            XMLInvoice.DueInvoices = Math.Round(XMLInvoice.DueInvoices, 2);
            XMLInvoice.TotalDueAmount = Math.Round(XMLInvoice.TotalDueAmount, 2);

            return XMLInvoice;
        }//RoundAllValueOfInvoice

        private InvoiceXml AddValueAsMapedServiceCode(string serviceType, double serviceValue, InvoiceXml XMLInvoice)
        {
            if (serviceValue < 0)
            {
                XMLInvoice.Discount += serviceValue;
                return XMLInvoice;
            }//if

            if (serviceType.Equals("FAT"))
                XMLInvoice.OtherCharges += serviceValue;

            if (serviceType.Equals("Handset"))
                XMLInvoice.HandsetInstallmentsReceivables += serviceValue;

            if (serviceType.Equals("INMO"))
                XMLInvoice.MonthlyPackageFee += serviceValue;

            if (serviceType.Equals("IR"))
                XMLInvoice.InternationalRoaming += serviceValue;

            if (serviceType.Equals("MF"))
                XMLInvoice.MonthlyPackageFee += serviceValue;

            if (serviceType.Equals("NR"))
                XMLInvoice.NationalRoaming += serviceValue;

            if (serviceType.Equals("Others"))
                XMLInvoice.OtherCharges += serviceValue;

            if (serviceType.Equals("SMS"))
                XMLInvoice.Messages += serviceValue;

            if (serviceType.Equals("VAS"))
                XMLInvoice.ValueAddedServicesFees += serviceValue;

            /*if (serviceType.Equals("Disc"))
                XMLInvoice.Discount += serviceValue;*/

            return XMLInvoice;

        }//AddValueAsMapedServiceCode

        string LookupType(OracleConnection connection, string serviceCode)
        {
            try
            {
                string serviceType = "";
                logger.LogInformation(string.Format("start LookupType of serviceCode: '{0}'", serviceCode));
                OracleCommand command = connection.CreateCommand();
                command.Parameters.Clear();

                command.CommandText = "select * from stream_serv_appl.distinct_services_new where VALUE2 = :serviceCode";
                command.Parameters.Add("serviceCode", OracleDbType.Varchar2).Value = serviceCode;

                using (OracleDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                       serviceType = reader["DES"].ToString() + "";
                    }//if
                }//using

                logger.LogInformation(string.Format("end LookupType of serviceCode: '{0}'", serviceCode));

                return serviceType;

            }//try
            catch (Exception ex)
            {
                logger.LogError(string.Format("Error in load serviceCode: '{0}' with ex: '{1}'", serviceCode, ex.Message));
                logger.LogInformation(string.Format("InnerException Message: '{0}'", ex.InnerException));
                logger.LogInformation(string.Format("StackTrace Message: '{0}'", ex.StackTrace));
                throw;
            }//catch
        }//LookupType

    }//class
}//namespace