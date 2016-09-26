using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace RimDev.Stuntman.Core
{
    public partial class StuntmanOptions
    {
        /// <summary>
        /// Useful for testing state of IOwinContext post sign-in since the
        /// request is redirected. Therefore, you cannot just add additional 
        /// middleware to check the state.
        /// </summary>
        public Action<MessageReceivedContext> AfterBearerValidateIdentity { get; set; }
    }
}
