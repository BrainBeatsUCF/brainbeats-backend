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
  /// <summary>Expected Form-Data request format for Playlist Vertices.</summary>
  public class PlaylistVertex {
    /// <summary>Playlist name.</summary>
    public string name { get; set; }

    /// <summary>Playlist thumbnail image. JPG or PNG only.</summary>
    public IFormFile image { get; set; }

    /// <summary>
    /// Playlist privacy toggle. True for private Playlists, 
    /// False for publicly accessible Playlists.
    /// </summary>
    public bool isPrivate { get; set; }
  }

  /// <summary>
  /// PlaylistsController.cs handles all playlist-specific logic on the api/v2/playlists route. Includes
  /// all CRUD operations on Playlist vertices and edges between Playlists and Beats.
  /// </summary>
  [Route("api/v2/playlists")]
  [ApiController]
  public class PlaylistsController : ControllerBase {
    /// <summary>Searches for the Playlist Vertex matching the inputted playlistId and returns it.</summary>
    /// <returns>A length 1 array containing the Playlist Vertex.</returns>
    /// <remarks>Expected request route: GET api/v2/playlists/{playlistId}.</remarks>
    /// <param name="playlistId">Playlist Vertex Id.</param>
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

    /// <summary>
    /// Returns Playlists matching specified parameters. If name is specified, searches for public Playlists 
    /// that match the name; else returns all Playlists. If email is specified, private Playlists belonging 
    /// to the user is also returned; else only public Playlists are returned.
    /// </summary>
    /// <returns>An array of Playlist vertices that match the specified parameters.</returns>
    /// <remarks>Expected request route: GET api/v2/playlists?name=name&email=email.</remarks>
    /// <param name="name">(Optional) Playlist name to search.</param>
    /// <param name="email">(Optional) Email of the user executing the search.</param>
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

    /// <summary>Creates a new Playlist owned by the user with the inputted email.</summary>
    /// <returns>A length 1 array containing the created Playlist Vertex owned by the user.</returns>
    /// <remarks>Expected request route: POST api/v2/playlists/create/{email}?seed=seed.</remarks>
    /// <param name="email">User email.</param>
    /// /// <param name="p">Form-Data request body content.</param>
    /// /// <param name="seed">(Optional) Database seed used for dev testing only.</param>
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

    /// <summary>Adds a Beat to the Playlist.</summary>
    /// <returns>A length 1 array containing created edge object.</returns>
    /// <remarks>Expected request route: PUT api/v2/playlists/add_beats/{playlistId}.</remarks>
    /// <param name="playlistId">Playlist Vertex Id.</param>
    /// <param name="req">JSON body containing the 'beatId' field.</param>
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

    /// <summary>Removes a Beat from the Playlist.</summary>
    /// <returns>An empty array.</returns>
    /// <remarks>Expected request route: PUT api/v2/playlists/remove_beats/{playlistId}.</remarks>
    /// <param name="playlistId">Playlist Vertex Id.</param>
    /// <param name="req">JSON body containing the 'beatId' field.</param>
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

    /// <summary>Updates the Playlist Vertex matching the inputted playlistId; partial updates are supported.</summary>
    /// <returns>A length 1 array containing the updated Playlist Vertex.</returns>
    /// <remarks>Expected request route: PUT api/v2/playlists/update/{playlistId}.</remarks>
    /// <param name="playlistId">Playlist Vertex Id.</param>
    /// <param name="p">Form-Data request body content.</param>
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

    /// <summary>Deletes the Playlist Vertex corresponding to the inputted playlistId.</summary>
    /// <returns>An empty array.</returns>
    /// <remarks>Expected request route: DELETE api/v2/playlists/delete/{playlistId}.</remarks>
    /// <param name="playlistId">Playlist Vertex Id.</param>
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

