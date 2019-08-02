using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace App.Localization {
    class SectionsToIgnoreLocalization : IEnumerable<string> {
        readonly List<string> sections;

        public SectionsToIgnoreLocalization(params string[] sections) {
            this.sections = new List<string>(sections);
        }

        public IEnumerator<string> GetEnumerator() {
            return sections.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return sections.GetEnumerator();
        }
    }
    class LocalizationMiddleware {
        const char slash = '/';

        readonly RequestDelegate next;
        readonly SectionsToIgnoreLocalization notLocalizedSection;

        public LocalizationMiddleware(RequestDelegate next, SectionsToIgnoreLocalization notLocalizedSection) {
            this.next = next;
            this.notLocalizedSection = notLocalizedSection;
        }

        public async Task InvokeAsync(HttpContext context) {
            var val = context.Request.Path.Value;
            var url = val.EndsWith(slash) ? val : string.Concat(context.Request.Path.Value, slash);

            if (notLocalizedSection.Any(x => url.Contains(x))) {
                await next(context);
                return;
            }

            var culture = context.Request.Path.Value.TrimStart(slash);
            var start = culture.IndexOf(slash);
            if (start != -1) {
                culture = culture.Substring(0, start);
            }

            if (!SupportedCultureHelper.TryGetCultureByUrlPart(culture, out var cultureinfo)) {
                var part = context.Request.Path.HasValue ? url.TrimStart(slash) : string.Empty;
                var tag = SupportedCultureHelper.GetCultureTagOrDefault(CultureInfo.CurrentCulture);
                context.Response.Redirect(string.Concat(slash, tag, slash, part));
                return;
            }

            CultureInfo.CurrentCulture = cultureinfo;
            CultureInfo.CurrentUICulture = cultureinfo;

            await next(context);
        }
    }
}
