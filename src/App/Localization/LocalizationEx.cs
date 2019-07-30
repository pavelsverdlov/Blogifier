using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace App.Localization {
    static class SupportedCultureHelper {
        public static readonly RequestCulture DefaultRequest =
            new RequestCulture(culture: "en-US", uiCulture: "en-US");

        public static readonly CultureInfo[] Cultures = new[]
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("ru-RU"),
                    new CultureInfo("zh-cn"),
                    new CultureInfo("zh-tw")
                };

        public static bool TryGetCultureByUrlPart(string part, out CultureInfo culture) {
            culture = CultureInfo.CurrentCulture;
            switch (part) {
                case "ru":
                    culture = new CultureInfo("ru-RU");
                    break;
                case "en":
                    culture = new CultureInfo("en-US");
                    break;
                default:
                    return false;
            }
            return true;
        }

    }

    static class LocalizationEx {
        public static void ConfigureLocalizationOptions(this IServiceCollection services) {
            services.Configure<RequestLocalizationOptions>(options => {
                options.DefaultRequestCulture = SupportedCultureHelper.DefaultRequest;
                options.SupportedCultures = SupportedCultureHelper.Cultures;
                options.SupportedUICultures = SupportedCultureHelper.Cultures;
            });
        }
    }
}
