using System.Collections.Generic;

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

        public ICollection<StuntmanUser> Users { get; set; }

        public string SignInUri
        {
            get { return _stuntmanRootPath + SignInEndpoint; }
        }

        public string SignOutUri
        {
            get { return _stuntmanRootPath + SignOutEndpoint; }
        }
    }
}
