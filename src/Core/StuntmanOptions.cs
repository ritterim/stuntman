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

        private readonly string _stuntmanRootPath;

        public StuntmanOptions(string stuntmanRootPath = DefaultStuntmanRootPath)
        {
            Users = new List<StuntmanUser>();

            _stuntmanRootPath = stuntmanRootPath;

            if (!_stuntmanRootPath.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                _stuntmanRootPath += "/";
        }

        /// <summary>
        /// Useful for testing state of IOwinContext post sign-in since the
        /// request is redirected. Therefore, you cannot just add additional OWIN
        /// middleware to check the state.
        /// </summary>
        public Action<OAuthValidateIdentityContext> AfterBearerValidateIdentity { get; set; }

        public StuntmanAlignment UserPickerAlignment { get; private set; }

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

        public StuntmanOptions SetUserPickerAlignment(StuntmanAlignment alignment)
        {
            UserPickerAlignment = alignment;

            return this;
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
