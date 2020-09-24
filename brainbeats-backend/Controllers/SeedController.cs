using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static brainbeats_backend.Utility;

namespace brainbeats_backend.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class SeedController : ControllerBase
  {
    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateSeed(dynamic req) {
      JObject body = DeserializeRequest(req);

      if (!body.ContainsKey("seed")) {
        return BadRequest("Malformed Request");
      }

      string seed = body.GetValue("seed").ToString();

      // Delete the current seed if it exists
      JObject deleteSeedObject =
        new JObject(
          new JProperty("seed", seed));

      try {
        await DeleteSeed(deleteSeedObject.ToString()).ConfigureAwait(false);

        System.Threading.Thread.Sleep(5000);
      } catch {
        return BadRequest("Error deleting prior seed");
      }

      try {
        for (int user = 0; user < 1; user++) {
          JObject userObject =
            new JObject(
              new JProperty("firstName", $"test_first_name_{user}_{seed}"),
              new JProperty("lastName", $"test_last_name_{user}_{seed}"),
              new JProperty("email", $"test_email_{user}_{seed}@email.com"),
              new JProperty("seed", seed));

          await new UserController().CreateUser(userObject.ToString());

          List<JObject> beatList = new List<JObject>();
          List<JObject> sampleList = new List<JObject>();

          for (int beat = 0; beat < 5; beat++) {
            // User 1 owns this beat
            JObject beatObject =
              new JObject(
                new JProperty("name", $"test_beat_{user}_{beat}"),
                new JProperty("email", $"test_email_{user}_{seed}@email.com"),
                new JProperty("isPrivate", "false"),
                new JProperty("attributes", "{}"),
                new JProperty("audio", $"test_beat_audio_{user}_{beat}"),
                new JProperty("instrumentList", $"test_beat_instrumentList_{user}_{beat}"),
                new JProperty("duration", $"test_beat_duration_{user}_{beat}"),
                new JProperty("image", $"test_beat_image_{user}_{beat}"),
                new JProperty("seed", seed));

            beatList.Add(beatObject);
          }

          for (int sample = 0; sample < 2; sample++) {
            JObject sampleObject =
              new JObject(
                new JProperty("name", $"test_sample_{user}_{sample}"),
                new JProperty("email", $"test_email_{user}_{seed}@email.com"),
                new JProperty("isPrivate", "false"),
                new JProperty("attributes", "{}"),
                new JProperty("audio", $"test_beat_audio_{user}_{sample}"),
                new JProperty("seed", seed));

            sampleList.Add(sampleObject);
          }

          List<string> beatIds = new List<string>();
          List<string> sampleIds = new List<string>();

          foreach (JObject obj in beatList) {
            IActionResult resSet = await new BeatController().CreateBeat(obj.ToString());
            OkObjectResult okResult = resSet as OkObjectResult;

            IEnumerable<dynamic> resEnum = okResult.Value as IEnumerable<dynamic>;
            string beatId = resEnum.First()["id"];

            beatIds.Add(beatId);

            System.Threading.Thread.Sleep(1000);
          }

          foreach (JObject obj in sampleList) {
            IActionResult resSet = await new SampleController().CreateSample(obj.ToString());
            OkObjectResult okResult = resSet as OkObjectResult;

            IEnumerable<dynamic> resEnum = okResult.Value as IEnumerable<dynamic>;
            string sampleId = resEnum.First()["id"];

            sampleIds.Add(sampleId);

            System.Threading.Thread.Sleep(1000);
          }

          Console.WriteLine(sampleIds.ToString());

          string playlistEvensId = "";
          string playlistOddsId = "";

          for (int i = 0; i < beatIds.Count(); i++) {
            // Even numbers belong in playlist 1, odd numbers belong in playlist 2
            if (i % 2 == 0) {
              if (i == 0) {
                JObject playlistObject =
                  new JObject(
                    new JProperty("name", $"test_playlist_name_{user}_{i}"),
                    new JProperty("email", $"test_email_{user}_{seed}@email.com"),
                    new JProperty("isPrivate", "false"),
                    new JProperty("image", $"test_playlist_image_{user}_{i}"),
                    new JProperty("beatId", beatIds[i]),
                    new JProperty("seed", seed));

                IActionResult resSet = await new PlaylistController().CreatePlaylist(playlistObject.ToString());
                OkObjectResult okResult = resSet as OkObjectResult;

                IEnumerable<dynamic> resEnum = okResult.Value as IEnumerable<dynamic>;
                playlistEvensId = resEnum.First()["id"];
              } else {
                JObject addBeatToPlaylistObject =
                  new JObject(
                    new JProperty("beatId", beatIds[i]),
                    new JProperty("playlistId", playlistEvensId));
                await new PlaylistController().UpdatePlaylistAddBeat(addBeatToPlaylistObject.ToString());
              }
            } else {
              if (i == 1) {
                JObject playlistObject =
                  new JObject(
                    new JProperty("name", $"test_playlist_name_{user}_{i}"),
                    new JProperty("email", $"test_email_{user}_{seed}@email.com"),
                    new JProperty("isPrivate", "false"),
                    new JProperty("image", $"test_playlist_image_{user}_{i}"),
                    new JProperty("beatId", beatIds[i]),
                    new JProperty("seed", seed));

                IActionResult resSet = await new PlaylistController().CreatePlaylist(playlistObject.ToString());
                OkObjectResult okResult = resSet as OkObjectResult;

                IEnumerable<dynamic> resEnum = okResult.Value as IEnumerable<dynamic>;
                playlistOddsId = resEnum.First()["id"];
              } else {
                JObject addBeatToPlaylistObject =
                  new JObject(
                    new JProperty("beatId", beatIds[i]),
                    new JProperty("playlistId", playlistOddsId));
                await new PlaylistController().UpdatePlaylistAddBeat(addBeatToPlaylistObject.ToString());
              }
            }

            System.Threading.Thread.Sleep(1000);

            JObject likeBeatObject =
              new JObject(
                new JProperty("vertexId", beatIds[i]),
                new JProperty("email", $"test_email_{user}_{seed}@email.com"));

            await new UserController().LikeVertex(likeBeatObject.ToString());

            System.Threading.Thread.Sleep(1000);
          }
        }

        return Ok();
      } catch {
        return BadRequest("Error");
      }
    }

    [HttpPost]
    [Route("delete")]
    public async Task<IActionResult> DeleteSeed(dynamic req) {
      string request = GetRequest(req);
      var body = JsonConvert.DeserializeObject<dynamic>(request);

      string seed = body.seed;

      if (seed == null) {
        return BadRequest("Malformed Request");
      }

      string queryString = $"g.V().has('seed', '{seed}').drop()";

      try {
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
        return Ok(result);
      } catch {
        return BadRequest();
      }
    }
  }
}