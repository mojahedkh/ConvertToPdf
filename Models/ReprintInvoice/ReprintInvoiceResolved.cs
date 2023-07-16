using StreamApi.Models.General;

namespace StreamApi.Models.ReprintInvoice
{
    public class ReprintInvoiceResolved : GeneralResponseError
    {
        public byte[] streamResponse { get; set; }

        public ReprintInvoiceResolved(string resp, string respMessage, byte[] streamResponse) : base(resp, respMessage)
        {
            this.streamResponse = streamResponse;
        }//ReprintInvoiceResolved
    }//class
}//namespace