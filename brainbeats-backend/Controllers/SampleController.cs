using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static brainbeats_backend.Utility;

namespace brainbeats_backend.Controllers {
  [Route("api/[controller]")]
  [ApiController]
  public class SampleController : ControllerBase {
    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateSample(dynamic req) {
      string request = GetRequest(req);
      var body = JsonConvert.DeserializeObject<dynamic>(request);

      string sampleId = body.sampleId;
      string name = body.name;
      string email = body.email;

      string isPrivate = body.isPrivate;
      string attributes = body.attributes;
      string audio = body.audio;
      string type = body.type;
      string modifiedDate = GetCurrentTime();

      StringBuilder queryString = new StringBuilder();
      try {
        queryString.Append(CreateVertex("sample", sampleId) +
          AddProperty("name", name) +
          AddProperty("isPrivate", isPrivate) +
          AddProperty("attributes", attributes) +
          AddProperty("audio", audio) +
          AddProperty("type", type) +
          AddProperty("modifiedDate", modifiedDate) +
          CreateEdge("OWNED_BY", email));
      } catch {
        return BadRequest();
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
    [Route("read")]
    public async Task<IActionResult> ReadSample(dynamic req) {
      string request = GetRequest(req);
      var body = JsonConvert.DeserializeObject<dynamic>(request);

      string sampleId = body.sampleId;

      if (sampleId == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = GetVertex(sampleId);

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
      string request = GetRequest(req);
      var body = JsonConvert.DeserializeObject<dynamic>(request);

      string sampleId = body.sampleId;
      string name = body.name;

      if (sampleId == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = GetVertex(sampleId) +
        AddProperty("name", name);

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("delete")]
    public async Task<IActionResult> DeleteSample(dynamic req) {
      string request = GetRequest(req);
      var body = JsonConvert.DeserializeObject<dynamic>(request);

      string sampleId = body.sampleId;

      if (sampleId == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = GetVertex(sampleId) + Delete();

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }
  }
}
