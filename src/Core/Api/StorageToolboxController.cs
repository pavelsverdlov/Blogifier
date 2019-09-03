using Core.Helpers;
using Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Api {
    public class StorageState {
        public string Provider { get; }
        public string Message { get; set; }

        public StorageState(string dbProvider) {
            this.Provider = dbProvider;
        }
    }

    public class StorageQuery {
        public string Text { get; set; }
    }


    [Route("api/[controller]")]
    [ApiController]
    public class StorageToolboxController : ControllerBase {
        readonly IDbStorageService db;

        public StorageToolboxController(IDbStorageService db, IOptions<RequestLocalizationOptions> options) {
            this.db = db;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<StorageState>> Get() {
            try {

                return Ok(new StorageState(AppSettings.DbProvider));
            } catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }


        [HttpPost]
        [Administrator]
        public async Task<ActionResult<StorageState>> Post(StorageQuery query) {
            var state = new StorageState(AppSettings.DbProvider);
            try {
                if (!ModelState.IsValid)
                    return BadRequest("Invalid data");

                if(query.Text.Contains("select", StringComparison.InvariantCultureIgnoreCase)) {
                    var res = await db.FromQuery(query.Text);
                    state.Message = res;
                } else {
                    var res = await db.ExecuteRaw(query.Text);
                    state.Message = $"{res}";
                }
            } catch (Exception ex) {
                state.Message = ex.Message;
            }

            return Ok(state);
        }
    }
}
