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
      string name = body.name;
      string image = body.image;
      string isPrivate = body.isPrivate;
      string isDeleted = "false";

      Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
      string modifiedDate = unixTimestamp.ToString();

      string queryString1 = "g.addV('playlist')" +
        ".property('type', 'playlist')" +
        $".property('id', '{playlistId}')" +
        $".property('name', '{name}')" +
        $".property('image', '{image}')" +
        $".property('isPrivate', '{isPrivate}')" +
        $".property('isDeleted', '{isDeleted}')" +
        $".property('modifiedDate', '{modifiedDate}')";

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
    [Route("update")]
    public async Task<IActionResult> UpdatePlaylist(dynamic req) {
      var body = JsonConvert.DeserializeObject<dynamic>(req.ToString());

      string playlistId = body.playlistId;
      string name = body.name;
      string image = body.image;
      string isPrivate = body.isPrivate;
      string isDeleted = body.isDeleted;

      Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
      string modifiedDate = unixTimestamp.ToString();

      if (playlistId == null || (name == null && image == null && isPrivate == null && isDeleted == null)) {
        return BadRequest("Malformed Request");
      }

      string queryString = $"g.V('{playlistId}')";
      StringBuilder sb = new StringBuilder(queryString);

      if (name != null) {
        sb.Append($".property('name', '{name}')");
      }

      if (image != null) {
        sb.Append($".property('image', '{image}')");
      }

      if (isPrivate != null) {
        sb.Append($".property('isPrivate', '{isPrivate}')");
      }

      if (isDeleted != null) {
        sb.Append($".property('isDeleted', '{isDeleted}')");
      }

      sb.Append($".property('modifiedDate', '{modifiedDate}')");

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(sb.ToString());
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

      Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
      string modifiedDate = unixTimestamp.ToString();

      string queryString = $"g.V('{playlistId}')" +
        $".property('modifiedDate', '{modifiedDate}')" +
        ".outE('CONTAINS')" +
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

      Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
      string modifiedDate = unixTimestamp.ToString();

      string queryString = $"g.V('{playlistId}')" +
        $".property('modifiedDate', '{modifiedDate}')" +
        ".addE('CONTAINS')" +
        $".to(g.V('{beatId}'))";

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