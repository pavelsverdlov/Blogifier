using Comments;
using Comments.Contracts;
using Core;
using Core.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace App.Comments {
    static class ApplicationBuilderExtenstions {
        public static IApplicationBuilder UseCustomComments(this IApplicationBuilder appBuilder) {
            var options = new CommentsOptions();

            options.SqliteDbFilePath = "comments.db";
            options.LoadCss = false;
            options.IncludeHashInUrl = true;
            options.MarkdigPipeline = null;
            options.NoCommentsTemplate = "0";
            options.MoreThanOneCommentTemplate = "{count}";
            //If true, comment's will be visible only after approval of moderator.
            //op.RequireCommentApproval = true;

            appBuilder.UseMiddleware<CustomCommentsMiddlware>(options);

            return appBuilder;
        }
    }
    public class CustomCommentsMiddlware : CommentsMiddlware {
        const string ModeratorEmail = "";

        IEmailService email;

        public CustomCommentsMiddlware(RequestDelegate next, CommentsOptions options) : base(next, options) {
            options.InformModerator = OnInformModerator;
            options.IsUserAdminModeratorCheck = IsUserAdminModerator;
        }

        public override Task Invoke(HttpContext ctx) {
            email = ctx.RequestServices.GetService<IEmailService>();

            return base.Invoke(ctx);
        }

        bool IsUserAdminModerator(HttpContext context) {
            return context.User.Identity.IsAuthenticated && context.User.IsInRole(AppSettings.Moderator);
        }

        void OnInformModerator(CommentModel model) {
            try {
                email.SendEmail(ModeratorEmail,
                    $"New comment {model.PosterName}",
                    $"Content: '{model.CommentContentSource}'");
            }catch(Exception ex) {
                Log.Logger.Error(ex, $"can't send email to {ModeratorEmail}");
            }
        }
    }
   
}
