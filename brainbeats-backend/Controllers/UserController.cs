using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using static brainbeats_backend.QueryBuilder;
using static brainbeats_backend.QueryStrings;
using static brainbeats_backend.Utility;

namespace brainbeats_backend.Controllers {
  public class User {
    public string email { get; set; }
    public string firstName { get; set; }
    public string lastName { get; set; }
    public string name { get; set; }
    public string seed { get; set; }
  }

  public class UserProfileSettings {
    public string email { get; set; }
    public IFormFile image { get; set; }
  }

  [Route("api/[controller]")]
  [ApiController]
  public class UserController : ControllerBase {
    private readonly string defaultProfilePicture = "DEFAULT_PROFILE_PLACEHOLDER";

    [HttpPost]
    [Route("login_user")]
    public async Task<IActionResult> LoginUser(dynamic req) {
      JObject body = DeserializeRequest(req);

      string res;
      try {
        res = await AuthConnection.Instance.LoginUser(body.GetValue("email").ToString(),
         body.GetValue("password").ToString());
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }

      // If the user exists in Azure B2C but doesn't exist in the database, create the user's profile
      // First, get the user's claims from the generated JWT
      JObject tokenObject = DeserializeRequest(res);

      if (tokenObject.ContainsKey("error")) {
        return Unauthorized(tokenObject.GetValue("error_description").ToString());
      }

      JwtSecurityToken jwt = AuthConnection.Instance.DecodeToken(tokenObject.GetValue("access_token").ToString());

      Dictionary<string, string> claimsDictionary = new Dictionary<string, string>();
      foreach (Claim claim in jwt.Claims) {
        claimsDictionary[claim.Type] = claim.Value;
      }

      try {
        // See if the user exists in the database
        string queryString = ReadVertexQuery(claimsDictionary["emails"]);
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);

        // If the user exists, return Ok()
        if (result.Count > 0) {
          return Ok(res);
        }

        // Else, create the user
        JObject user = new JObject(
          new JProperty("firstName", claimsDictionary["given_name"]),
          new JProperty("lastName", claimsDictionary["family_name"]),
          new JProperty("email", claimsDictionary["emails"]));

        IActionResult createUserResult = await CreateUser(user.ToString()).ConfigureAwait(false);
        OkObjectResult okResult = createUserResult as OkObjectResult;

        if (okResult.StatusCode != 200) {
          return BadRequest("Error creating new user vertex when signing in user for the first time");
        }

        return Ok(res);

      } catch (Exception e) {
        return BadRequest($"Unknown error signing user for the first time: {e}");
      }
    }

    [HttpPost]
    [Route("refresh_token")]
    public async Task<IActionResult> RefreshToken(dynamic req) {
      JObject body = DeserializeRequest(req);

      string res;
      try {
        res = await AuthConnection.Instance.RefreshToken(body.GetValue("refreshToken").ToString());
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }

      return Ok(res);
    }

    // Not a publicly accessible API
    public async Task<IActionResult> CreateUser(dynamic req) {
      User u = DeserializeRequest(req, new User());
      string queryString;

      // User's name field is populated through concatenating the first and last names
      u.name = $"{u.firstName} {u.lastName}";

      try {
        queryString = await CreateVertexQueryAsync(u) + AddProperty("image", defaultProfilePicture);
      } catch {
        return BadRequest("Malformed request");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }
    }

    [HttpPost]
    [Route("read_user")]
    public async Task<IActionResult> ReadUser(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      User u = DeserializeRequest(req, new User());
      string queryString;

      try {
        queryString = ReadVertexQuery(u.email);
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
    [Route("search_user")]
    public async Task<IActionResult> SearchUser(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      User u = DeserializeRequest(req, new User());
      string queryString;

      try {
        queryString = SearchVertexQuery("user", u.name.ToLowerInvariant());
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
    [Route("update_user")]
    public async Task<IActionResult> UpdateUser(dynamic req) {
      try {
        HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
        AuthConnection.Instance.ValidateToken(authorizationToken);
      } catch (ArgumentException e) {
        return BadRequest($"Malformed or missing authorization token: {e}");
      } catch (Exception e) {
        return Unauthorized($"Unauthenticated error: {e}");
      }

      User u = DeserializeRequest(req, new User());
      string queryString;

      // User's name field is populated through concatenating the first and last names
      // Important: This means that the firstName and lastName fields are required for all requests, even
      // if the user only wants to update the first name by itself
      u.name = $"{u.firstName} {u.lastName}";

      try {
        queryString = await UpdateVertexQueryAsync(u);
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
    [Route("upload_profile_picture")]
    public async Task<IActionResult> UpdateProfilePicture([FromForm] UserProfileSettings request) {
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
        string extension = Path.GetExtension(request.image.FileName);

        // Reject improper file extensions
        if (!extension.ToLowerInvariant().Equals(".jpg") && !extension.ToLowerInvariant().Equals(".png")) {
          return BadRequest("Image file extension must be jpg or png");
        }

        string url = await StorageConnection.Instance.UploadFileAsync(request.image, "user", request.email + "_image");
        queryString = GetVertex(request.email) + AddProperty("image", url);
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
    [Route("delete_profile_picture")]
    public async Task<IActionResult> DeleteProfilePicture([FromForm] UserProfileSettings request) {
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
        await StorageConnection.Instance.DeleteFileAsync("user", request.email + "_image");
        queryString = GetVertex(request.email) + AddProperty("image", defaultProfilePicture);
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
    [Route("delete_user")]
    public async Task<IActionResult> DeleteUser(dynamic req) {
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
        await StorageConnection.Instance.DeleteFileAsync("user", body.GetValue("email").ToString() + "_image");
      } catch (Exception e) {
        return BadRequest($"Error deleting associated storage uploads for User: {e}");
      }

      try {
        queryString = DeleteVertexQuery(body.GetValue("email").ToString());
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
    [Route("get_liked_beats")]
    public async Task<IActionResult> GetLikedBeats(dynamic req) {
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
        queryString = GetOutNeighborsQuery("beat", "LIKES", body.GetValue("email").ToString());
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
    [Route("get_liked_playlists")]
    public async Task<IActionResult> GetLikedPlaylists(dynamic req) {
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
        queryString = GetOutNeighborsQuery("playlist", "LIKES", body.GetValue("email").ToString());
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
    [Route("get_liked_samples")]
    public async Task<IActionResult> GetLikedSamples(dynamic req) {
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
        queryString = GetOutNeighborsQuery("sample", "LIKES", body.GetValue("email").ToString());
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
    [Route("get_owned_beats")]
    public async Task<IActionResult> GetOwnedBeats(dynamic req) {
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
        queryString = GetInNeighborsQuery("beat", "OWNED_BY", body.GetValue("email").ToString());
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
    [Route("get_owned_playlists")]
    public async Task<IActionResult> GetOwnedPlaylists(dynamic req) {
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
        queryString = GetInNeighborsQuery("playlist", "OWNED_BY", body.GetValue("email").ToString());
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
    [Route("get_owned_samples")]
    public async Task<IActionResult> GetOwnedSamples(dynamic req) {
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
        queryString = GetInNeighborsQuery("sample", "OWNED_BY", body.GetValue("email").ToString());
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
    [Route("like_vertex")]
    public async Task<IActionResult> LikeVertex(dynamic req) {
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
        queryString = CreateOutNeighborQuery("LIKES", body.GetValue("email").ToString(), body.GetValue("vertexId").ToString());
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
    [Route("unlike_vertex")]
    public async Task<IActionResult> UnlikeVertex(dynamic req) {
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
        queryString = DeleteOutNeighborQuery("LIKES", body.GetValue("email").ToString(), body.GetValue("vertexId").ToString());
      } catch (Exception e) {
        return BadRequest("Malformed request: {e}");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch (Exception e) {
        return BadRequest("Something went wrong: {e}");
      }
    }

    [HttpPost]
    [Route("get_vertex_owner")]
    public async Task<IActionResult> GetVertexOwner(dynamic req) {
      JObject body = DeserializeRequest(req);

      string queryString;

      try {
        queryString = GetOutNeighborsQuery("user", "OWNED_BY", body.GetValue("vertexId").ToString());
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