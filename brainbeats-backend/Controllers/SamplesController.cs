using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace brainbeats_backend.Controllers
{
[Route("api/[controller]")]
[ApiController]
public class SamplesController : ControllerBase
{
    private readonly Database db;

    public SamplesController(IConfiguration configuration)
    {
        db = new DatabaseContext(configuration).db;
    }

    [Route("read")]
    [HttpPost]
    public async Task<IActionResult>
    ReadSample(Sample sample)
    {
        if (sample.id == null)
        {
            return BadRequest("Malformed Request");
        }

        // Verify the sample with this ID exists
        string queryString = "SELECT * FROM s WHERE s.id = @id";
        QueryDefinition queryDefinition =
            new QueryDefinition(queryString).WithParameter("@id", sample.id);

        FeedIterator<Sample> feedIterator =
            db.GetContainer("Samples").GetItemQueryIterator<Sample>(queryDefinition);

        List<Sample> samples = new List<Sample>();

        // Get the matching Sample with our ID
        while (feedIterator.HasMoreResults)
        {
            FeedResponse<Sample> response = await feedIterator.ReadNextAsync();

            foreach (Sample s in response)
            {
                samples.Add(s);
            }
        }

        if (samples.Count == 0)
        {
            return BadRequest("Sample not found");
        }

        return Ok(samples[0]);
    }

    [Route("update")]
    [HttpPost]
    public async Task<IActionResult>
    UpdateSample(Sample sample)
    {
        if (sample.id == null)
        {
            return BadRequest("Malformed Request");
        }

        // Verify the sample with this ID exists
        string queryString = "SELECT * FROM s WHERE s.id = @id";
        QueryDefinition queryDefinition =
            new QueryDefinition(queryString).WithParameter("@id", sample.id);

        FeedIterator<Sample> feedIterator =
            db.GetContainer("Samples").GetItemQueryIterator<Sample>(queryDefinition);

        List<Sample> samples = new List<Sample>();

        // Get the matching Sample with our ID
        while (feedIterator.HasMoreResults)
        {
            FeedResponse<Sample> response = await feedIterator.ReadNextAsync();

            foreach (Sample s in response)
            {
                samples.Add(s);
            }
        }

        if (samples.Count == 0)
        {
            return BadRequest("Sample not found");
        }

        Sample updatedSample = samples[0];

        IList<PropertyInfo> properties = new List<PropertyInfo>(updatedSample.GetType().GetProperties());

        foreach (var field in properties)
        {
            if (field.GetValue(sample, null) != null)
            {
                field.SetValue(updatedSample, field.GetValue(sample, null));
            }
        }

        ItemResponse<Sample> res = await db.GetContainer("Samples").ReplaceItemAsync(updatedSample, updatedSample.id, new PartitionKey(updatedSample.id));

        if (res.StatusCode == HttpStatusCode.OK)
        {
            return Ok();
        }
        else
        {
            return BadRequest("Something went wrong");
        }
    }

    [Route("delete")]
    [HttpPost]
    public async Task<IActionResult>
    DeleteSample(Sample sample)
    {
        if (sample.id == null)
        {
            return BadRequest("Malformed Request");
        }

        // TODO: Give a real status code response for errors
        try
        {
            ItemResponse<Sample> res = await db.GetContainer("Samples").DeleteItemAsync<Sample>(sample.id, new PartitionKey(sample.id));

            return Ok();
        }
        catch (CosmosException e)
        {
            return BadRequest(e);
        }
    }
}
}