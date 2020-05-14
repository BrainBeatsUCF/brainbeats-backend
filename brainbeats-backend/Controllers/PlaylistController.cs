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
      string userEmail = body.userEmail;
      string beatId = body.beatId;

      string queryString1 = $"g.addV('playlist')" +
        $".property('type', 'playlist')" +
        $".property('id', '{playlistId}')";

      StringBuilder sb1 = new StringBuilder(queryString1);

      if (beatId != null) {
        string addEdge = $".addE('contains')" +
          $".to(g.V('{beatId}'))";

        sb1.Append(addEdge);
      }

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(sb1.ToString());

        string queryString2 = $"g.V('{userEmail}')" +
          $".addE('OWNER_OF')" +
          $".to(g.V('{playlistId}'))";

        await DatabaseConnection.Instance.ExecuteQuery(queryString2);

        string queryString3 = $"g.V('{playlistId}')" +
          $".addE('OWNED_BY')" +
          $".to(g.V('{userEmail}'))";

        await DatabaseConnection.Instance.ExecuteQuery(queryString3);

        return Ok(result);
      } catch {
        return BadRequest();
      }
    }
  }
}