using Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Core.Api {
    public class SitemapContent {
        public string Text { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class SitemapController : ControllerBase {
        const string sitemap = "sitemap.xml";
        readonly IStorageService store;

        public SitemapController(IStorageService store) {
            this.store = store;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<SitemapContent>> Get() {
            try {
                var text = await store.ReadRootFileAsync(sitemap);
                return Ok(new SitemapContent() { Text = text });
            } catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<SitemapContent>> Post(SitemapContent content) {
            try {
                await store.WriteRootFileAsync(sitemap, Encoding.UTF8.GetBytes(content.Text));
                return Ok(new SitemapContent());
            } catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
