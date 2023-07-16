using Microsoft.AspNetCore.Authorization;

namespace StreamApi.Models.Authorization
{
    public class BasicAuthorizationAttribute : AuthorizeAttribute
    {
        public BasicAuthorizationAttribute()
        {
            Policy = "BasicAuthentication";
        }//BasicAuthorizationAttribute

    }//class
}//namespace
