using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static brainbeats_backend.Utility;

namespace brainbeats_backend.Controllers {
  [Route("api/[controller]")]
  [ApiController]
  public class UserController : ControllerBase
  {
    [HttpPost]
    [Route("create_user")]
    public async Task<IActionResult> CreateUser(dynamic req) {
      string request = GetRequest(req);
      var body = JsonConvert.DeserializeObject<dynamic>(request);

      string firstName = body.firstName;
      string lastName = body.lastName;
      string email = body.email;

      StringBuilder queryString = new StringBuilder();

      try {
        queryString.Append(CreateVertex("user", email) +
          AddProperty("firstName", firstName) +
          AddProperty("lastName", lastName));
      } catch {
        return BadRequest("Malformed Request");
      }

      if (body.seed != null) {
        queryString.Append(AddProperty("seed", body.seed, false));
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString.ToString());
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("read_user")]
    public async Task<IActionResult> ReadUser(dynamic req) {
      string request = GetRequest(req);
      var body = JsonConvert.DeserializeObject<dynamic>(request);

      string email = body.email;

      if (email == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = GetVertex(email);

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("update_user")]
    public async Task<IActionResult> UpdateUser(dynamic req) {
      string request = Utility.GetRequest(req);
      var body = JsonConvert.DeserializeObject<dynamic>(request);

      string email = body.email;
      string firstName = body.firstName;
      string lastName = body.lastName;

      StringBuilder queryString = new StringBuilder();
      bool required = false;

      try {
        queryString.Append(GetVertex(email) +
          AddProperty("firstName", firstName, required) +
          AddProperty("lastName", lastName, required));
      } catch {
        return BadRequest("Malformed Request");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString.ToString());
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("delete_user")]
    public async Task<IActionResult> DeleteUser(dynamic req) {
      string request = GetRequest(req);
      var body = JsonConvert.DeserializeObject<dynamic>(request);

      string email = body.email;

      if (email == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = GetVertex(email) + DeleteVertex();

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }
  }
}