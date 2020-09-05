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

      JObject sampleObject0 =
        new JObject(
          new JProperty("sampleId", "test_sample_id_0"),
          new JProperty("name", "test_sample_name_0"),
          new JProperty("email", $"test_email_{seed}@email.com"),
          new JProperty("seed", seed));

      JObject sampleObject1 =
        new JObject(
          new JProperty("sampleId", "test_sample_id_1"),
          new JProperty("name", "test_sample_name_1"),
          new JProperty("email", $"test_email_{seed}@email.com"),
          new JProperty("seed", seed));

      try {
        await new UserController().CreateUser(userObject.ToString());
        await new SampleController().CreateSample(sampleObject0.ToString());
        await new SampleController().CreateSample(sampleObject1.ToString());
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