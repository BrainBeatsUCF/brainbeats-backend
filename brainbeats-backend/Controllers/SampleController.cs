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
  public class SampleController : ControllerBase {
    [HttpPost]
    [Route("create_sample")]
    public async Task<IActionResult> CreateSample(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return BadRequest($"Unauthenticated error: {e}");
      }

      JObject body = DeserializeRequest(req);
      string queryString;

      try {
        string sampleId = Guid.NewGuid().ToString();
        List<KeyValuePair<string, string>> edges = new List<KeyValuePair<string, string>> {
          new KeyValuePair<string, string>("OWNED_BY", body.GetValue("email").ToString())
        };

        queryString = CreateVertexQuery("sample", sampleId, body, edges);
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
    [Route("read_sample")]
    public async Task<IActionResult> ReadSample(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return BadRequest($"Unauthenticated error: {e}");
      }

      JObject body = DeserializeRequest(req);
      string queryString;

      try {
        queryString = ReadVertexQuery(body.GetValue("sampleId").ToString());
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
    [Route("get_all_samples")]
    public async Task<IActionResult> GetAllSamples(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return BadRequest($"Unauthenticated error: {e}");
      }

      JObject body = DeserializeRequest(req);
      string queryStringPublic;
      string queryStringPrivate;

      try {
        queryStringPublic = GetAllPublicVerticesQuery("sample");
        queryStringPrivate = GetAllPrivateVerticesQuery("sample", body.GetValue("email").ToString());
      } catch {
        return BadRequest("Malformed Request");
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
    [Route("update_sample")]
    public async Task<IActionResult> UpdateSample(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return BadRequest($"Unauthenticated error: {e}");
      }

      JObject body = DeserializeRequest(req);
      string queryString;

      try {
        queryString = UpdateVertexQuery("sample", body.GetValue("sampleId").ToString(), body);
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
    [Route("delete_sample")]
    public async Task<IActionResult> DeleteSample(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return BadRequest($"Unauthenticated error: {e}");
      }

      JObject body = DeserializeRequest(req);
      string queryString;

      try {
        queryString = DeleteVertexQuery(body.GetValue("sampleId").ToString());
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
