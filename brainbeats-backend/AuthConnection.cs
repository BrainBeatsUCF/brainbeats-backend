using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace brainbeats_backend {
  public class AuthConnection {
    public static AuthConnection Instance { get; set; }
    private HttpClient httpClient { get; set; }
    private GraphServiceClient graphClient { get; set; }
    private string appId { get; set; }
    private string tenantId { get; set; }
    private string metadataAddress { get; set; }
    private string tokenEndpoint { get; set; }

    public static void Init(IConfiguration configuration) {
      Instance = new AuthConnection(configuration);
    }

    private AuthConnection(IConfiguration configuration) {
      appId = configuration["Auth:AppId"];
      tenantId = configuration["Auth:TenantId"];
      metadataAddress = configuration["Auth:MetadataAddress"];
      tokenEndpoint = configuration["Auth:TokenEndpoint"];
      string clientSecret = configuration["Auth:ClientSecret"];

      // Initialize new Microsoft Graph client
      IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
          .Create(appId)
          .WithTenantId(tenantId)
          .WithClientSecret(clientSecret)
          .Build();
      ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);

      graphClient = new GraphServiceClient(authProvider);

      httpClient = new HttpClient();
    }

    public JwtSecurityToken ValidateToken(string token) {
      if (token == null || !token.StartsWith("Bearer ")) {
        throw new ArgumentException();
      }

      token = token.Split(" ")[1];

      ConfigurationManager<OpenIdConnectConfiguration> configManager = new ConfigurationManager<OpenIdConnectConfiguration>(metadataAddress, new OpenIdConnectConfigurationRetriever());
      OpenIdConnectConfiguration config = configManager.GetConfigurationAsync().Result;

      TokenValidationParameters validationParameters = new TokenValidationParameters {
        ValidAudience = appId, // Audience for B2C is the app's AppId
        ValidIssuer = $"https://ucfbrainbeats.b2clogin.com/{tenantId}/v2.0/",
        ValidateAudience = true,
        ValidateIssuer = true,
        ValidateLifetime = true,
        IssuerSigningKeys = config.SigningKeys
      };

      try {
        JwtSecurityTokenHandler tokendHandler = new JwtSecurityTokenHandler();
        SecurityToken jwt;
        tokendHandler.ValidateToken(token, validationParameters, out jwt);

        return jwt as JwtSecurityToken;
      } catch (Exception e) {
        throw;
      }
    }

    public async Task CreateUser(string name, string email, string password) {
      var user = new User {
        AccountEnabled = true,
        DisplayName = name,
        MailNickname = email,
        UserPrincipalName = email,
        PasswordProfile = new PasswordProfile {
          ForceChangePasswordNextSignIn = false,
          ForceChangePasswordNextSignInWithMfa = false,
          Password = password
        }
      };

      await graphClient.Users
        .Request()
        .AddAsync(user);
    }

    public async Task<string> LoginUser(string email, string password) {
      var values = new Dictionary<string, string>
      {
        { "client_id", appId },
        { "grant_type", "password" },
        { "username", email },
        { "password", password },
        { "scope", $"openid {appId} offline_access" },
        { "response_type", "token id_token" }
      };

      var content = new FormUrlEncodedContent(values);
      var response = await this.httpClient.PostAsync(tokenEndpoint, content);
      var responseString = await response.Content.ReadAsStringAsync();

      return responseString;
    }
  }
}
