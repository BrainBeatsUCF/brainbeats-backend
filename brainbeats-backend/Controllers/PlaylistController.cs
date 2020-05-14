using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace brainbeats_backend.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class PlaylistController : ControllerBase
  {
    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreatePlaylist(dynamic req) {
      var body = JsonConvert.DeserializeObject<dynamic>(req.ToString());

      string playlistId = Guid.NewGuid().ToString();
      string email = body.email;
      string beatId = body.beatId;

      string queryString1 = "g.addV('playlist')" +
        ".property('type', 'playlist')" +
        $".property('id', '{playlistId}')";

      StringBuilder sb1 = new StringBuilder(queryString1);

      // If an initial beatId is specified, add it onto our playlist,
      // otherwise, create an empty playlist
      if (beatId != null) {
        string addEdge = $".addE('CONTAINS')" +
          $".to(g.V('{beatId}'))";

        sb1.Append(addEdge);
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(sb1.ToString());

        // Add edge from user to playlist with type 'OWNER_OF'
        string queryString2 = $"g.V('{email}')" +
          ".addE('OWNER_OF')" +
          $".to(g.V('{playlistId}'))";

        await DatabaseConnection.Instance.ExecuteQuery(queryString2);

        // Add edge from playlist to user with type 'OWNED_BY'
        string queryString3 = $"g.V('{playlistId}')" +
          ".addE('OWNED_BY')" +
          $".to(g.V('{email}'))";

        await DatabaseConnection.Instance.ExecuteQuery(queryString3);

        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("read")]
    public async Task<IActionResult> ReadPlaylist(dynamic req) {
      var body = JsonConvert.DeserializeObject<dynamic>(req.ToString());

      string playlistId = body.playlistId;

      if (playlistId == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = $"g.V('{playlistId}')";

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("readBeats")]
    public async Task<IActionResult> ReadPlaylistBeats(dynamic req) {
      var body = JsonConvert.DeserializeObject<dynamic>(req.ToString());

      string playlistId = body.playlistId;

      if (playlistId == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = $"g.V('{playlistId}').out('CONTAINS')";

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("readOwner")]
    public async Task<IActionResult> ReadPlaylistOwner(dynamic req) {
      var body = JsonConvert.DeserializeObject<dynamic>(req.ToString());

      string playlistId = body.playlistId;

      if (playlistId == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = $"g.V('{playlistId}').out('OWNED_BY')";

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("updateDeleteBeat")]
    public async Task<IActionResult> UpdatePlaylistDeleteBeats(dynamic req) {
      var body = JsonConvert.DeserializeObject<dynamic>(req.ToString());

      string playlistId = body.playlistId;
      string beatId = body.beatId;

      if (playlistId == null || beatId == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = $"g.V('{playlistId}')" +
        ".out('CONTAINS')" +
        $".where(inV().has('id', '{beatId}'))" +
        ".drop()";

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("updateAddBeat")]
    public async Task<IActionResult> UpdatePlaylistAddBeats(dynamic req) {
      var body = JsonConvert.DeserializeObject<dynamic>(req.ToString());

      string playlistId = body.playlistId;
      string beatId = body.beatId;

      if (playlistId == null || beatId == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = $"g.V('{playlistId}')" +
        ".addE('CONTAINS')" +
        $".to(g.V('{beatId}))";

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }

    [HttpPost]
    [Route("delete")]
    public async Task<IActionResult> DeletePlaylist(dynamic req) {
      var body = JsonConvert.DeserializeObject<dynamic>(req.ToString());

      string playlistId = body.playlistId;

      if (playlistId == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = $"g.V('{playlistId}').drop()";

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }
  }
}