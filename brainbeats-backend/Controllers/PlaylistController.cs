using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static brainbeats_backend.Utility;

namespace brainbeats_backend.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class PlaylistController : ControllerBase
  {
    [HttpPost]
    [Route("create_playlist")]
    public async Task<IActionResult> CreatePlaylist(dynamic req) {
      string request = GetRequest(req);
      var body = JsonConvert.DeserializeObject<dynamic>(request);

      string playlistId = Guid.NewGuid().ToString();
      string email = body.email;
      string beatId = body.beatId;

      string name = body.name;
      string image = body.image;
      string isPrivate = body.isPrivate;
      string createdDate = GetCurrentTime();
      string modifiedDate = GetCurrentTime();

      StringBuilder queryString = new StringBuilder();

      try {
        queryString.Append(CreateVertex("playlist", playlistId) +
          AddProperty("name", name) +
          AddProperty("image", image) +
          AddProperty("isPrivate", isPrivate) +
          AddProperty("createdDate", createdDate) +
          AddProperty("modifiedDate", modifiedDate) +
          CreateEdge("OWNED_BY", email));

        if (beatId != null) {
          queryString.Append(CreateEdge("CONTAINS", beatId));
        }
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
    [Route("read_playlist")]
    public async Task<IActionResult> ReadPlaylist(dynamic req) {
      string request = GetRequest(req);
      var body = JsonConvert.DeserializeObject<dynamic>(request);

      string playlistId = body.playlistId;

      if (playlistId == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = GetVertex(playlistId);

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("read_playlist_beats")]
    public async Task<IActionResult> ReadPlaylistBeats(dynamic req) {
      string request = GetRequest(req);
      var body = JsonConvert.DeserializeObject<dynamic>(request);

      string playlistId = body.playlistId;

      if (playlistId == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = GetVertex(playlistId) + GetNeighbors("CONTAINS");

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("read_playlist_owner")]
    public async Task<IActionResult> ReadPlaylistOwner(dynamic req) {
      string request = GetRequest(req);
      var body = JsonConvert.DeserializeObject<dynamic>(request);

      string playlistId = body.playlistId;

      if (playlistId == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = GetVertex(playlistId) + GetNeighbors("OWNED_BY");

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("update_playlist")]
    public async Task<IActionResult> UpdatePlaylist(dynamic req) {
      string request = GetRequest(req);
      var body = JsonConvert.DeserializeObject<dynamic>(request);

      string playlistId = body.playlistId;

      string name = body.name;
      string image = body.image;
      string isPrivate = body.isPrivate;
      string modifiedDate = GetCurrentTime();

      StringBuilder queryString = new StringBuilder();
      bool required = false;

      try {
        queryString.Append(GetVertex(playlistId) +
          AddProperty("name", name, required) +
          AddProperty("image", image, required) +
          AddProperty("isPrivate", isPrivate, required) +
          AddProperty("modifiedDate", modifiedDate));
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
    [Route("update_playlist_delete_beat")]
    public async Task<IActionResult> UpdatePlaylistDeleteBeat(dynamic req) {
      string request = GetRequest(req);
      var body = JsonConvert.DeserializeObject<dynamic>(request);

      string playlistId = body.playlistId;
      string beatId = body.beatId;
      string modifiedDate = GetCurrentTime();

      if (playlistId == null || beatId == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = GetVertex(playlistId) +
        AddProperty("modifiedDate", modifiedDate) +
        DeleteEdge("CONTAINS", beatId);

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("update_playlist_add_beat")]
    public async Task<IActionResult> UpdatePlaylistAddBeat(dynamic req) {
      string request = GetRequest(req);
      var body = JsonConvert.DeserializeObject<dynamic>(request);

      string playlistId = body.playlistId;
      string beatId = body.beatId;
      string modifiedDate = GetCurrentTime();

      if (playlistId == null || beatId == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = GetVertex(playlistId) +
        AddProperty("modifiedDate", modifiedDate) +
        CreateEdge("CONTAINS", beatId);

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("delete_playlist")]
    public async Task<IActionResult> DeletePlaylist(dynamic req) {
      string request = GetRequest(req);
      var body = JsonConvert.DeserializeObject<dynamic>(request);

      string playlistId = body.playlistId;

      if (playlistId == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = GetVertex(playlistId) + Delete();

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }
  }
}