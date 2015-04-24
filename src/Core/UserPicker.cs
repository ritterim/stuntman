using System;
using System.Linq;
using System.Net;
using System.Security.Principal;

namespace RimDev.Stuntman.Core
{
    public class UserPicker
    {
        private readonly StuntmanOptions _options;

        public UserPicker(StuntmanOptions options)
        {
            _options = options;
        }

        public string GetHtml(IPrincipal currentPrincipal, string returnUrl)
        {
            var css = Resources.GetCss();

            var currentUser = currentPrincipal.Identity.Name;

            if (string.IsNullOrEmpty(currentUser))
                currentUser = "Anonymous";

            var items = _options.Users.Select(x => string.Format(
                @"<li><a href=""{0}?{1}={2}&{3}={4}"" class=""stuntman-item""><h3>{5}</h3></a></li>",
                _options.SignInUri,
                StuntmanOptions.OverrideQueryStringKey,
                WebUtility.UrlEncode(x.Id),
                StuntmanOptions.ReturnUrlQueryStringKey,
                WebUtility.UrlEncode(returnUrl),
                x.Name))
                .ToList();

            items.Add(string.Format(
                @"<li><a href=""{0}?{1}={2}"" class=""stuntman-item""><h3>Logout</h3></a></li>",
                _options.SignOutUri,
                StuntmanOptions.ReturnUrlQueryStringKey,
                WebUtility.UrlEncode(returnUrl)));

            return string.Format(@"
<!-- Begin Stuntman -->
<style>
    {0}
</style>
<div class=""stuntman-widget"">
    <div id=""stuntman-header-js"" class=""stuntman-header"">
        <h2 class=""stuntman-title"">
            <a href=""#"">
                Viewing as: {1}
            </a>
        </h2>
    </div>
    <div id=""stuntman-collapse-container-js"" class=""stuntman-body"">
        <ul>
            {2}
        </ul>
    </div>
</div>
<script>
    var header = document.getElementById('stuntman-header-js');
    var collapseContainer = document.getElementById('stuntman-collapse-container-js');

    collapseContainer.style.display = 'none';

    header.addEventListener('click', function() {{
        var currentDisplay = collapseContainer.style.display;

        if (currentDisplay === 'none') {{
            collapseContainer.style.display = 'inherit';
        }}
        else {{
            collapseContainer.style.display = 'none';
        }}
    }}, false);
</script>
<!-- End Stuntman -->",
                css,
                currentUser,
                string.Join(Environment.NewLine, items));
        }
    }
}
