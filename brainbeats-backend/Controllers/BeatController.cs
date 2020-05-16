using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace brainbeats_backend.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class BeatController : ControllerBase
  {
    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateBeat(dynamic req) {
      var body = JsonConvert.DeserializeObject<dynamic>(req.ToString());

      string beatId = Guid.NewGuid().ToString();
      string name = body.name;
      string image = body.image;

      if (body.duration == null || name == null || image == null) {
        return BadRequest("Malformed Request");
      }

      string duration = body.duration.ToString();

      string queryString = $"g.addV('beat')" +
        ".property('type', 'beat')" +
        $".property('id', '{beatId}')" +
        $".property('name', '{name}')" +
        $".property('image', '{image}')" +
        $".property('duration', '{duration}')";

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }
  }
}