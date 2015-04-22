using System.Linq;
using System.Net;

namespace RimDev.Stuntman.Core
{
    public class UserPicker
    {
        private readonly StuntmanOptions _options;

        public UserPicker(StuntmanOptions options)
        {
            _options = options;
        }

        public string GetHtml(string returnUrl)
        {
            var items = _options.Users.Select(x => string.Format(
                "<li><a href=\"{0}?{1}={2}&{3}={4}\">{5}</a></li>",
                _options.SignInUri,
                StuntmanOptions.OverrideQueryStringKey,
                WebUtility.UrlEncode(x.Id),
                StuntmanOptions.ReturnUrlQueryStringKey,
                WebUtility.UrlEncode(returnUrl),
                x.Name))
                .ToList();

            items.Add(string.Format(
                "<li><a href=\"{0}?{1}={2}\">Logout</a></li>",
                _options.SignOutUri,
                StuntmanOptions.ReturnUrlQueryStringKey,
                WebUtility.UrlEncode(returnUrl)));

            return string.Format(
                "<ul>{0}</ul>",
                string.Join(string.Empty, items));
        }
    }
}
