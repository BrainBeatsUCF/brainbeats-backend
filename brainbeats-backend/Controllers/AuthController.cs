using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using static brainbeats_backend.Utility;
using static brainbeats_backend.QueryBuilder;

namespace brainbeats_backend.Controllers {
  /// <summary>
  /// AuthController.cs handles all authentication logic.
  /// Includes Azure B2C ROPC Login and Token Refresh code, implemented with
  /// Azure Graph.
  /// </summary>
  [Route("api/v2")]
  [ApiController]
  public class AuthController : ControllerBase {
    /// <summary>Logs in the User with a plaintext email and password.</summary>
    /// <returns>Standard JWT.</returns>
    /// <remarks>Expected request route: GET api/v2/login.</remarks>
    /// <param name="req">JSON body containing the 'email' and 'password' field.</param>
    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login(dynamic req) {
      JObject body = DeserializeRequest(req);

      if (!body.ContainsKey("email") || !body.ContainsKey("password")) {
        return BadRequest("Request body must contain 'email' and 'password'");
      }

      string res;
      try {
        res = await AuthConnection.Instance.LoginUser(body.GetValue("email").ToString().ToLowerInvariant(),
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

      JwtSecurityToken jwt = AuthConnection.DecodeToken(tokenObject.GetValue("access_token").ToString());
      Dictionary<string, string> claimsDictionary = AuthConnection.GetClaimsFromToken(jwt);

      try {
        // See if the user exists in the database
        string queryString = GetVertex(claimsDictionary["emails"]);
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);

        // If the user exists, return Ok()
        if (result.Count > 0) {
          return Ok(res);
        }

        string firstName = claimsDictionary["given_name"];
        string lastName = claimsDictionary["family_name"];
        string email = claimsDictionary["emails"].ToLowerInvariant();

        // Else, create the user
        UserVertex u = new UserVertex(firstName, lastName);

        IActionResult createUserResult = await new UsersController().CreateUser(email, u).ConfigureAwait(false);
        OkObjectResult okResult = createUserResult as OkObjectResult;

        if (okResult.StatusCode != 200) {
          return BadRequest("Error creating new user vertex when signing in user for the first time");
        }

        return Ok(res);

      } catch (Exception e) {
        return BadRequest($"Unknown error signing user for the first time: {e}");
      }
    }

    /// <summary>
    /// Regenerates a JWT access token when supplied with a refresh token generated
    /// during login.
    /// </summary>
    /// <returns>Standard JWT.</returns>
    /// <remarks>Expected request route: GET api/v2/refresh_token.</remarks>
    /// <param name="req">JSON body containing the 'refreshToken' field.</param>
    [HttpPost]
    [Route("refresh_token")]
    public async Task<IActionResult> RefreshToken(dynamic req) {
      JObject body = DeserializeRequest(req);

      if (!body.ContainsKey("refreshToken")) {
        return BadRequest("Request body must contain 'refreshToken'");
      }

      string res;
      try {
        res = await AuthConnection.Instance.RefreshToken(body.GetValue("refreshToken").ToString());
      } catch (Exception e) {
        return BadRequest($"Something went wrong: {e}");
      }

      return Ok(res);
    }
  }
}
