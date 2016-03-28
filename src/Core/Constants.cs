namespace RimDev.Stuntman.Core
{
    public static class Constants
    {
        public const string StuntmanAuthenticationType = "StuntmanAuthentication";

        public static class StuntmanOptions
        {
            public const string DefaultStuntmanRootPath = "/stuntman/";
            public const string SignInEndpoint = "sign-in";
            public const string SignOutEndpoint = "sign-out";
            public const string OverrideQueryStringKey = "OverrideUserId";
            public const string ReturnUrlQueryStringKey = "ReturnUrl";
            public const string ServerEndpoint = "server";
        }
    }
}
