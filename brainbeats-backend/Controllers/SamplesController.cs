using System;
using System.Threading.Tasks;
using Gremlin.Net.Driver.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using static brainbeats_backend.GremlinQueries.SampleQueries;

namespace brainbeats_backend.Controllers {
  /// <summary>Expected Form-Data request format for Sample Vertices.</summary>
  public class SampleVertex {
    /// <summary>Sample name.</summary>
    public string name { get; set; }

    /// <summary>
    /// Sample privacy toggle. True for private Samples, 
    /// False for publicly accessible Samples.
    /// </summary>
    public bool isPrivate { get; set; }

    /// <summary>
    /// JSON-stringified array of objects containing Sample metadata defined
    /// in the Brain Beats desktop application.
    /// </summary>
    public string attributes { get; set; }

    /// <summary>Sample audio file. WAV or MP3 only.</summary>
    public IFormFile audio { get; set; }

    /// <summary>Sample thumbnail image. JPG or PNG only.</summary>
    public string image { get; set; }
  }

  /// <summary>
  /// SamplesController.cs handles all sample-specific logic on the api/v2/samples route. Includes
  /// all CRUD operations on Sample vertices.
  /// </summary>
  [Route("api/v2/samples")]
  [ApiController]
  public class SamplesController : ControllerBase {
    /// <summary>Searches for the Sample Vertex matching the inputted sampleId and returns it.</summary>
    /// <returns>A length 1 array containing the Sample Vertex.</returns>
    /// <remarks>Expected request route: GET api/v2/samples/{sampleId}.</remarks>
    /// <param name="sampleId">Sample Vertex Id.</param>
    [HttpGet("{sampleId}")]
    public async Task<IActionResult> ReadSample(string sampleId) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      try {
        var result = await ReadSampleVertexQuery(sampleId);
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
    /// Returns Samples matching specified parameters. If name is specified, searches for public Samples 
    /// that match the name; else returns all Samples. If email is specified, private Samples belonging 
    /// to the user is also returned; else only public Samples are returned.
    /// </summary>
    /// <returns>An array of Sample vertices that match the specified parameters.</returns>
    /// <remarks>Expected request route: GET api/v2/samples?name=name&email=email.</remarks>
    /// <param name="name">(Optional) Sample name to search.</param>
    /// <param name="email">(Optional) Email of the user executing the search.</param>
    [HttpGet("")]
    public async Task<IActionResult> GetSamples(string name, string email) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      try {
        var result = await SearchSampleVertexQuery(name, email);
        return Ok(result);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed request: {e}");
      } catch (ResponseException e) {
        return BadRequest($"Error with Gremlin Query: {e}");
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    /// <summary>Creates a new Sample owned by the user with the inputted email.</summary>
    /// <returns>A length 1 array containing the created Sample Vertex owned by the user.</returns>
    /// <remarks>Expected request route: POST api/v2/samples/create/{email}?seed=seed.</remarks>
    /// <param name="email">User email.</param>
    /// /// <param name="s">Form-Data request body content.</param>
    /// /// <param name="seed">(Optional) Database seed used for dev testing only.</param>
    [HttpPost("create/{email}")]
    public async Task<IActionResult> CreateSample(string email, string seed, [FromForm] SampleVertex s) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      try {
        var result = await CreateSampleVertexQuery(email, s, seed);
        return Ok(result);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed request: {e}");
      } catch (ResponseException e) {
        return BadRequest($"Error with Gremlin Query: {e}");
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    /// <summary>Updates the Sample Vertex matching the inputted sampleId; partial updates are supported.</summary>
    /// <returns>A length 1 array containing the updated Sample Vertex.</returns>
    /// <remarks>Expected request route: PUT api/v2/samples/update/{sampleId}.</remarks>
    /// <param name="sampleId">Sample Vertex Id.</param>
    /// <param name="s">Form-Data request body content.</param>
    // PUT api/v2/samples/update/{sampleId}
    [HttpPut("update/{sampleId}")]
    public async Task<IActionResult> UpdateSample(string sampleId, [FromForm] SampleVertex s) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      try {
        var result = await UpdateSampleVertexQuery(sampleId, s);
        return Ok(result);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed request: {e}");
      } catch (ResponseException e) {
        return BadRequest($"Error with Gremlin Query: {e}");
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    /// <summary>Deletes the Sample Vertex corresponding to the inputted sampleId.</summary>
    /// <returns>An empty array.</returns>
    /// <remarks>Expected request route: DELETE api/v2/samples/delete/{sampleId}.</remarks>
    /// <param name="sampleId">Sample Vertex Id.</param>
    [HttpDelete("delete/{sampleId}")]
    public async Task<IActionResult> DeleteSample(string sampleId) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      try {
        var result = await DeleteSampleVertexQuery(sampleId);
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