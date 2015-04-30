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

            if (!_stuntmanRootPath.EndsWith("/"))
                _stuntmanRootPath += "/";
        }

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
            if (user == null) throw new ArgumentNullException("user");

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
