using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace RimDev.Stuntman.Core
{
    public class StuntmanOptions
    {
        private readonly string _stuntmanRootPath;
        private readonly StuntmanOptionsRetriever _stuntmanOptionsRetriever;

        public StuntmanOptions(
            string stuntmanRootPath = Constants.StuntmanOptions.DefaultStuntmanRootPath,
            StuntmanOptionsRetriever stuntmanOptionsRetriever = null)
        {
            Users = new List<StuntmanUser>();

            _stuntmanRootPath = stuntmanRootPath;
            _stuntmanOptionsRetriever = stuntmanOptionsRetriever ?? new StuntmanOptionsRetriever();

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
            get { return _stuntmanRootPath + Constants.StuntmanOptions.SignInEndpoint; }
        }

        public string SignOutUri
        {
            get { return _stuntmanRootPath + Constants.StuntmanOptions.SignOutEndpoint; }
        }

        public StuntmanOptions AddUser(StuntmanUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            if (!IsUniqueUser(user))
            {
                throw new ApplicationException($"{nameof(user)} must have unique Id.");
            }

            Users.Add(user);

            return this;
        }

        /// <summary>
        /// Add users from JSON at the file system path or URL specified.
        /// </summary>
        public StuntmanOptions AddUsersFromJson(string pathOrUrl)
        {
            if (pathOrUrl == null) throw new ArgumentNullException(nameof(pathOrUrl));

            var uri = new Uri(pathOrUrl);

            var json = uri.IsFile
                ? _stuntmanOptionsRetriever.GetStringFromLocalFile(uri)
                : _stuntmanOptionsRetriever.GetStringUsingWebClient(uri);

            var users = JsonConvert.DeserializeObject<IEnumerable<StuntmanUser>>(
                json,
                new StuntmanClaimConverter());

            foreach (var user in users)
            {
                AddUser(user);
            }

            return this;
        }

        public StuntmanOptions SetUserPickerAlignment(StuntmanAlignment alignment)
        {
            UserPickerAlignment = alignment;

            return this;
        }

        public string UserPicker(IPrincipal principal)
        {
            return new UserPicker(this).GetHtml(principal);
        }

        public string UserPicker(IPrincipal principal, string returnUrl)
        {
            return new UserPicker(this).GetHtml(principal, returnUrl);
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
