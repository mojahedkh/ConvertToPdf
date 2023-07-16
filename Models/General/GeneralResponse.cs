using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace StreamApi.Models.General
{
    public class GeneralResponse : JsonResult
    {
        public GeneralResponse(int statusCode, object responseObjrct) : base(responseObjrct)
        {
            StatusCode = statusCode;
        }//Constructore
    }//class
}//namespace