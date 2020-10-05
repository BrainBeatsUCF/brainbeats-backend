using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using static brainbeats_backend.QueryStrings;
using static brainbeats_backend.Utility;

namespace brainbeats_backend.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class PlaylistController : ControllerBase
  {
    [HttpPost]
    [Route("create_playlist")]
    public async Task<IActionResult> CreatePlaylist(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch {
        return BadRequest("Unauthenticated");
      }

      JObject body = DeserializeRequest(req);
      string queryString;

      try {
        string playlistId = Guid.NewGuid().ToString();
        List<KeyValuePair<string, string>> edges = new List<KeyValuePair<string, string>> {
          new KeyValuePair<string, string>("OWNED_BY", body.GetValue("email").ToString())
        };

        if (body.ContainsKey("beatId")) {
          edges.Add(new KeyValuePair<string, string>("CONTAINS", body.GetValue("beatId").ToString()));
        }

        queryString = CreateVertexQuery("playlist", playlistId, body, edges);
      } catch {
        return BadRequest("Malformed request");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString.ToString());
        return Ok(result);
      } catch {
        return BadRequest("Something went wrong");
      }
    }

    [HttpPost]
    [Route("read_playlist")]
    public async Task<IActionResult> ReadPlaylist(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch {
        return BadRequest("Unauthenticated");
      }

      JObject body = DeserializeRequest(req);
      string queryString;

      try {
        queryString = ReadVertexQuery(body.GetValue("playlistId").ToString());
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
    [Route("read_playlist_beats")]
    public async Task<IActionResult> ReadPlaylistBeats(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch {
        return BadRequest("Unauthenticated");
      }

      JObject body = DeserializeRequest(req);
      string queryString;

      try {
        queryString = GetOutNeighborsQuery("beat", "CONTAINS", body.GetValue("playlistId").ToString());
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
    [Route("get_all_playlists")]
    public async Task<IActionResult> GetAllPlaylists(dynamic req) {
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
        queryStringPublic = GetAllPublicVerticesQuery("playlist");
        queryStringPrivate = GetAllPrivateVerticesQuery("playlist", body.GetValue("email").ToString());
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
    [Route("update_playlist")]
    public async Task<IActionResult> UpdatePlaylist(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch {
        return BadRequest("Unauthenticated");
      }

      JObject body = DeserializeRequest(req);
      string queryString;

      try {
        queryString = UpdateVertexQuery("playlist", body.GetValue("playlistId").ToString(), body);
      } catch {
        return BadRequest("Malformed Request");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString.ToString());
        return Ok(result);
      } catch {
        return BadRequest("Something went wrong");
      }
    }

    [HttpPost]
    [Route("update_playlist_delete_beat")]
    public async Task<IActionResult> UpdatePlaylistDeleteBeat(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch {
        return BadRequest("Unauthenticated");
      }

      JObject body = DeserializeRequest(req);
      string queryString;

      try {
        queryString = DeleteOutNeighborQuery("CONTAINS", body.GetValue("playlistId").ToString(), body.GetValue("beatId").ToString());
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
    [Route("update_playlist_add_beat")]
    public async Task<IActionResult> UpdatePlaylistAddBeat(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch {
        return BadRequest("Unauthenticated");
      }

      JObject body = DeserializeRequest(req);
      string queryString;

      try {
        queryString = CreateOutNeighborQuery("CONTAINS", body.GetValue("playlistId").ToString(), body.GetValue("beatId").ToString());
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
    [Route("delete_playlist")]
    public async Task<IActionResult> DeletePlaylist(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch {
        return BadRequest("Unauthenticated");
      }

      JObject body = DeserializeRequest(req);
      string queryString;

      try {
        queryString = DeleteVertexQuery(body.GetValue("playlistId").ToString());
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