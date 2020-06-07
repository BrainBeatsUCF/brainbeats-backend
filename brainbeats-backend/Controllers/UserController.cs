using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace brainbeats_backend.Controllers {
  /*
   * User Schema:
   * id - string
   * firstName - string
   * lastName - string
   */

  [Route("api/[controller]")]
  [ApiController]
  public class UserController : ControllerBase
  {
    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateUser(dynamic req) {
      string request;
      if (req.GetType().Equals(typeof(string))) {
        request = req;
      } else {
        request = req.ToString();
      }

      var body = JsonConvert.DeserializeObject<dynamic>(request);

      string firstName = body.firstName;
      string lastName = body.lastName;
      string email = body.email;

      if (firstName == null || lastName == null || email == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = $"g.addV('user')" +
        $".property('id', '{email}')" +
        $".property('firstName', '{firstName}')" +
        $".property('lastName', '{lastName}')" +
        $".property('type', 'user')";

      if (body.seed != null) {
        queryString += $".property('seed', '{body.seed}')";
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("read")]
    public async Task<IActionResult> ReadUser(dynamic req) {
      var body = JsonConvert.DeserializeObject<dynamic>(req.ToString());

      string email = body.email;

      if (email == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = $"g.V('{email}')";

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("update")]
    public async Task<IActionResult> UpdateUser(dynamic req) {
      var body = JsonConvert.DeserializeObject<dynamic>(req.ToString());

      string firstName = body.firstName;
      string lastName = body.lastName;
      string email = body.email;

      if ((firstName == null && lastName == null) || email == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = $"g.V('{email}')";
      StringBuilder sb = new StringBuilder(queryString);

      if (firstName != null) {
        sb.Append($".property('firstName', '{firstName}')");
      }

      if (lastName != null) {
        sb.Append($".property('lastName', '{lastName}')");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(sb.ToString());
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("delete")]
    public async Task<IActionResult> DeleteUser(dynamic req) {
      var body = JsonConvert.DeserializeObject<dynamic>(req.ToString());
      string email = body.email;

      if (email == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = $"g.V('{email}').drop()";

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }
  }
}