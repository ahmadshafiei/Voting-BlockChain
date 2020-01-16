using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Voting.Model.Exceptions;

namespace Voting.API.Controllers.Util
{
    public class BaseController : Controller
    {
        protected bool ApplyAuthorization { get; set; } = true;
        protected string PublicKey { get; set; }
        protected string PrivateKey { get; set; }

        public BaseController(IHttpContextAccessor httpContext)
        {
            InitWallet(httpContext);
        }

        private void InitWallet(IHttpContextAccessor httpContext)
        {
            PrivateKey = httpContext.HttpContext.Request.Headers["PrivateKey"];
            PublicKey = httpContext.HttpContext.Request.Headers["PublicKey"];

            if (ApplyAuthorization && (string.IsNullOrEmpty(PrivateKey) || string.IsNullOrEmpty(PublicKey)))
                throw new UnauthorizedException("عدم دسترسی , مجددا وارد شوید");
        }
    }
}