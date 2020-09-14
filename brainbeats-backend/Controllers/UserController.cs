using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using static brainbeats_backend.QueryStrings;
using static brainbeats_backend.Utility;

namespace brainbeats_backend.Controllers {
  [Route("api/[controller]")]
  [ApiController]
  public class UserController : ControllerBase {
    [HttpPost]
    [Route("create_user")]
    public async Task<IActionResult> CreateUser(dynamic req) {
      JObject body = DeserializeRequest(req);

      string queryString;

      try {
        queryString = CreateVertexQuery("user", body.GetValue("email").ToString(), body);
      } catch {
        return BadRequest("Malformed request");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest("Something went wrong");
      }
    }

    [HttpPost]
    [Route("read_user")]
    public async Task<IActionResult> ReadUser(dynamic req) {
      JObject body = DeserializeRequest(req);

      string queryString;

      try {
        queryString = ReadVertexQuery(body.GetValue("email").ToString());
      } catch {
        return BadRequest("Malformed request");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest("Something went wrong");
      }
    }

    [HttpPost]
    [Route("update_user")]
    public async Task<IActionResult> UpdateUser(dynamic req) {
      JObject body = DeserializeRequest(req);

      string queryString;

      try {
        queryString = UpdateVertexQuery("user", body.GetValue("email").ToString(), body);
      } catch {
        return BadRequest("Malformed Request");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest("Something went wrong");
      }
    }

    [HttpPost]
    [Route("delete_user")]
    public async Task<IActionResult> DeleteUser(dynamic req) {
      JObject body = DeserializeRequest(req);

      string queryString;

      try {
        queryString = DeleteVertexQuery(body.GetValue("email").ToString());
      } catch {
        return BadRequest("Malformed request");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest("Something went wrong");
      }
    }

    [HttpPost]
    [Route("get_liked_beats")]
    public async Task<IActionResult> GetLikedBeats(dynamic req) {
      JObject body = DeserializeRequest(req);

      string queryString;

      try {
        queryString = GetOutNeighborsQuery("beat", "LIKES", body.GetValue("email").ToString());
      } catch {
        return BadRequest("Malformed Request");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest("Something went wrong");
      }
    }

    [HttpPost]
    [Route("get_liked_playlists")]
    public async Task<IActionResult> GetLikedPlaylists(dynamic req) {
      JObject body = DeserializeRequest(req);

      string queryString;

      try {
        queryString = GetOutNeighborsQuery("playlist", "LIKES", body.GetValue("email").ToString());
      } catch {
        return BadRequest("Malformed Request");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest("Something went wrong");
      }
    }

    [HttpPost]
    [Route("get_liked_samples")]
    public async Task<IActionResult> GetLikedSamples(dynamic req) {
      JObject body = DeserializeRequest(req);

      string queryString;

      try {
        queryString = GetOutNeighborsQuery("sample", "LIKES", body.GetValue("email").ToString());
      } catch {
        return BadRequest("Malformed Request");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest("Something went wrong");
      }
    }

    [HttpPost]
    [Route("get_owned_beats")]
    public async Task<IActionResult> GetOwnedBeats(dynamic req) {
      JObject body = DeserializeRequest(req);

      string queryString;

      try {
        queryString = GetInNeighborsQuery("beat", "OWNED_BY", body.GetValue("email").ToString());
      } catch {
        return BadRequest("Malformed Request");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest("Something went wrong");
      }
    }

    [HttpPost]
    [Route("get_owned_playlists")]
    public async Task<IActionResult> GetOwnedPlaylists(dynamic req) {
      JObject body = DeserializeRequest(req);

      string queryString;

      try {
        queryString = GetInNeighborsQuery("playlist", "OWNED_BY", body.GetValue("email").ToString());
      } catch {
        return BadRequest("Malformed Request");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest("Something went wrong");
      }
    }

    [HttpPost]
    [Route("get_owned_samples")]
    public async Task<IActionResult> GetOwnedSamples(dynamic req) {
      JObject body = DeserializeRequest(req);

      string queryString;

      try {
        queryString = GetInNeighborsQuery("sample", "OWNED_BY", body.GetValue("email").ToString());
      } catch {
        return BadRequest("Malformed Request");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest("Something went wrong");
      }
    }

    [HttpPost]
    [Route("like_vertex")]
    public async Task<IActionResult> LikeVertex(dynamic req) {
      JObject body = DeserializeRequest(req);

      string queryString;

      try {
        queryString = CreateOutNeighborQuery("LIKES", body.GetValue("email").ToString(), body.GetValue("vertexId").ToString());
      } catch {
        return BadRequest("Malformed request");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest("Something went wrong");
      }
    }

    [HttpPost]
    [Route("unlike_vertex")]
    public async Task<IActionResult> UnlikeVertex(dynamic req) {
      JObject body = DeserializeRequest(req);

      string queryString;

      try {
        queryString = DeleteOutNeighborQuery("LIKES", body.GetValue("email").ToString(), body.GetValue("vertexId").ToString());
      } catch {
        return BadRequest("Malformed request");
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest("Something went wrong");
      }
    }
  }
}