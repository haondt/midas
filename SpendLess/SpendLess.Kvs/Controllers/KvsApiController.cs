using Microsoft.AspNetCore.Mvc;
using SpendLess.Kvs.Models;
using SpendLess.Kvs.Services;
using System.ComponentModel.DataAnnotations;

namespace SpendLess.Kvs.Controllers
{
    [Route("api/kvs")]
    [ApiController]
    public class KvsApiController(IKvsService kvs) : ControllerBase
    {
        /// <summary>
        /// Retrieve a key by it's alias (or return the key itself if it is a key)
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        [HttpGet("key")]
        public async Task<IActionResult> GetKey(
            [FromQuery]
            [Required(AllowEmptyStrings = false)]
            string alias)
        {
            var result = await kvs.GetKeyFromKeyOrAlias(alias);
            if (result.HasValue)
                return Ok(result.Value);
            return NotFound();
        }

        /// <summary>
        /// Retrieve a value by it's key or it's alias
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        [HttpGet("value")]
        public async Task<IActionResult> GetValue(
            [FromQuery]
            [Required(AllowEmptyStrings = false)]
            string alias)
        {
            var result = await kvs.GetValueFromKeyOrAlias(alias);
            if (result.HasValue)
                return Ok(result.Value.Value);

            return NotFound();
        }

        /// <summary>
        /// Upsert a value by its key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpPost("value")]
        public async Task<IActionResult> SetValue([FromBody] ApiUpsertValueRequest request)
        {
            await kvs.UpsertValue(request.Key, request.Value);
            return new StatusCodeResult(StatusCodes.Status201Created);
        }

        /// <summary>
        /// Retrieve a key and value by it's key or it's alias
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        [HttpGet("mapping")]
        public async Task<IActionResult> GetMapping(
            [FromQuery]
            [Required(AllowEmptyStrings = false)]
            string alias)
        {
            var result = await kvs.GetKeyAndValueFromKeyOrAlias(alias);
            if (result.HasValue)
                return Ok(new
                {
                    key = result.Value.Key,
                    value = result.Value.Value.Value
                });

            return NotFound();
        }
    }
}
