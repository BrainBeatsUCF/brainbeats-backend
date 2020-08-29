using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace brainbeats_backend.Controllers {
  [Route("api/[controller]")]
  [ApiController]
  public class BeatController : ControllerBase
  {
    /* Beat Schema:
     * id - string
     * duration - double
     * name - string
     * image - string
     * isPrivate - boolean
     * isDeleted - boolean
     * instrumentList - array
     * createdDate - date
     * modifiedDate - date
     * composition - ?
     * genre - ?
     */

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

    [HttpPost]
    [Route("read")]
    public async Task<IActionResult> ReadBeat(dynamic req) {
      var body = JsonConvert.DeserializeObject<dynamic>(req.ToString());

      string beatId = body.beatId;

      if (beatId == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = $"g.V('{beatId}')";

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("update")]
    public async Task<IActionResult> UpdateBeat(dynamic req) {
      var body = JsonConvert.DeserializeObject<dynamic>(req.ToString());

      string beatId = body.beatId;
      string name = body.name;
      string image = body.image;

      if (beatId == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = $"g.V('{beatId}')";
      StringBuilder sb = new StringBuilder(queryString);

      if (name != null) {
        sb.Append($".property('name', '{name}')");
      }

      if (image != null) {
        sb.Append($".property('image', '{image}')");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(sb.ToString());
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("delete")]
    public async Task<IActionResult> DeleteBeat(dynamic req) {
      var body = JsonConvert.DeserializeObject<dynamic>(req.ToString());

      string beatId = body.beatId;

      if (beatId == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = $"g.V('{beatId}').drop()";

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }
  }
}