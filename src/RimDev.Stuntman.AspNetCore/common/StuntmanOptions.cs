using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace RimDev.Stuntman.Core
{
    public partial class StuntmanOptions
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
        /// Determines whether bearer token authentication is enabled
        /// when calling `UseStuntman`.
        /// </summary>
        public bool AllowBearerTokenAuthentication { get; set; } = true;

        /// <summary>
        /// Allows multiple bearer token providers to be used.
        /// When `true`, a bearer token not matching a Stuntman user
        /// does not result in an immediate `403`.
        /// </summary>
        public bool AllowBearerTokenPassthrough { get; set; }

        /// <summary>
        /// Determines whether the UI-driven authentication is enabled
        /// when calling `UseStuntman`. This also determines whether cookie authentication is enabled
        /// since the two are related.
        /// </summary>
        public bool AllowCookieAuthentication { get; set; } = true;

        /// <summary>
        /// The current alignment of the on-screen user picker.
        /// </summary>
        public StuntmanAlignment UserPickerAlignment { get; private set; }

        /// <summary>
        /// Users available for the on-screen user picker and the login UI.
        /// </summary>
        public ICollection<StuntmanUser> Users { get; private set; }

        /// <summary>
        /// The current state of the Stuntman server.
        /// Note: This server endpoint is public (does not require authentication).
        /// </summary>
        public bool ServerEnabled { get; private set; }

        public string ServerUri
        {
            get { return _stuntmanRootPath + Constants.StuntmanOptions.ServerEndpoint; }
        }

        public string SignInUri
        {
            get { return _stuntmanRootPath + Constants.StuntmanOptions.SignInEndpoint; }
        }

        public string SignOutUri
        {
            get { return _stuntmanRootPath + Constants.StuntmanOptions.SignOutEndpoint; }
        }

        /// <summary>
        /// Add a new Stuntman user.
        /// </summary>
        public StuntmanOptions AddUser(StuntmanUser user)
        {
            return AddUser(user, Constants.StuntmanOptions.LocalSource);
        }

        public StuntmanOptions AddUsers(IEnumerable<StuntmanUser> users) 
        {
            if (users == null) throw new ArgumentNullException(nameof(users));

            foreach(var user in users)
                AddUser(user);

            return this;
        }

        /// <remarks>
        /// This method is private to avoid exposing it as part of the public API.
        /// </remarks>
        private StuntmanOptions AddUser(StuntmanUser user, string source)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            if (!IsUniqueUser(user))
            {
                throw new Exception($"{nameof(user)} must have unique Id.");
            }

            if (source != null)
            {
                user.SetSource(source);
            }

            Users.Add(user);

            return this;
        }

        /// <summary>
        /// Add users from JSON at the file system path or URL specified.
        /// The expected JSON format is [{"Id":"user-1","Name":"User 1"}]
        /// </summary>
        /// <param name="pathOrUrl">The path or url to the JSON file.</param>
        /// <param name="configureUsers">Optionally configure all users prior to being added.</param>
        public StuntmanOptions AddUsersFromJson(string pathOrUrl, Action<StuntmanUser> configureUsers = null)
        {
            if (pathOrUrl == null) throw new ArgumentNullException(nameof(pathOrUrl));

            var uri = new Uri(pathOrUrl);

            var json = uri.IsFile
                ? _stuntmanOptionsRetriever.GetStringFromLocalFile(uri)
                : _stuntmanOptionsRetriever.GetStringUsingWebClient(uri);

            var users = JsonConvert.DeserializeObject<StuntmanServerResponse>(
                json,
                new StuntmanClaimConverter());

            foreach (var user in users.Users)
            {
                configureUsers?.Invoke(user);

                AddUser(user, pathOrUrl);
            }

            return this;
        }

        /// <summary>
        /// Add configuration from another Stuntman enabled application that has server mode enabled.
        /// This request is made via HTTP or HTTPS (the 'server' application must be running).
        /// Any exceptions will be thrown as expected.
        /// </summary>
        /// <param name="serverBaseUrl">
        /// The HTTP or HTTPS root url to a Stuntman enabled application
        /// with server mode enabled.
        /// Example: https://my-application.example.com/
        /// </param>
        public StuntmanOptions AddConfigurationFromServer(string serverBaseUrl)
        {
            if (serverBaseUrl == null) throw new ArgumentNullException(nameof(serverBaseUrl));

            var response = _stuntmanOptionsRetriever.GetStringUsingWebClient(
                new Uri(
                    new Uri(serverBaseUrl),
                    Constants.StuntmanOptions.DefaultStuntmanRootPath
                        + Constants.StuntmanOptions.ServerEndpoint));

            var stuntmanServerResponse = JsonConvert.DeserializeObject<StuntmanServerResponse>(
                response,
                new StuntmanClaimConverter());

            ProcessStuntmanServerResponse(stuntmanServerResponse, serverBaseUrl);

            return this;
        }

        /// <summary>
        /// Add configuration from another Stuntman enabled application that has server mode enabled.
        /// This request is made via HTTP or HTTPS (the 'server' application must be running).
        /// Any exceptions while attempting to add the configuration will be silently ignored,
        /// which could result in missing configuration from the 'server'.
        /// </summary>
        /// <param name="serverBaseUrl">
        /// The HTTP or HTTPS root url to a Stuntman enabled application
        /// with server mode enabled.
        /// Example: https://my-application.example.com/
        /// </param>
        /// <param name="onException">Called if an error occurs during configuration</param>
        public StuntmanOptions TryAddConfigurationFromServer(string serverBaseUrl, Action<Exception> onException = null)
        {
            if (serverBaseUrl == null) throw new ArgumentNullException(nameof(serverBaseUrl));

            try
            {
                AddConfigurationFromServer(serverBaseUrl);
            }
            catch (Exception ex)
            {
                if (onException != null)
                {
                    onException(ex);
                }
            }

            return this;
        }

        /// <summary>
        /// Enable the 'server' endpoint on this application.
        /// Note: This server endpoint is public (does not require authentication).
        /// </summary>
        public StuntmanOptions EnableServer()
        {
            ServerEnabled = true;

            return this;
        }

        /// <summary>
        /// Set the alignment of the on-screen user picker.
        /// </summary>
        public StuntmanOptions SetUserPickerAlignment(StuntmanAlignment alignment)
        {
            UserPickerAlignment = alignment;

            return this;
        }

        /// <summary>
        /// Returns on-screen user picker assets for usage in a view or similar.
        /// </summary>
        public string UserPicker(IPrincipal principal)
        {
            return new UserPicker(this).GetHtml(principal);
        }

        /// <summary>
        /// Returns on-screen user picker assets for usage in a view or similar.
        /// </summary>
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

        private void ProcessStuntmanServerResponse(
            StuntmanServerResponse stuntmanServerResponse,
            string source)
        {
            foreach (var user in stuntmanServerResponse.Users)
            {
                AddUser(user, source);
            }
        }
    }
}
