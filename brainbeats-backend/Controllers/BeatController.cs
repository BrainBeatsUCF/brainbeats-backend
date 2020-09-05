using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language;
using Newtonsoft.Json;
using static brainbeats_backend.Utility;

namespace brainbeats_backend.Controllers {
  [Route("api/[controller]")]
  [ApiController]
  public class BeatController : ControllerBase
  {
    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateBeat(dynamic req) {
      string request = Utility.GetRequest(req);
      var body = JsonConvert.DeserializeObject<dynamic>(request);

      string beatId = Guid.NewGuid().ToString();
      string name = body.name;
      string email = body.email;

      string duration = body.duration;
      string image = body.image;
      string isPrivate = body.isPrivate;
      string instrumentList = body.instrumentList;
      string createdDate = GetCurrentTime();
      string modifiedDate = GetCurrentTime();
      string attributes = body.attributes;
      string audio = body.audio;

      StringBuilder queryString = new StringBuilder();
      try {
        queryString.Append(CreateVertex("beat", beatId) +
          AddProperty("name", name) +
          AddProperty("duration", duration) +
          AddProperty("image", image) +
          AddProperty("isPrivate", isPrivate) +
          AddProperty("instrumentList", instrumentList) +
          AddProperty("createdDate", createdDate) +
          AddProperty("modifiedDate", modifiedDate) +
          AddProperty("attributes", attributes) +
          AddProperty("audio", audio));
      } catch {
        return BadRequest("Malformed Request");
      }

      if (body.seed != null) {
        queryString.Append(AddProperty("seed", body.seed));
      }

      queryString.Append(CreateEdge("OWNED_BY", email));

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString.ToString());
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("read")]
    public async Task<IActionResult> ReadBeat(dynamic req) {
      string request = Utility.GetRequest(req);
      var body = JsonConvert.DeserializeObject<dynamic>(request);

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
      string request = Utility.GetRequest(req);
      var body = JsonConvert.DeserializeObject<dynamic>(request);

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
      string request = Utility.GetRequest(req);
      var body = JsonConvert.DeserializeObject<dynamic>(request);

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