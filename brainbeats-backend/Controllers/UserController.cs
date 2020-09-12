using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static brainbeats_backend.QueryBuilder;
using static brainbeats_backend.QueryStrings;
using static brainbeats_backend.Utility;

namespace brainbeats_backend.Controllers {
  [Route("api/[controller]")]
  [ApiController]
  public class UserController : ControllerBase
  {
    HashSet<string> schema = GetSchema("user");

    [HttpPost]
    [Route("create_user")]
    public async Task<IActionResult> CreateUser(dynamic req) {
      JObject body = DeserializeRequest(req);

      if (!body.ContainsKey("email")) {
        return BadRequest("Malformed request");
      }

      string queryString = CreateVertexQuery("user", body.GetValue("email").ToString(), body);

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest("Something went wrong");
      }
    }

    [HttpPost]
    [Route("read_user")]
    public async Task<IActionResult> ReadUser(dynamic req) {
      JObject body = DeserializeRequest(req);

      if (!body.ContainsKey("email")) {
        return BadRequest("Malformed request");
      }

      string queryString = ReadVertexQuery(body.GetValue("email").ToString());

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest("Something went wrong");
      }
    }

    [HttpPost]
    [Route("update_user")]
    public async Task<IActionResult> UpdateUser(dynamic req) {
      JObject body = DeserializeRequest(req);

      if (!body.ContainsKey("email")) {
        return BadRequest("Malformed Request");
      }

      string queryString;

      try {
        queryString = UpdateVertexQuery("user", body.GetValue("email").ToString(), body));
      } catch {
        return BadRequest("Malformed Request");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest("Something went wrong");
      }
    }

    [HttpPost]
    [Route("delete_user")]
    public async Task<IActionResult> DeleteUser(dynamic req) {
      JObject body = DeserializeRequest(req);
      return await new BaseGremlinTemplate().DeleteVertex(body.GetValue("id"));
    }

    [HttpPost]
    [Route("get_liked_beats")]
    public async Task<IActionResult> GetLikedBeats(dynamic req) {
      var body = DeserializeRequest(req);

      string email = body.email;

      if (email == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = GetLikedVerticesQuery("beat", email);

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("get_owned_beats")]
    public async Task<IActionResult> GetOwnedBeats(dynamic req) {
      var body = DeserializeRequest(req);

      string email = body.email;

      if (email == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = GetOwnedVerticesQuery("beat", email);

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("like_vertex")]
    public async Task<IActionResult> LikeVertex(dynamic req) {
      var body = DeserializeRequest(req);

      string vertexId = body.vertexId;
      string email = body.email;

      if (vertexId == null || email == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = GetVertex(email) +
        CreateEdge("LIKES", vertexId);

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("unlike_vertex")]
    public async Task<IActionResult> UnlikeVertex(dynamic req) {
      var body = DeserializeRequest(req);

      string vertexId = body.vertexId;
      string email = body.email;

      if (vertexId == null || email == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = GetVertex(email) +
        GetEdge("LIKES", vertexId) +
        Delete();

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }
  }
}