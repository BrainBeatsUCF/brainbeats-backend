using brainbeats_backend.Controllers;
using Gremlin.Net.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static brainbeats_backend.QueryBuilder;
using static brainbeats_backend.QueryStrings;
using static brainbeats_backend.Utility;

namespace brainbeats_backend.GremlinQueries {
  public static class BeatQueries {
    public static async Task<ResultSet<dynamic>> CreateBeatVertexQuery(string email, BeatVertex b, string seed) {
      string beatId = Guid.NewGuid().ToString();
      StringBuilder queryString = new StringBuilder(CreateVertex("beat", beatId));

      foreach (PropertyInfo prop in b.GetType().GetProperties()) {
        // Skip null or empty fields
        if (prop.GetValue(b) == null || string.IsNullOrWhiteSpace(prop.GetValue(b).ToString())) {
          throw new ArgumentException($"{prop.Name} must not be empty");
        }

        queryString.Append(await SetField(prop, "beat", beatId, b).ConfigureAwait(false));
      }

      if (!string.IsNullOrWhiteSpace(seed)) {
        queryString.Append(AddProperty("seed", seed));
      }

      queryString.Append(CreateEdge("OWNED_BY", email) + EdgeSourceReference());

      return await DatabaseConnection.Instance.ExecuteQuery(queryString.ToString());
    }

    public static async Task<ResultSet<dynamic>> UpdateBeatVertexQuery(string beatId, BeatVertex b) {
      StringBuilder queryString = new StringBuilder(GetVertex(beatId));

      foreach (PropertyInfo prop in b.GetType().GetProperties()) {
        // Skip null or empty fields
        if (prop.GetValue(b) == null || string.IsNullOrWhiteSpace(prop.GetValue(b).ToString())) {
          continue;
        }

        queryString.Append(await SetField(prop, "beat", beatId, b).ConfigureAwait(false));
      }

      return await DatabaseConnection.Instance.ExecuteQuery(queryString.ToString());
    }

    public static async Task<List<dynamic>> SearchBeatVertexQuery(string name, string email) {
      // If name and email is null, get public Beats
      if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(email)) {
        string queryString = GetAllPublicVerticesQuery("beat");

        ResultSet<dynamic> result = await DatabaseConnection.Instance.ExecuteQuery(queryString);

        List<dynamic> resultList = await PopulatePlaylistLength(result);
        resultList = await PopulateVertexOwners(resultList);

        return resultList;
      }
      // If name is not null and email is null, search public Beats
      else if (!string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(email)) {
        string queryString = SearchPublicVertexQuery("beat", name);

        ResultSet<dynamic> result = await DatabaseConnection.Instance.ExecuteQuery(queryString);

        List<dynamic> resultList = await PopulatePlaylistLength(result);
        resultList = await PopulateVertexOwners(resultList);

        return resultList;
      }
      // If name is null and email is not null, get public and owned Beats
      else if (string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(email)) {
        string queryStringPublic = GetAllPublicVerticesQuery("beat");
        string queryStringPrivate = GetAllOwnedVerticesQuery("beat", email);

        ResultSet<dynamic> resultsPublic = await DatabaseConnection.Instance.ExecuteQuery(queryStringPublic);
        ResultSet<dynamic> resultsPrivate = await DatabaseConnection.Instance.ExecuteQuery(queryStringPrivate);

        List<dynamic> resultList = await PopulateVertexOwners(resultsPublic.Concat(resultsPrivate));

        return resultList;
      }
      // If name and email is not null, search public and owned Beats
      else {
        string queryStringPublic = SearchPublicVertexQuery("beat", name);
        string queryStringPrivate = SearchOwnedVertexQuery("beat", email, name);

        ResultSet<dynamic> resultsPublic = await DatabaseConnection.Instance.ExecuteQuery(queryStringPublic);
        ResultSet<dynamic> resultsPrivate = await DatabaseConnection.Instance.ExecuteQuery(queryStringPrivate);

        List<dynamic> resultList = await PopulateVertexOwners(resultsPublic.Concat(resultsPrivate));

        return resultList;
      }
    }

    public static async Task<List<dynamic>> ReadBeatVertexQuery(string beatId) {
      string queryString = GetVertex(beatId);

      ResultSet<dynamic> result = await DatabaseConnection.Instance.ExecuteQuery(queryString);

      List<dynamic> resultList = await PopulatePlaylistLength(result);
      resultList = await PopulateVertexOwners(resultList);

      return resultList;
    }

    public static async Task<ResultSet<dynamic>> DeleteBeatVertexQuery(string beatId) {
      string queryString = GetVertex(beatId) + Delete();

      // Delete the png or jpg picture associated with this Beat
      await StorageConnection.Instance.DeleteFileAsync("beat", $"{beatId}_image.jpg");
      await StorageConnection.Instance.DeleteFileAsync("beat", $"{beatId}_image.png");

      // Delete the wav or mp3 audio associated with this Beat
      await StorageConnection.Instance.DeleteFileAsync("beat", $"{beatId}_audio.wav");
      await StorageConnection.Instance.DeleteFileAsync("beat", $"{beatId}_audio.mp3");

      return await DatabaseConnection.Instance.ExecuteQuery(queryString);
    }
  }
}
