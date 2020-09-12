using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static brainbeats_backend.QueryBuilder;

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
      string seed = body.seed;

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
          queryString.Append(EdgeSourceReference() + CreateEdge("CONTAINS", beatId));
        }
      } catch {
        return BadRequest("Malformed Request");
      }

      if (seed != null) {
        queryString.Append(EdgeSourceReference() + AddProperty("seed", seed, false));
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

      string queryString = GetVertex(playlistId) + GetOutNeighbors("CONTAINS");

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("get_all_playlists")]
    public async Task<IActionResult> GetAllPlaylists(dynamic req) {
      string request = GetRequest(req);
      var body = JsonConvert.DeserializeObject<dynamic>(request);

      string email = body.email;

      if (email == null) {
        return BadRequest("Malformed Request");
      }

      string queryStringPublic = GetAllVertices("playlist") + HasProperty("isPrivate", "false");
      string queryStringPrivate = GetVertex(email) + GetInNeighbors("OWNED_BY") + EdgeSourceReference() + HasLabel("playlist") + HasProperty("isPrivate", "true");

      try {
        var resultsPublic = await DatabaseConnection.Instance.ExecuteQuery(queryStringPublic);
        var resultsPrivate = await DatabaseConnection.Instance.ExecuteQuery(queryStringPrivate);

        List<dynamic> resultList = new List<dynamic>();

        foreach (var item in resultsPublic) {
          resultList.Add(item);
        }

        foreach (var item in resultsPrivate) {
          resultList.Add(item);
        }

        return Ok(resultList);
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
        GetEdge("CONTAINS", beatId) +
        Delete();

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