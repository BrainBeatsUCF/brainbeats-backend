using System;
using System.Threading.Tasks;
using Gremlin.Net.Driver.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using static brainbeats_backend.GremlinQueries.BeatQueries;

namespace brainbeats_backend.Controllers {
  /// <summary>Expected Form-Data request format for Beat Vertices.</summary>
  public class BeatVertex {
    /// <summary>Beat name.</summary>
    public string name { get; set; }

    /// <summary>Beat thumbnail image. JPG or PNG only.</summary>
    public IFormFile image { get; set; }

    /// <summary>
    /// Beat privacy toggle. True for private Beats, 
    /// False for publicly accessible Beats.
    /// </summary>
    public bool isPrivate { get; set; }

    /// <summary>
    /// JSON-stringified array of strings containing the instruments
    /// used in the Beat.
    /// </summary>
    public string instrumentList { get; set; }

    /// <summary>Beat audio file. WAV or MP3 only.</summary>
    public IFormFile audio { get; set; }

    /// <summary>
    /// JSON-stringified array of objects containing Beat metadata defined
    /// in the Brain Beats desktop application.
    /// </summary>
    public string attributes { get; set; }

    /// <summary>Beat duration in seconds.</summary>
    public float duration { get; set; }
  }

  /// <summary>
  /// BeatsController.cs handles all beat-specific logic on the api/v2/beats route. Includes
  /// all CRUD operations on Beat vertices.
  /// </summary>
  [Route("api/v2/beats")]
  [ApiController]
  public class BeatsController : ControllerBase {
    /// <summary>Searches for the Beat Vertex matching the inputted beatId and returns it.</summary>
    /// <returns>A length 1 array containing the Beat Vertex.</returns>
    /// <remarks>Expected request route: GET api/v2/beats/{beatId}.</remarks>
    /// <param name="beatId">Beat Vertex Id.</param>
    [HttpGet("{beatId}")]
    public async Task<IActionResult> ReadBeat(string beatId) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      try {
        var result = await ReadBeatVertexQuery(beatId);
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
    /// Returns Beats matching specified parameters. If name is specified, searches for public Beats 
    /// that match the name; else returns all Beats. If email is specified, private Beats belonging 
    /// to the user is also returned; else only public Beats are returned.
    /// </summary>
    /// <returns>An array of Beat vertices that match the specified parameters.</returns>
    /// <remarks>Expected request route: GET api/v2/beats?name=name&email=email.</remarks>
    /// <param name="name">(Optional) Beat name to search.</param>
    /// <param name="email">(Optional) Email of the user executing the search.</param>
    [HttpGet("")]
    public async Task<IActionResult> GetBeats(string name, string email) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }
      
      try {
        var result = await SearchBeatVertexQuery(name, email);
        return Ok(result);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed request: {e}");
      } catch (ResponseException e) {
        return BadRequest($"Error with Gremlin Query: {e}");
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    /// <summary>Creates a new Beat owned by the user with the inputted email.</summary>
    /// <returns>A length 1 array containing the created Beat Vertex owned by the user.</returns>
    /// <remarks>Expected request route: POST api/v2/beats/create/{email}?seed=seed.</remarks>
    /// <param name="email">User email.</param>
    /// /// <param name="b">Form-Data request body content.</param>
    /// /// <param name="seed">(Optional) Database seed used for dev testing only.</param>
    [HttpPost("create/{email}")]
    public async Task<IActionResult> CreateBeat(string email, string seed, [FromForm] BeatVertex b) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      try {
        var result = await CreateBeatVertexQuery(email, b, seed);
        return Ok(result);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed request: {e}");
      } catch (ResponseException e) {
        return BadRequest($"Error with Gremlin Query: {e}");
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    /// <summary>Updates the Beat Vertex matching the inputted beatId; partial updates are supported.</summary>
    /// <returns>A length 1 array containing the updated Beat Vertex.</returns>
    /// <remarks>Expected request route: PUT api/v2/beats/update/{beatId}.</remarks>
    /// <param name="beatId">Beat Vertex Id.</param>
    /// <param name="b">Form-Data request body content.</param>
    [HttpPut("update/{beatId}")]
    public async Task<IActionResult> UpdateBeat(string beatId, [FromForm] BeatVertex b) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      try {
        var result = await UpdateBeatVertexQuery(beatId, b);
        return Ok(result);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed request: {e}");
      } catch (ResponseException e) {
        return BadRequest($"Error with Gremlin Query: {e}");
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    /// <summary>Deletes the Beat Vertex corresponding to the inputted beatId.</summary>
    /// <returns>An empty array.</returns>
    /// <remarks>Expected request route: DELETE api/v2/beats/delete/{beatId}.</remarks>
    /// <param name="beatId">Beat Vertex Id.</param>
    [HttpDelete("delete/{beatId}")]
    public async Task<IActionResult> DeleteBeat(string beatId) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      try {
        var result = await DeleteBeatVertexQuery(beatId);
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
