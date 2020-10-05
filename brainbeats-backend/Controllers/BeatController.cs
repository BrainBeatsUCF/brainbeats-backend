using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using static brainbeats_backend.QueryStrings;
using static brainbeats_backend.Utility;

namespace brainbeats_backend.Controllers {
  [Route("api/[controller]")]
  [ApiController]
  public class BeatController : ControllerBase {
    [HttpPost]
    [Route("create_beat")]
    public async Task<IActionResult> CreateBeat(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch {
        return BadRequest("Unauthenticated");
      }

      JObject body = DeserializeRequest(req);
      string queryString;

      try {
        string beatId = Guid.NewGuid().ToString();
        List<KeyValuePair<string, string>> edges = new List<KeyValuePair<string, string>> {
          new KeyValuePair<string, string>("OWNED_BY", body.GetValue("email").ToString())
        };

        queryString = CreateVertexQuery("beat", beatId, body, edges);
      } catch {
        return BadRequest("Malformed request");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest("Something went wrong");
      }
    }

    [HttpPost]
    [Route("read_beat")]
    public async Task<IActionResult> ReadBeat(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch {
        return BadRequest("Unauthenticated");
      }

      JObject body = DeserializeRequest(req);
      string queryString;

      try {
        queryString = ReadVertexQuery(body.GetValue("beatId").ToString());
      } catch {
        return BadRequest("Malformed request");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest("Something went wrong");
      }
    }

    [HttpPost]
    [Route("get_all_beats")]
    public async Task<IActionResult> GetAllBeats(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch {
        return BadRequest("Unauthenticated");
      }

      JObject body = DeserializeRequest(req);
      string queryStringPublic;
      string queryStringPrivate;

      try {
        queryStringPublic = GetAllPublicVerticesQuery("beat");
        queryStringPrivate = GetAllPrivateVerticesQuery("beat", body.GetValue("email").ToString());
      } catch {
        return BadRequest("Malformed request");
      }

      try {
        var resultsPublic = await DatabaseConnection.Instance.ExecuteQuery(queryStringPublic);
        var resultsPrivate = await DatabaseConnection.Instance.ExecuteQuery(queryStringPrivate);

        List<dynamic> resultList = new List<dynamic>();

        foreach (var item in resultsPublic) {
          resultList.Add(item);
        }

        foreach (var item in resultsPrivate) {
          resultList.Add(item);
        }

        return Ok(resultList);
      } catch {
        return BadRequest("Something went wrong");
      }
    }

    [HttpPost]
    [Route("update_beat")]
    public async Task<IActionResult> UpdateBeat(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch {
        return BadRequest("Unauthenticated");
      }

      JObject body = DeserializeRequest(req);
      string queryString;

      try {
        queryString = UpdateVertexQuery("beat", body.GetValue("beatId").ToString(), body);
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
    [Route("delete_beat")]
    public async Task<IActionResult> DeleteBeat(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch {
        return BadRequest("Unauthenticated");
      }

      JObject body = DeserializeRequest(req);
      string queryString;

      try {
        queryString = DeleteVertexQuery(body.GetValue("beatId").ToString());
      } catch {
        return BadRequest("Malformed request");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest("Something went wrong");
      }
    }
  }
}