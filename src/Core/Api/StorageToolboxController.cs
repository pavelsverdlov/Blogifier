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

    public enum QueryProcessingTypes{
        ExecuteNoResult = 0,
        ExecuteWithResult = 1,
    }

    public class StorageQuery {
        public string Text { get; set; }
        public QueryProcessingTypes Type { get; set; }
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
        [Authorize]
        public async Task<ActionResult<StorageState>> Post(StorageQuery query) {
            var state = new StorageState(AppSettings.DbProvider);
            try {
                if (!ModelState.IsValid)
                    return BadRequest("Invalid data");

                switch (query.Type) {
                    case QueryProcessingTypes.ExecuteNoResult:
                        var res = await db.ExecuteRaw(query.Text);
                        state.Message = $"{res}";
                        break;
                    case QueryProcessingTypes.ExecuteWithResult:
                        state.Message = await db.FromQuery(query.Text);
                        break;
                }
            } catch (Exception ex) {
                state.Message = ex.Message;
            }

            return Ok(state);
        }
    }
}
