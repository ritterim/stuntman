using System;
using System.Linq;
using System.Net;
using System.Security.Principal;

namespace RimDev.Stuntman.Core
{
    public class UserPicker
    {
        private const string AnonymousUser = "Anonymous";
        private const string HelmetImgSrc = "data:image/svg+xml;base64,PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4NCjwhLS0gR2VuZXJhdG9yOiBBZG9iZSBJbGx1c3RyYXRvciAxOS4xLjAsIFNWRyBFeHBvcnQgUGx1Zy1JbiAuIFNWRyBWZXJzaW9uOiA2LjAwIEJ1aWxkIDApICAtLT4NCjxzdmcgdmVyc2lvbj0iMS4xIiBpZD0iTGF5ZXJfMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeD0iMHB4IiB5PSIwcHgiDQoJIHZpZXdCb3g9Ii0zNjMgMjc1IDY0IDY0IiBzdHlsZT0iZW5hYmxlLWJhY2tncm91bmQ6bmV3IC0zNjMgMjc1IDY0IDY0OyIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSI+DQo8c3R5bGUgdHlwZT0idGV4dC9jc3MiPg0KCS5zdDB7ZmlsbDojRkZGRkZGO30NCgkuc3Qxe2ZpbGw6I0VGM0IyNDt9DQoJLnN0MntmaWxsOiMzQTVBQTk7fQ0KCS5zdDN7ZmlsbDojOTY5NDk5O30NCgkuc3Q0e2ZpbGw6IzM2MzYzNjt9DQo8L3N0eWxlPg0KPHBhdGggY2xhc3M9InN0MCIgZD0iTS0zMzAuNSwyODQuNWMtMTMsMC4yLTE5LDExLjItMTksMTEuMmwtOC41LDE1LjVjLTIsNi4yLTEuMiwxMC41LTAuMSwxMy4yYzAuMSwwLjMsMC4yLDAuNSwwLjMsMC43DQoJYzAuOCwxLjYsMS43LDIuNSwxLjcsMi41YzMyLjYsNC41LDQ0LjQsMCw0NC40LDBjMC42LTAuNywxLjItMS40LDEuNy0yLjFjMC4xLTAuMSwwLjEtMC4xLDAuMS0wLjJjNS44LTcuNiw2LjMtMTQuMSw2LjMtMTQuMQ0KCUMtMzAyLjMsMjkxLjktMzE3LjQsMjg0LjMtMzMwLjUsMjg0LjVMLTMzMC41LDI4NC41eiIvPg0KPHBhdGggY2xhc3M9InN0MSIgZD0iTS0zMzAuNSwyODQuN2MtMTMuMSwwLjItMTksMTEuMS0xOSwxMS4xbC04LjUsMTUuNWMtMiw2LjItMS4yLDEwLjUtMC4xLDEzLjJjMC4xLDAuMywwLjIsMC41LDAuMywwLjcNCgljMy4zLDEuMSwxNC4xLDIuNyw0Ny44LDAuNGMwLjEtMC4xLDAuMS0wLjEsMC4xLTAuMmM1LjgtNy42LDYuMy0xNC4xLDYuMy0xNC4xQy0zMDIuNCwyOTIuMS0zMTcuNCwyODQuNS0zMzAuNSwyODQuN0wtMzMwLjUsMjg0Ljd6DQoJIi8+DQo8cGF0aCBjbGFzcz0ic3QwIiBkPSJNLTMyOS4zLDI4OS4xYy0xMS44LDAuMi0xNy4yLDEwLjEtMTcuMiwxMC4xbC03LjcsMTQuMWMtMS44LDUuNi0xLjEsOS42LTAuMSwxMS45YzAuMSwwLjIsMC4yLDAuNCwwLjMsMC42DQoJYzMsMSwxMi43LDIuNSw0My4zLDAuNGMwLjEtMC4xLDAuMS0wLjEsMC4xLTAuMmM1LjItNi45LDUuNy0xMi44LDUuNy0xMi44Qy0zMDMuOSwyOTUuOS0zMTcuNSwyODktMzI5LjMsMjg5LjF6Ii8+DQo8cGF0aCBjbGFzcz0ic3QyIiBkPSJNLTMyOS4xLDI5MS42Yy0xMS4xLDAuMi0xNi4yLDkuNS0xNi4yLDkuNWwtNy4yLDEzLjJjLTEuNyw1LjMtMSw5LTAuMSwxMS4yYzAuMSwwLjIsMC4yLDAuNCwwLjMsMC42DQoJYzIuOCwwLjksMTEuOSwyLjMsNDAuNiwwLjNjMC4xLTAuMSwwLjEtMC4xLDAuMS0wLjJjNC45LTYuNCw1LjQtMTIsNS40LTEyQy0zMDUuMiwyOTcuOS0zMTgsMjkxLjQtMzI5LjEsMjkxLjZ6Ii8+DQo8cGF0aCBjbGFzcz0ic3QzIiBkPSJNLTM1Ny44LDMyNS4yYzAuOCwxLjYsMS43LDIuNSwxLjcsMi41YzMyLjYsNC41LDQ0LjQsMCw0NC40LDBjMC42LTAuNywxLjItMS40LDEuNy0yLjENCglDLTM0My43LDMyOC0zNTQuNCwzMjYuMy0zNTcuOCwzMjUuMkwtMzU3LjgsMzI1LjJ6Ii8+DQo8cGF0aCBjbGFzcz0ic3QwIiBkPSJNLTMzMi43LDMxN2wxLjcsMi40aDNsLTEuOCwyLjRsMC45LDIuOGwtMi44LTFsLTIuNCwxLjhsMC4xLTNsLTIuNC0xLjdsMi45LTAuOUwtMzMyLjcsMzE3eiIvPg0KPHBhdGggY2xhc3M9InN0MCIgZD0iTS0zMjQsMzE1LjdsMS43LDIuNGgzbC0xLjgsMi40bDEsMi44bC0yLjgtMWwtMi40LDEuOGwwLjEtM2wtMi40LTEuN2wyLjktMC45TC0zMjQsMzE1Ljd6Ii8+DQo8cGF0aCBjbGFzcz0ic3QwIiBkPSJNLTMxNS4zLDMxNC4zbDEuNywyLjRsMywwbC0xLjgsMi40bDEsMi44bC0yLjgtMWwtMi40LDEuOGwwLjEtM2wtMi40LTEuN2wyLjktMC45TC0zMTUuMywzMTQuM3oiLz4NCjxwYXRoIGNsYXNzPSJzdDAiIGQ9Ik0tMzQ5LjYsMjk0LjRjMCwwLTUuMyw1LjctOSwxNy4ybDE4LjYsMS41YzAsMCw3LjgtMC4zLDEwLjktNS4zYzMuMi01LDUuMS04LjYtMi4yLTkuOQ0KCUMtMzM4LjcsMjk2LjctMzQ5LjYsMjk0LjQtMzQ5LjYsMjk0LjR6Ii8+DQo8cGF0aCBjbGFzcz0ic3QwIiBkPSJNLTM0OS4yLDI5My44YzAsMC01LjksNi4xLTEwLjEsMTguNGwyMC43LDEuNmMwLDAsOC43LTAuMywxMi4yLTUuN2MzLjUtNS4zLDUuNy05LjItMi41LTEwLjYNCglDLTMzNywyOTYuMi0zNDkuMiwyOTMuOC0zNDkuMiwyOTMuOHoiLz4NCjxwYXRoIGNsYXNzPSJzdDQiIGQ9Ik0tMzQ5LjksMjk0LjRjMCwwLTUuMyw1LjctOSwxNy4ybDE4LjYsMS41YzAsMCw3LjgtMC4zLDEwLjktNS4zYzMuMi01LDUuMS04LjYtMi4yLTkuOQ0KCUMtMzM5LDI5Ni43LTM0OS45LDI5NC40LTM0OS45LDI5NC40eiIvPg0KPC9zdmc+DQo=";

        private readonly StuntmanOptions _options;

        public UserPicker(StuntmanOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Returns the self-contained assets for the on-screen user picker.
        /// </summary>
        public string GetHtml(IPrincipal currentPrincipal)
        {
            if (currentPrincipal == null) throw new ArgumentNullException(nameof(currentPrincipal));

            return GetHtmlInternal(currentPrincipal, null);
        }

        /// <summary>
        /// Returns the self-contained assets for the on-screen user picker.
        /// </summary>
        public string GetHtml(IPrincipal currentPrincipal, string returnUrl)
        {
            if (currentPrincipal == null) throw new ArgumentNullException(nameof(currentPrincipal));
            if (returnUrl == null) throw new ArgumentNullException(nameof(returnUrl));

            return GetHtmlInternal(currentPrincipal, returnUrl);
        }

        private string GetHtmlInternal(IPrincipal currentPrincipal, string returnUrl)
        {
            var css = Resources.GetCss();

            var currentUser = currentPrincipal.Identity.Name;

            if (string.IsNullOrEmpty(currentUser))
                currentUser = AnonymousUser;

            var items = _options.Users.Select(x => string.Format(@"
<li class=""{0}"">
    <a href=""{1}?{2}={3}{4}"" class=""stuntman-item"" title=""Source: {5}"">
        <h3>{6}</h3>
    </a>
</li>",
                string.Equals(currentUser, x.Name, StringComparison.OrdinalIgnoreCase)
                    ? "stuntman-active"
                    : string.Empty,
                _options.SignInUri,
                Constants.StuntmanOptions.OverrideQueryStringKey,
                WebUtility.UrlEncode(x.Id),
                returnUrl == null
                    ? null
                    : $"&{Constants.StuntmanOptions.ReturnUrlQueryStringKey}={WebUtility.UrlEncode(returnUrl)}",
                x.Source,
                x.Name))
                .ToList();

            items.Add($@"
<li>
    <a href=""{_options.SignOutUri}?{Constants.StuntmanOptions.ReturnUrlQueryStringKey}={WebUtility.UrlEncode(returnUrl)}"" class=""stuntman-item stuntman-logout"">
        <h3>Logout</h3>
    </a>
</li>");

            return @"
<!-- Begin Stuntman -->" + Environment.NewLine +
$@"<style>
    {css}
</style>
<div class=""stuntman-widget stuntman-alignment-{_options.UserPickerAlignment.ToString().ToLowerInvariant()}"">
    <div id=""stuntman-header-js"" class=""stuntman-header"">
        <h2 class=""stuntman-title"">
            <a href=""#"">
                <img class=""stuntman-helmet{(currentUser == AnonymousUser ? " stuntman-helmet-disabled" : null)}"" src=""{HelmetImgSrc}"" />
                {currentUser}
            </a>
        </h2>
    </div>
    <div id=""stuntman-collapse-container-js"" class=""stuntman-body"">
        <ul>
            {string.Join(Environment.NewLine, items)}
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
</script>".Replace(Environment.NewLine, null) + Environment.NewLine +
"<!-- End Stuntman -->";
        }
    }
}
