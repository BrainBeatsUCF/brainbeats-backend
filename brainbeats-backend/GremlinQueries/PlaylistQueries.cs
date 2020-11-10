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
  public static class PlaylistQueries {
    public static async Task<ResultSet<dynamic>> CreatePlaylistVertexQuery(string email, PlaylistVertex p, string seed) {
      string playlistId = Guid.NewGuid().ToString();
      email = email.ToLowerInvariant();

      StringBuilder queryString = new StringBuilder(CreateVertex("playlist", playlistId));

      foreach (PropertyInfo prop in p.GetType().GetProperties()) {
        // Skip null or empty fields
        if (prop.GetValue(p) == null || string.IsNullOrWhiteSpace(prop.GetValue(p).ToString())) {
          throw new ArgumentException($"{prop.Name} must not be empty");
        }

        queryString.Append(await SetField(prop, "playlist", playlistId, p).ConfigureAwait(false));
      }

      if (!string.IsNullOrWhiteSpace(seed)) {
        queryString.Append(AddProperty("seed", seed));
      }

      queryString.Append(CreateEdge("OWNED_BY", email) + EdgeSourceReference());

      return await DatabaseConnection.Instance.ExecuteQuery(queryString.ToString());
    }

    public static async Task<ResultSet<dynamic>> UpdatePlaylistVertexQuery(string playlistId, PlaylistVertex p) {
      StringBuilder queryString = new StringBuilder(GetVertex(playlistId));

      foreach (PropertyInfo prop in p.GetType().GetProperties()) {
        // Skip null or empty fields
        if (prop.GetValue(p) == null || string.IsNullOrWhiteSpace(prop.GetValue(p).ToString())) {
          continue;
        }

        queryString.Append(await SetField(prop, "playlist", playlistId, p).ConfigureAwait(false));
      }

      return await DatabaseConnection.Instance.ExecuteQuery(queryString.ToString());
    }

    public static async Task<List<dynamic>> SearchPlaylistVertexQuery(string name, string email) {
      // If name and email is null, get public Beats
      if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(email)) {
        string queryString = GetAllPublicVerticesQuery("playlist");
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);

        List<dynamic> resultList = await PopulatePlaylistLength(result);
        resultList = await PopulateVertexOwners(resultList);

        return resultList;
      }
      // If name is not null and email is null, search public Beats
      else if (!string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(email)) {
        string queryString = SearchPublicVertexQuery("playlist", name);
        var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);

        List<dynamic> resultList = await PopulatePlaylistLength(result);
        resultList = await PopulateVertexOwners(resultList);

        return resultList;
      }
      // If name is null and email is not null, get public and owned Beats
      else if (string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(email)) {
        string queryStringPublic = GetAllPublicVerticesQuery("playlist");
        string queryStringPrivate = GetAllOwnedVerticesQuery("playlist", email);

        var resultsPublic = await DatabaseConnection.Instance.ExecuteQuery(queryStringPublic);
        var resultsPrivate = await DatabaseConnection.Instance.ExecuteQuery(queryStringPrivate);
        var result = resultsPublic.Concat(resultsPrivate);

        List<dynamic> resultList = await PopulatePlaylistLength(result);
        resultList = await PopulateVertexOwners(resultList);

        return resultList;
      }
      // If name and email is not null, search public and owned Beats
      else {
        string queryStringPublic = SearchPublicVertexQuery("playlist", name);
        string queryStringPrivate = SearchOwnedVertexQuery("playlist", email, name);

        var resultsPublic = await DatabaseConnection.Instance.ExecuteQuery(queryStringPublic);
        var resultsPrivate = await DatabaseConnection.Instance.ExecuteQuery(queryStringPrivate);
        var result = resultsPublic.Concat(resultsPrivate);

        List<dynamic> resultList = await PopulatePlaylistLength(result);
        resultList = await PopulateVertexOwners(resultList);

        return resultList;
      }
    }

    public static async Task<ResultSet<dynamic>> UpdatePlaylistAddBeat(string playlistId, string beatId) {
      string queryString = CreateOutNeighborQuery("CONTAINS", playlistId, beatId);

      return await DatabaseConnection.Instance.ExecuteQuery(queryString);
    }

    public static async Task<ResultSet<dynamic>> UpdatePlaylistRemoveBeat(string playlistId, string beatId) {
      string queryString = DeleteOutNeighborQuery("CONTAINS", playlistId, beatId);

      return await DatabaseConnection.Instance.ExecuteQuery(queryString);
    }

    public static async Task<List<dynamic>> ReadPlaylistVertexQuery(string playlistId) {
      string queryString = GetVertex(playlistId);

      var result = await DatabaseConnection.Instance.ExecuteQuery(queryString);
      List<dynamic> resultList = await PopulatePlaylistLength(result);
      resultList = await PopulateVertexOwners(resultList);

      return resultList;
    }

    public static async Task<ResultSet<dynamic>> DeletePlaylistVertexQuery(string playlistId) {
      string queryString = GetVertex(playlistId) + Delete();

      // Delete the png or jpg picture associated with this Playlist
      await StorageConnection.Instance.DeleteFileAsync("playlist", $"{playlistId}_image.jpg");
      await StorageConnection.Instance.DeleteFileAsync("playlist", $"{playlistId}_image.png");

      return await DatabaseConnection.Instance.ExecuteQuery(queryString);
    }
  }
}
