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
        const string en_Name = "en-US";
        const string ru_Name = "ru-RU";

        public static readonly RequestCulture DefaultRequest =
            new RequestCulture(culture: en_Name, uiCulture: en_Name);

        public static readonly CultureInfo[] Cultures = new[]
                {
                    new CultureInfo(en_Name),
                    new CultureInfo(ru_Name),
                    new CultureInfo("zh-cn"),
                    new CultureInfo("zh-tw")
                };

        public static bool TryGetCultureByUrlPart(string part, out CultureInfo culture) {
            culture = CultureInfo.CurrentCulture;
            switch (part) {
                case "ru":
                    culture = new CultureInfo(ru_Name);
                    break;
                case "en":
                    culture = new CultureInfo(en_Name);
                    break;
                default:
                    return false;
            }
            return true;
        }
        public static string GetCultureTagOrDefault(CultureInfo culture) {
            var part = "en";
            switch (culture.Name) {
                case ru_Name:
                    part = "ru";
                    break;
            }
            return part;
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
