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
  public class TestController : ControllerBase
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
      } catch {
        return BadRequest("Error deleting prior seed");
      }

      // User 1
      JObject userObject1 =
        new JObject(
          new JProperty("firstName", $"test_first_name_{seed}"),
          new JProperty("lastName", $"test_last_name_{seed}"),
          new JProperty("email", $"test_email_1_{seed}@email.com"),
          new JProperty("seed", seed));

      // User 1 owns this sample
      JObject sampleObject1a =
        new JObject(
          new JProperty("name", "test_sample_name_1"),
          new JProperty("email", $"test_email_1_{seed}@email.com"),
          new JProperty("isPrivate", "false"),
          new JProperty("attributes", "test_sample_attributes_1"),
          new JProperty("audio", "test_sample_audio_1"),
          new JProperty("seed", seed));

      // User 1 owns this sample
      JObject sampleObject1b =
        new JObject(
          new JProperty("name", "test_sample_name_2"),
          new JProperty("email", $"test_email_1_{seed}@email.com"),
          new JProperty("isPrivate", "false"),
          new JProperty("attributes", "test_sample_attributes_2"),
          new JProperty("audio", "test_sample_audio_2"),
          new JProperty("seed", seed));

      // User 1 owns this beat
      JObject beatObject1a =
        new JObject(
          new JProperty("name", "test_beat_name_1"),
          new JProperty("email", $"test_email_1_{seed}@email.com"),
          new JProperty("isPrivate", "true"),
          new JProperty("duration", "test_beat_duration_1"),
          new JProperty("image", "test_beat_duration_1"),
          new JProperty("instrumentList", "test_beat_instrument_list_1"),
          new JProperty("attributes", "test_beat_attributes_1"),
          new JProperty("audio", "test_beat_audio_1"),
          new JProperty("seed", seed));

      // User 1 owns this beat
      JObject beatObject1b =
        new JObject(
          new JProperty("name", "test_beat_name_2"),
          new JProperty("email", $"test_email_1_{seed}@email.com"),
          new JProperty("isPrivate", "false"),
          new JProperty("duration", "test_beat_duration_2"),
          new JProperty("image", "test_beat_duration_2"),
          new JProperty("instrumentList", "test_beat_instrument_list_2"),
          new JProperty("attributes", "test_beat_attributes_2"),
          new JProperty("audio", "test_beat_audio_2"),
          new JProperty("seed", seed));

      string beatId1a;

      try {
        await new UserController().CreateUser(userObject1.ToString());
        await new SampleController().CreateSample(sampleObject1a.ToString());
        await new SampleController().CreateSample(sampleObject1b.ToString());

        IActionResult resSet = await new BeatController().CreateBeat(beatObject1a.ToString());
        OkObjectResult okResult = resSet as OkObjectResult;

        IEnumerable<dynamic> resEnum = okResult.Value as IEnumerable<dynamic>;
        beatId1a = resEnum.First()["id"];

        await new BeatController().CreateBeat(beatObject1b.ToString());
      } catch {
        return BadRequest("Error creating base vertices and edges");
      }

      // User 1 likes this beat
      JObject likeBeatObject1a =
        new JObject(
          new JProperty("vertexId", beatId1a),
          new JProperty("email", $"test_email_1_{seed}@email.com"));

      // User 1 owns this playlist consisting of the prior created beat
      JObject playlistObject1a =
        new JObject(
          new JProperty("name", "test_playlist_name_1"),
          new JProperty("email", $"test_email_1_{seed}@email.com"),
          new JProperty("isPrivate", "false"),
          new JProperty("image", "test_playlist_image_1"),
          new JProperty("beatId", beatId1a),
          new JProperty("seed", seed));

      try {
        await new UserController().LikeVertex(likeBeatObject1a.ToString());
        await new PlaylistController().CreatePlaylist(playlistObject1a.ToString());

        return Ok();
      } catch {
        return BadRequest("Error creating extra vertices and edges");
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