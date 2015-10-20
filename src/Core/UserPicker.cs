using System;
using System.Linq;
using System.Net;
using System.Security.Principal;

namespace RimDev.Stuntman.Core
{
    public class UserPicker
    {
        private const string AnonymousUser = "Anonymous";

        private readonly StuntmanOptions _options;

        public UserPicker(StuntmanOptions options)
        {
            _options = options;
        }

        public string GetHtml(IPrincipal currentPrincipal, string returnUrl)
        {
            if (currentPrincipal == null) throw new ArgumentNullException(nameof(currentPrincipal));
            if (returnUrl == null) throw new ArgumentNullException(nameof(returnUrl));

            _options.VerifyUsageIsPermitted();

            var css = Resources.GetCss();

            var currentUser = currentPrincipal.Identity.Name;

            if (string.IsNullOrEmpty(currentUser))
                currentUser = AnonymousUser;

            var items = _options.Users.Select(x => string.Format(
                @"<li class=""{0}""><a href=""{1}?{2}={3}&{4}={5}"" class=""stuntman-item""><h3>{6}{7}</h3></a></li>",
                string.Equals(currentUser, x.Name, StringComparison.OrdinalIgnoreCase)
                    ? "stuntman-active"
                    : string.Empty,
                _options.SignInUri,
                StuntmanOptions.OverrideQueryStringKey,
                WebUtility.UrlEncode(x.Id),
                StuntmanOptions.ReturnUrlQueryStringKey,
                WebUtility.UrlEncode(returnUrl),
                string.Equals(currentUser, x.Name, StringComparison.OrdinalIgnoreCase)
                    ? "&#10004; "
                    : null,
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
                <img class=""stuntman-helmet{1}"" src=""data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAADEAAAAoCAYAAABXRRJPAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAA/RpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuNi1jMDY3IDc5LjE1Nzc0NywgMjAxNS8wMy8zMC0yMzo0MDo0MiAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wTU09Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9tbS8iIHhtbG5zOnN0UmVmPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvc1R5cGUvUmVzb3VyY2VSZWYjIiB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iIHhtbG5zOmRjPSJodHRwOi8vcHVybC5vcmcvZGMvZWxlbWVudHMvMS4xLyIgeG1wTU06T3JpZ2luYWxEb2N1bWVudElEPSJ1dWlkOjVEMjA4OTI0OTNCRkRCMTE5MTRBODU5MEQzMTUwOEM4IiB4bXBNTTpEb2N1bWVudElEPSJ4bXAuZGlkOjkxNDlENjdFMjcxNTExRTVCNTZGODdGNzQ4QkIyOERBIiB4bXBNTTpJbnN0YW5jZUlEPSJ4bXAuaWlkOjkxNDlENjdEMjcxNTExRTVCNTZGODdGNzQ4QkIyOERBIiB4bXA6Q3JlYXRvclRvb2w9IkFkb2JlIElsbHVzdHJhdG9yIENDIDIwMTUgKE1hY2ludG9zaCkiPiA8eG1wTU06RGVyaXZlZEZyb20gc3RSZWY6aW5zdGFuY2VJRD0idXVpZDpmMDFhMTc5ZC0yNWI0LWNlNGYtYTg2ZS1lYWI5OGQ0NGFkNGQiIHN0UmVmOmRvY3VtZW50SUQ9InhtcC5kaWQ6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"" />
                {2}
            </a>
        </h2>
    </div>
    <div id=""stuntman-collapse-container-js"" class=""stuntman-body"">
        <ul>
            {3}
        </ul>
    </div>
</div>
<script>
    (function() {{
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
    }})();
</script>
<!-- End Stuntman -->",
                css,
                currentUser == AnonymousUser ? " stuntman-helmet-disabled" : null,
                currentUser,
                string.Join(Environment.NewLine, items));
        }
    }
}
