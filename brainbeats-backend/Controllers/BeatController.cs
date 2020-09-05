using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static brainbeats_backend.Utility;

namespace brainbeats_backend.Controllers {
  [Route("api/[controller]")]
  [ApiController]
  public class BeatController : ControllerBase
  {
    [HttpPost]
    [Route("create_beat")]
    public async Task<IActionResult> CreateBeat(dynamic req) {
      string request = GetRequest(req);
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
          AddProperty("audio", audio) +
          CreateEdge("OWNED_BY", email));
      } catch {
        return BadRequest("Malformed Request");
      }

      if (body.seed != null) {
        queryString.Append(AddProperty("seed", body.seed, false));
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString.ToString());
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("read_beat")]
    public async Task<IActionResult> ReadBeat(dynamic req) {
      string request = GetRequest(req);
      var body = JsonConvert.DeserializeObject<dynamic>(request);

      string beatId = body.beatId;

      if (beatId == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = GetVertex(beatId);

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("update_beat")]
    public async Task<IActionResult> UpdateBeat(dynamic req) {
      string request = GetRequest(req);
      var body = JsonConvert.DeserializeObject<dynamic>(request);

      string beatId = body.beatId;

      string name = body.name;
      string duration = body.duration;
      string image = body.image;
      string isPrivate = body.isPrivate;
      string instrumentList = body.instrumentList;
      string modifiedDate = GetCurrentTime();
      string attributes = body.attributes;
      string audio = body.audio;

      if (beatId == null) {
        return BadRequest("Malformed Request");
      }

      StringBuilder queryString = new StringBuilder();
      bool required = false;

      try {
        queryString.Append(GetVertex(beatId) +
          AddProperty("name", name, required) +
          AddProperty("duration", duration, required) +
          AddProperty("image", image, required) +
          AddProperty("isPrivate", isPrivate, required) +
          AddProperty("instrumentList", instrumentList, required) +
          AddProperty("modifiedDate", modifiedDate) +
          AddProperty("attributes", attributes, required) +
          AddProperty("audio", audio, required));
      } catch {
        return BadRequest("Malformed Request");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString.ToString());
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("like_beat")]
    public async Task<IActionResult> LikeBeat(dynamic req) {
      string request = GetRequest(req);
      var body = JsonConvert.DeserializeObject<dynamic>(request);

      string beatId = body.beatId;
      string email = body.email;

      if (beatId == null || email == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = GetVertex(email) + CreateEdge("LIKES", beatId);

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("delete_beat")]
    public async Task<IActionResult> DeleteBeat(dynamic req) {
      string request = GetRequest(req);
      var body = JsonConvert.DeserializeObject<dynamic>(request);

      string beatId = body.beatId;

      if (beatId == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = GetVertex(beatId) + DeleteVertex();

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }
  }
}