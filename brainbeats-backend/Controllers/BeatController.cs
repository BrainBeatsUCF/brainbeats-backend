using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static brainbeats_backend.QueryBuilder;
using static brainbeats_backend.QueryStrings;
using static brainbeats_backend.Utility;

namespace brainbeats_backend.Controllers {
  [Route("api/[controller]")]
  [ApiController]
  public class BeatController : ControllerBase
  {
    [HttpPost]
    [Route("create_beat")]
    public async Task<IActionResult> CreateBeat(dynamic req) {
      JObject body = DeserializeRequest(req);

      if (!body.ContainsKey("email")) {
        return BadRequest("Malformed Request");
      }

      string beatId = Guid.NewGuid().ToString();

      StringBuilder queryString = new StringBuilder();

      try {
        queryString.Append(CreateVertexQuery("beat", beatId, body) + 
          CreateEdge("OWNED_BY", body.GetValue("email").ToString()));
      } catch {
        return BadRequest("Malformed Request");
      }

      if (body.GetValue("seed") != null) {
        queryString.Append(EdgeSourceReference() + AddProperty("seed", body.GetValue("seed").ToString()));
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
      JObject body = DeserializeRequest(req);

      if (!body.ContainsKey("id")) {
        return BadRequest("Malformed Request");
      }

      string queryString = ReadVertexQuery(body.GetValue("id").ToString());

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("get_all_beats")]
    public async Task<IActionResult> GetAllBeats(dynamic req) {
      JObject body = DeserializeRequest(req);

      if (!body.ContainsKey("email")) {
        return BadRequest("Malformed Request");
      }

      string queryStringPublic = GetAllPublicVerticesQuery("beat");
      string queryStringPrivate = GetAllPrivatelyOwnedVerticesQuery("beat", body.GetValue("email").ToString());

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
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("update_beat")]
    public async Task<IActionResult> UpdateBeat(dynamic req) {
      JObject body = DeserializeRequest(req);

      if (!body.ContainsKey("id")) {
        return BadRequest("Malformed Request");
      }

      string queryString;

      try {
        queryString = UpdateVertexQuery("beat", body.GetValue("id").ToString(), body));
      } catch {
        return BadRequest("Malformed Request");
      }

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
      JObject body = DeserializeRequest(req);

      if (!body.ContainsKey("id")) {
        return BadRequest("Malformed Request");
      }

      string queryString = DeleteVertexQuery(body.GetValue("id").ToString());

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }
  }
}