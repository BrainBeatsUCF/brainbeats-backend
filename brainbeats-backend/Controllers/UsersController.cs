using System;
using System.Threading.Tasks;
using Gremlin.Net.Driver;
using Gremlin.Net.Driver.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using static brainbeats_backend.GremlinQueries.UserQueries;

namespace brainbeats_backend.Controllers {
  /// <summary>Expected Form-Data request format for User Vertices.</summary>
  public class UserVertex {
    /// <summary>User first name.</summary>
    public string firstName { get; set; }

    /// <summary>User last name.</summary>
    public string lastName { get; set; }

    /// <summary>User profile picture. JPG or PNG only.</summary>
    public IFormFile image { get; set; }

    /// <summary>Constructor for manual User Vertex creation.</summary>
    public UserVertex(string firstName, string lastName) {
      this.firstName = firstName;
      this.lastName = lastName;
    }
  }

  /// <summary>
  /// UsersController.cs handles all user-specific logic on the api/v2/users route. Includes
  /// all CRUD operations on User vertices and edges between Users and other vertices.
  /// </summary>
  [Route("api/v2/users")]
  [ApiController]
  public class UsersController : ControllerBase {
    /// <summary>Searches for the User Vertex matching the inputted email and returns it.</summary>
    /// <returns>A length 1 array containing the User Vertex corresponding to the email.</returns>
    /// <remarks>Expected request route: GET api/v2/users/{email}.</remarks>
    /// <param name="email">User email.</param>
    [HttpGet("{email}")]
    public async Task<IActionResult> ReadUser(string email) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      email = email.ToLowerInvariant();

      try {
        ResultSet<dynamic> result = await ReadUserVertexQuery(email);
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
    /// Searches for the User Vertex matching the inputted email and returns all neighboring vertices
    /// that match a specific edge relationship and vertex type.
    /// </summary>
    /// <returns>An array of neighboring vertices that match the specified parameters.</returns>
    /// <remarks>Expected request route: GET api/v2/users/{email}/{relationship}?type=type.</remarks>
    /// <param name="email">User email.</param>
    /// <param name="relationship">Edge (relationship) label. Expects 'LIKES', 'RECOMMENDED', or 'OWNED_BY'. Case insensitive.</param>
    /// <param name="type">(Optional) Vertex type to filter by. Expects 'beat', 'playlist', or 'sample'. Case insensitive.</param>
    [HttpGet("{email}/{relationship}")]
    public async Task<IActionResult> ReadUserNeighbors(string email, string relationship, string type) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      email = email.ToLowerInvariant();

      try {
        return Ok(await GetUserVertexNeighborsQuery(email, relationship, type));
      } catch (ArgumentException e) {
        return BadRequest($"Malformed request: {e}");
      } catch (ResponseException e) {
        return BadRequest($"Error with Gremlin Query: {e}");
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    /// <summary>Creates a new User Vertex in the database.</summary>
    /// <returns>A length 1 array containing the new User Vertex.</returns>
    /// <remarks>Internal use only (not a web API). See <see cref="AuthController.Login(dynamic)"/> for use.</remarks>
    /// <param name="email">User email.</param>
    /// <param name="u">User object to create.</param>
    public async Task<IActionResult> CreateUser(string email, UserVertex u) {
      try {
        var result = await CreateUserVertexQuery(email, u);
        return Ok(result);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed request: {e}");
      } catch (ResponseException e) {
        return BadRequest($"Error with Gremlin Query: {e}");
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    /// <summary>Updates the User Vertex matching the inputted email; partial updates are supported.</summary>
    /// <returns>A length 1 array containing the updated User Vertex corresponding to the email.</returns>
    /// <remarks>Expected request route: PUT api/v2/users/update/{email}.</remarks>
    /// <param name="email">User email.</param>
    /// <param name="u">Form-Data request body content.</param>
    [HttpPut("update/{email}")]
    public async Task<IActionResult> UpdateUser(string email, [FromForm] UserVertex u) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      try {
        var result = await UpdateUserVertexQuery(email, u);
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
    /// Adds a new LIKES edge between the User Vertex corresponding to the inputted email and
    /// the target vertex corresponding to the inputted target vertex Id.
    /// </summary>
    /// <returns>A length 1 array containing the created edge object.</returns>
    /// <remarks>Expected request route: PUT api/v2/users/add_edge/{email}/{target}.</remarks>
    /// <param name="email">User email.</param>
    /// <param name="target">Target vertex Id.</param>
    [HttpPut("add_edge/{email}/{target}")]
    public async Task<IActionResult> AddEdge(string email, string target) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      try {
        var result = await AddEdgeToUserVertexQuery(email, target);
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
    /// Deletes the existing LIKES edge between the User Vertex corresponding to the inputted email and
    /// the target vertex corresponding to the inputted target vertex Id.
    /// </summary>
    /// <returns>An empty array.</returns>
    /// <remarks>Expected request route: DELETE api/v2/users/delete_edge/{email}/{target}.</remarks>
    /// <param name="email">User email.</param>
    /// <param name="target">Target vertex Id.</param>
    [HttpDelete("delete_edge/{email}/{target}")]
    public async Task<IActionResult> DeleteEdge(string email, string target) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      try {
        var result = await DeleteEdgeFromUserVertexQuery(email, target);
        return Ok(result);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed request: {e}");
      } catch (ResponseException e) {
        return BadRequest($"Error with Gremlin Query: {e}");
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    /// <summary>Deletes the User Vertex corresponding to the inputted email.</summary>
    /// <returns>An empty array.</returns>
    /// <remarks>Expected request route: DELETE api/v2/users/delete/{email}.</remarks>
    /// <param name="email">User email.</param>
    [HttpDelete("delete/{email}")]
    public async Task<IActionResult> DeleteUser(string email) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      try {
        var result = await DeleteUserVertexQuery(email);
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
