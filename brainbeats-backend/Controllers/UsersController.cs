using System;
using System.Threading.Tasks;
using Gremlin.Net.Driver;
using Gremlin.Net.Driver.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using static brainbeats_backend.GremlinQueries.UserQueries;

namespace brainbeats_backend.Controllers {
  public class UserVertex {
    public string firstName { get; set; }
    public string lastName { get; set; }
    public IFormFile image { get; set; }

    public UserVertex(string firstName, string lastName) {
      this.firstName = firstName;
      this.lastName = lastName;
    }
  }

  [Route("api/v2/users")]
  [ApiController]
  public class UsersController : ControllerBase {
    // GET api/v2/users/{email}
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

    // GET api/v2/users/{email}/{relationship}?type=type
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

    // Internal use only
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

    // PUT api/v2/users/update/{email}
    [HttpPut("update/{email}")]
    public async Task<IActionResult> UpdateUser(string email, UserVertex u) {
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

    // DELETE api/v2/users/delete/{email}
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
