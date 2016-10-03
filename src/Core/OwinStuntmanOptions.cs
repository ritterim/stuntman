using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimDev.Stuntman.Core
{
    public partial class StuntmanOptions
    {

        /// <summary>
        /// Useful for testing state of IOwinContext post sign-in since the
        /// request is redirected. Therefore, you cannot just add additional OWIN
        /// middleware to check the state.
        /// </summary>
        public Action<OAuthValidateIdentityContext> AfterBearerValidateIdentity { get; set; }
    }
}
