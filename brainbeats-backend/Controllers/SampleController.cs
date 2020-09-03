using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace brainbeats_backend.Controllers {
  /*
   * Sample Schema:
   * id - string
   * name - string
   * isPrivate - boolean
   * isDeleted - boolean
   * sampleNote - ?
   * type - ?
   */

  [Route("api/[controller]")]
  [ApiController]
  public class SampleController : ControllerBase {
    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateSample(dynamic req) {
      string request;
      if (req.GetType().Equals(typeof(string))) {
        request = req;
      } else {
        request = req.ToString();
      }

      var body = JsonConvert.DeserializeObject<dynamic>(request);

      string sampleId = body.sampleId;
      string name = body.name;

      if (sampleId == null || name == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = $"g.addV('sample')" +
        $".property('id', '{sampleId}')" +
        $".property('name', '{name}')";

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("read")]
    public async Task<IActionResult> ReadSample(dynamic req) {
      string request;
      if (req.GetType().Equals(typeof(string))) {
        request = req;
      } else {
        request = req.ToString();
      }

      var body = JsonConvert.DeserializeObject<dynamic>(request);

      string sampleId = body.sampleId;

      if (sampleId == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = $"g.V('{sampleId}')";

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("update")]
    public async Task<IActionResult> UpdateSample(dynamic req) {
      string request;
      if (req.GetType().Equals(typeof(string))) {
        request = req;
      } else {
        request = req.ToString();
      }

      var body = JsonConvert.DeserializeObject<dynamic>(request);

      string sampleId = body.sampleId;
      string name = body.name;

      if (sampleId == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = $"g.V('{sampleId}')";
      StringBuilder sb = new StringBuilder(queryString);

      if (name != null) {
        sb.Append($".property('name', '{name}')");
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
    public async Task<IActionResult> DeleteSample(dynamic req) {
      string request;
      if (req.GetType().Equals(typeof(string))) {
        request = req;
      } else {
        request = req.ToString();
      }

      var body = JsonConvert.DeserializeObject<dynamic>(request);

      string sampleId = body.sampleId;

      if (sampleId == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = $"g.V('{sampleId}').drop()";

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }
  }
}
