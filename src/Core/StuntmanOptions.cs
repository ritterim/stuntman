using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RimDev.Stuntman.Core
{
    public class StuntmanOptions
    {
        public const string DefaultStuntmanRootPath = "/stuntman/";
        public const string SignInEndpoint = "sign-in";
        public const string SignOutEndpoint = "sign-out";
        public const string OverrideQueryStringKey = "OverrideUserId";
        public const string ReturnUrlQueryStringKey = "ReturnUrl";

        private static readonly Func<bool> IsDEBUGConstantSet = (() =>
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        });

        private readonly string _stuntmanRootPath;
        private readonly Func<bool> _isDebug;

        public StuntmanOptions(string stuntmanRootPath = DefaultStuntmanRootPath, Func<bool> isDebug = null)
        {
            Users = new List<StuntmanUser>();

            _stuntmanRootPath = stuntmanRootPath;

            if (!_stuntmanRootPath.EndsWith("/"))
                _stuntmanRootPath += "/";

            _isDebug = isDebug ?? IsDEBUGConstantSet;
        }

        /// <summary>
        /// Useful for testing state of IOwinContext post sign-in since the
        /// request is redirected. Therefore, you cannot just add additional OWIN
        /// middleware to check the state.
        /// </summary>
        public Action<OAuthValidateIdentityContext> AfterBearerValidateIdentity { get; set; }

        public bool NonDebugUsageAllowed { get; private set; }

        public ICollection<StuntmanUser> Users { get; private set; }

        public string SignInUri
        {
            get { return _stuntmanRootPath + SignInEndpoint; }
        }

        public string SignOutUri
        {
            get { return _stuntmanRootPath + SignOutEndpoint; }
        }

        public StuntmanOptions AddUser(StuntmanUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            if (!IsUniqueUser(user))
            {
                throw new ApplicationException("user must have unique Id.");
            }
            else
            {
                Users.Add(user);
            }

            return this;
        }

        /// <summary>
        /// Allow Stuntman to be used when the DEBUG constant is not set.
        /// This is to prevent accidental implementation in production by default.
        /// </summary>
        public StuntmanOptions AllowNonDebugUsage()
        {
            NonDebugUsageAllowed = true;

            return this;
        }

        /// <summary>
        /// Verify the usage of Stuntman is permitted.
        /// If usage is not permitted, an InvalidOperationException is thrown.
        /// </summary>
        public void VerifyUsageIsPermitted()
        {
            if (!_isDebug() && !NonDebugUsageAllowed)
            {
                throw new InvalidOperationException(
                    "Stuntman failed to initialize because NonDebugUsageAllowed is false. " +
                    "This default behavior is to prevent accidental implementation in production. " +
                    "To override this behavior, invoke the AllowNonDebugUsage method on StuntmanOptions.");
            }
        }

        /// <summary>
        /// Uses existing user collection to validate whether potential
        /// user's Id is already in the collection.
        /// </summary>
        /// <param name="user">Potential user to add to collection</param>
        private bool IsUniqueUser(StuntmanUser user)
        {
            return Users
                .Select(x => x.Id)
                .Contains(user.Id) == false;
        }
    }
}
