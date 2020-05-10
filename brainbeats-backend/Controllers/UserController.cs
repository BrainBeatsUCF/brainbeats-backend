using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace brainbeats_backend.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class UserController : ControllerBase
  {
    private readonly IConfiguration _configuration;

    public UserController(IConfiguration configuration) {
      _configuration = configuration;
    }

    [HttpPost]
    [Route("create")]
    public string CreateUser(dynamic req) {
      var body = JsonConvert.DeserializeObject<dynamic>(req.ToString());

      return new string($"{body}");
    }
  }
}