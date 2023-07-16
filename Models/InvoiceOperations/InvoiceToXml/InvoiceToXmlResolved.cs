using StreamApi.Models.General;

namespace StreamApi.Models.InvoiceOperations.InvoiceToXml
{
    public class InvoiceToXmlResolved : GeneralResponseError
    {
        public byte[]? XmlFileInByte { get; set; }
    }//class
}//namespace