using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using StreamApi.Models.Authorization;
using System.Reflection;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Text;
using StreamApi.Models.General;

namespace StreamApi.Services
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private string FailReson = "";
        private int StatusCode;
        private string AuthErrorCode = ErrorCode.SUCCESS;

        private readonly ILogger<BasicAuthenticationHandler> Filelogger;
        private readonly IConfiguration Configuration;

        public BasicAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, ILogger<BasicAuthenticationHandler> Filelogger, IConfiguration configuration) : base(options, logger, encoder, clock)
        {
            this.Filelogger = Filelogger;
            Configuration = configuration;
        }//constructore

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                Response.Headers.Add("WWW-Authenticate", "Basic");

                if (!Request.Headers.ContainsKey("Authorization"))
                {
                    AuthErrorCode = ErrorCode.FAIL;
                    FailReson = "Authorization header missing.";
                    StatusCode = StatusCodes.Status401Unauthorized;
                    Filelogger.LogError(FailReson);
                    return await Task.FromResult(AuthenticateResult.Fail(FailReson));
                }//if

                var authorizationHeader = Request.Headers["Authorization"].ToString();
                var authHeaderRegex = new Regex(@"Basic (.*)");

                if (!authHeaderRegex.IsMatch(authorizationHeader))
                {
                    AuthErrorCode = ErrorCode.FAIL;
                    FailReson = "Authorization code not formatted properly.";
                    StatusCode = StatusCodes.Status401Unauthorized;
                    Filelogger.LogError(FailReson);
                    return await Task.FromResult(AuthenticateResult.Fail(FailReson));
                }//if

                var authBase64 = Encoding.UTF8.GetString(Convert.FromBase64String(authHeaderRegex.Replace(authorizationHeader, "$1")));
                var authSplit = authBase64.Split(Convert.ToChar(":"), 2);
                var authUsername = authSplit[0];
                var authPassword = authSplit.Length > 1 ? authSplit[1] : throw new Exception("Unable to get password");

                if (authUsername != Configuration["StreamApi:UserName"] || authPassword != Configuration["StreamApi:Password"])
                {
                    AuthErrorCode = ErrorCode.FAIL;
                    FailReson = "The username or password is not correct.";
                    StatusCode = StatusCodes.Status403Forbidden;
                    Filelogger.LogError(FailReson);
                    return await Task.FromResult(AuthenticateResult.Fail(FailReson));
                }//if

                var authenticatedUser = new AuthenticatedUser("BasicAuthentication", true, Configuration["StreamApi:UserName"]);
                var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(authenticatedUser));

                return await Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name)));

            }//try
            catch (Exception ex)
            {
                AuthErrorCode = ErrorCode.FATAL;
                FailReson = string.Format("FATAL error occurred in: {0} --> {1}, Details: {2}.", GetType().Name, MethodBase.GetCurrentMethod()?.Name, ex.Message);
                StatusCode = StatusCodes.Status500InternalServerError;
                Filelogger.LogCritical(FailReson);
                return await Task.FromResult(AuthenticateResult.Fail(FailReson));
            }//catch            
        }//HandleAuthenticateAsync

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            await base.HandleChallengeAsync(properties);
            Response.StatusCode = StatusCode;

            await Response.WriteAsJsonAsync(new GeneralResponseError(AuthErrorCode, FailReson));
        }//HandleChallengeAsync

    }//class
}//namespace