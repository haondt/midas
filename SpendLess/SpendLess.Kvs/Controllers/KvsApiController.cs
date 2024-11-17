using Microsoft.AspNetCore.Mvc;
using SpendLess.Kvs.Services;

namespace SpendLess.Kvs.Controllers
{
    [Route("api/kvs")]
    public class KvsApiController(IKvsService kvs) : ControllerBase
    {
        /// <summary>
        /// Retrieve a key by it's alias (or return the key itself if it is a key)
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        [HttpGet("{alias}/key")]
        public async Task<IActionResult> GetKey(string alias)
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
        [HttpGet("{alias}/value")]
        public async Task<IActionResult> GetValue(string alias)
        {
            var result = await kvs.GetValueFromKeyOrAlias(alias);
            if (result.HasValue)
                return Ok(result.Value.Value);

            return NotFound();
        }

        /// <summary>
        /// Retrieve a key and value by it's key or it's alias
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        [HttpGet("{alias}/mapping")]
        public async Task<IActionResult> GetMapping(string alias)
        {
            var result = await kvs.GetKeyAndValueFromKeyOrAlias(alias);
            if (result.HasValue)
                return Ok(new
                {
                    key = result.Value.Key,
                    value = result.Value.Value
                });

            return NotFound();
        }
    }
}
