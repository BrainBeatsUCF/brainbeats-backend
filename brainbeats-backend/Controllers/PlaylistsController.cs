using System;
using System.Threading.Tasks;
using Gremlin.Net.Driver.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using static brainbeats_backend.GremlinQueries.PlaylistQueries;
using static brainbeats_backend.Utility;

namespace brainbeats_backend.Controllers {
  public class PlaylistVertex {
    public string name { get; set; }
    public IFormFile image { get; set; }
    public bool isPrivate { get; set; }
  }

  [Route("api/v2/playlists")]
  [ApiController]
  public class PlaylistsController : ControllerBase {
    // GET api/v2/playlists/{playlistId}
    [HttpGet("{playlistId}")]
    public async Task<IActionResult> ReadPlaylist(string playlistId) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      try {
        var result = await ReadPlaylistVertexQuery(playlistId);
        return Ok(result);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed request: {e}");
      } catch (ResponseException e) {
        return BadRequest($"Error with Gremlin Query: {e}");
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    // GET api/v2/playlists?name=name&email=email
    [HttpGet("")]
    public async Task<IActionResult> GetPlaylists(string name, string email) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      try {
        var result = await SearchPlaylistVertexQuery(name, email);
        return Ok(result);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed request: {e}");
      } catch (ResponseException e) {
        return BadRequest($"Error with Gremlin Query: {e}");
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    // POST api/v2/playlists/create/{email}?seed=seed
    [HttpPost("create/{email}")]
    public async Task<IActionResult> CreatePlaylist(string email, string seed, [FromForm] PlaylistVertex p) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      try {
        var result = await CreatePlaylistVertexQuery(email, p, seed);
        return Ok(result);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed request: {e}");
      } catch (ResponseException e) {
        return BadRequest($"Error with Gremlin Query: {e}");
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    // PUT api/v2/playlists/add_beats/{playlistId}
    [HttpPut("add_beats/{playlistId}")]
    public async Task<IActionResult> UpdatePlaylistAddBeats(string playlistId, dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      try {
        JObject body = DeserializeRequest(req);

        if (!body.ContainsKey("beatId")) {
          return BadRequest("Request body must contain 'beatId'");
        }

        var result = await UpdatePlaylistAddBeat(playlistId, body.GetValue("beatId").ToString());
        return Ok(result);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed request: {e}");
      } catch (ResponseException e) {
        return BadRequest($"Error with Gremlin Query: {e}");
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    // PUT api/v2/playlists/remove_beats/{playlistId}
    [HttpPut("remove_beats/{playlistId}")]
    public async Task<IActionResult> UpdatePlaylistRemoveBeats(string playlistId, dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      try {
        JObject body = DeserializeRequest(req);

        if (!body.ContainsKey("beatId")) {
          return BadRequest("Request body must contain 'beatId'");
        }

        var result = await UpdatePlaylistRemoveBeat(playlistId, body.GetValue("beatId").ToString());
        return Ok(result);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed request: {e}");
      } catch (ResponseException e) {
        return BadRequest($"Error with Gremlin Query: {e}");
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    // PUT api/v2/playlists/update/{playlistId}
    [HttpPut("update/{playlistId}")]
    public async Task<IActionResult> UpdatePlaylist(string playlistId, [FromForm] PlaylistVertex p) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      try {
        var result = await UpdatePlaylistVertexQuery(playlistId, p);
        return Ok(result);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed request: {e}");
      } catch (ResponseException e) {
        return BadRequest($"Error with Gremlin Query: {e}");
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    // DELETE api/v2/playlists/delete/{playlistId}
    [HttpDelete("delete/{playlistId}")]
    public async Task<IActionResult> DeletePlaylist(string playlistId) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      try {
        var result = await DeletePlaylistVertexQuery(playlistId);
        return Ok(result);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed request: {e}");
      } catch (ResponseException e) {
        return BadRequest($"Error with Gremlin Query: {e}");
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }
  }
}

