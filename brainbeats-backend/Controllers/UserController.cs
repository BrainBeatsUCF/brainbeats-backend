using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gremlin.Net.Driver;
using Gremlin.Net.Structure.IO.GraphSON;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace brainbeats_backend.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class UserController : ControllerBase
  {
    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateUser(dynamic req) {
      var body = JsonConvert.DeserializeObject<dynamic>(req.ToString());

      string id = Guid.NewGuid().ToString();
      string firstName = body.firstName;
      string lastName = body.lastName;
      string email = body.email;

      if (firstName == null || lastName == null || email == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = $"g.addV('user')" +
        $".property('id', '{id}')" +
        $".property('firstName', '{firstName}')" +
        $".property('lastName', '{lastName}')" +
        $".property('email', '{email}')" +
        $".property('type', 'user')";

      try {
        DatabaseConnection.Instance.ExecuteQuery(queryString).Wait();
        return Ok();
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

      string queryString = $"g.V()" +
        ".hasLabel('user')" +
        $".has('email', '{email}')";

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

      string queryString = $"g.V().hasLabel('user').has('email', '{email}')";
      StringBuilder sb = new StringBuilder(queryString);

      if (firstName != null) {
        sb.Append($".property('firstName', '{firstName}')");
      }

      if (lastName != null) {
        sb.Append($".property('lastName', '{lastName}')");
      }

      Console.WriteLine(sb.ToString());

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

      string queryString = $"g.V().hasLabel('user').has('email', '{email}').drop()";

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }
  }
}