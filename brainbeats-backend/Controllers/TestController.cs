using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace brainbeats_backend.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class TestController : ControllerBase
  {
    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateSeed(dynamic req) {
      var body = JsonConvert.DeserializeObject<dynamic>(req.ToString());

      string seed = body.seed;

      if (seed == null) {
        return BadRequest("Malformed Request");
      }

      JObject userObject =
        new JObject(
          new JProperty("firstName", $"test_first_name_{seed}"),
          new JProperty("lastName", $"test_last_name_{seed}"),
          new JProperty("email", $"test_email_{seed}@email.com"),
          new JProperty("seed", seed));

      try {
        await new UserController().CreateUser(userObject.ToString());
        return Ok();
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("delete")]
    public async Task<IActionResult> DeleteSeed(dynamic req) {
      var body = JsonConvert.DeserializeObject<dynamic>(req.ToString());

      string seed = body.seed;

      if (seed == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = $"g.V().has('seed', '{seed}').drop()";

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }
  }
}