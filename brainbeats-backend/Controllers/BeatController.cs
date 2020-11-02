using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using static brainbeats_backend.QueryStrings;
using static brainbeats_backend.Utility;

namespace brainbeats_backend.Controllers {
  public class Beat {
    public string email { get; set; }
    public string id { get; set; }
    public string name { get; set; }
    public IFormFile image { get; set; }
    public bool isPrivate { get; set; }
    public string instrumentList { get; set; }
    public IFormFile audio { get; set; }
    public string attributes { get; set; }
    public int duration { get; set; }
    public string seed { get; set; }
  }

  [Route("api/[controller]")]
  [ApiController]
  public class BeatController : ControllerBase {
    [HttpPost]
    [Route("create_beat")]
    public async Task<IActionResult> CreateBeat([FromForm] Beat request) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }
      
      string queryString;

      try {
        List<KeyValuePair<string, string>> edges = new List<KeyValuePair<string, string>> {
          new KeyValuePair<string, string>("OWNED_BY", request.email)
        };

        queryString = await CreateVertexQueryAsync(request, edges);
      } catch (Exception e) {
        return BadRequest($"Malformed request: {e}");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    [HttpPost]
    [Route("read_beat")]
    public async Task<IActionResult> ReadBeat(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      JObject body = DeserializeRequest(req);
      string queryString;

      // Verify ownership
      try {
        if (!await ValidateVertexOwnershipAsync(body.GetValue("email").ToString().ToLowerInvariant(), body.GetValue("id").ToString().ToLowerInvariant())) {
          return BadRequest("User is not the owner of this private Beat");
        }
      } catch (Exception e) {
        return BadRequest($"Error validating ownership: {e}");
      }

      try {
        queryString = ReadVertexQuery(body.GetValue("id").ToString().ToLowerInvariant());
      } catch (Exception e) {
        return BadRequest($"Malformed request: {e}");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        List<dynamic> resultList = await PopulateVertexOwners(result);

        return Ok(resultList);
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    [HttpPost]
    [Route("search_beat")]
    public async Task<IActionResult> SearchBeat(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      JObject body = DeserializeRequest(req);
      string queryString;

      try {
        queryString = SearchVertexQuery("beat", body.GetValue("name").ToString().ToLower());
      } catch (Exception e) {
        return BadRequest($"Malformed request: {e}");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        List<dynamic> resultList = await PopulateVertexOwners(result);

        return Ok(resultList);
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    [HttpPost]
    [Route("search_public_beats")]
    public async Task<IActionResult> SearchPublicBeats(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      JObject body = DeserializeRequest(req);
      string queryString;

      try {
        queryString = SearchPublicVertexQuery("beat", body.GetValue("name").ToString().ToLowerInvariant());
      } catch (Exception e) {
        return BadRequest($"Malformed request: {e}");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        List<dynamic> resultList = await PopulateVertexOwners(result);

        return Ok(resultList);
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    [HttpPost]
    [Route("search_owned_beats")]
    public async Task<IActionResult> SearchOwnedBeats(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      JObject body = DeserializeRequest(req);
      string queryString;

      try {
        queryString = SearchOwnedVertexQuery("beat", body.GetValue("email").ToString().ToLowerInvariant(), body.GetValue("name").ToString().ToLowerInvariant());
      } catch (Exception e) {
        return BadRequest($"Malformed request: {e}");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        List<dynamic> resultList = await PopulateVertexOwners(result);

        return Ok(resultList);
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    [HttpPost]
    [Route("get_all_beats")]
    public async Task<IActionResult> GetAllBeats(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      JObject body = DeserializeRequest(req);
      string queryStringPublic;
      string queryStringPrivate;

      try {
        queryStringPublic = GetAllPublicVerticesQuery("beat");
        queryStringPrivate = GetAllPrivateVerticesQuery("beat", body.GetValue("email").ToString().ToLowerInvariant());
      } catch (Exception e) {
        return BadRequest($"Malformed request: {e}");
      }

      try {
        var resultsPublic = await DatabaseConnection.Instance.ExecuteQuery(queryStringPublic);
        var resultsPrivate = await DatabaseConnection.Instance.ExecuteQuery(queryStringPrivate);

        List<dynamic> resultListPublic = await PopulateVertexOwners(resultsPublic);
        List<dynamic> resultListPrivate = await PopulateVertexOwners(resultsPrivate);

        return Ok(resultListPublic.Concat(resultListPrivate));
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    [HttpPost]
    [Route("update_beat")]
    public async Task<IActionResult> UpdateBeat([FromForm] Beat b) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      string queryString;

      // Verify ownership
      try {
        if (!await ValidateVertexOwnershipAsync(b.email, b.id)) {
          return BadRequest("User is not the owner of this private Beat");
        }
      } catch (Exception e) {
        return BadRequest($"Error validating ownership: {e}");
      }

      try {
        queryString = await UpdateVertexQueryAsync(b);
      } catch (Exception e) {
        return BadRequest($"Malformed Request: {e}");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    [HttpPost]
    [Route("delete_beat")]
    public async Task<IActionResult> DeleteBeat(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      JObject body = DeserializeRequest(req);
      string queryString;

      // Verify ownership
      try {
        if (!await ValidateVertexOwnershipAsync(body.GetValue("email").ToString().ToLowerInvariant(), body.GetValue("id").ToString().ToLowerInvariant())) {
          return BadRequest("User is not the owner of this private Beat");
        }
      } catch (Exception e) {
        return BadRequest($"Error validating ownership: {e}");
      }

      try {
        // Delete the png or jpg picture associated with this Beat
        await StorageConnection.Instance.DeleteFileAsync("beat", body.GetValue("id").ToString() + "_image.jpg");
        await StorageConnection.Instance.DeleteFileAsync("beat", body.GetValue("id").ToString() + "_image.png");

        // Delete the wav or mp3 audio associated with this Beat
        await StorageConnection.Instance.DeleteFileAsync("beat", body.GetValue("id").ToString() + "_audio.wav");
        await StorageConnection.Instance.DeleteFileAsync("beat", body.GetValue("id").ToString() + "_audio.mp3");
      } catch (Exception e) {
        return BadRequest($"Error deleting associated storage uploads for Beat: {e}");
      }

      try {
        queryString = DeleteVertexQuery(body.GetValue("id").ToString().ToLowerInvariant());
      } catch (Exception e) {
        return BadRequest($"Malformed request: {e}");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }
  }
}