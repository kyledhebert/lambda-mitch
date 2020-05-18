using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json.Linq;
using System.IO;
using System;
using Amazon.S3;
using Amazon;
using Amazon.S3.Model;
using System.Text.RegularExpressions;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace LambdaMitch
{
  public class JokeFunction
  {
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
    {
      var accessKey = Environment.GetEnvironmentVariable("ACCESS_KEY");
      var secretKey = Environment.GetEnvironmentVariable("SECRET_KEY");
      var bucketName = Environment.GetEnvironmentVariable("BUCKET_NAME");

      var jokeFileKey = "lambda-mitch/mitch-jokes.json";
      var jokeList = new List<string>();

      using (var client = new AmazonS3Client(accessKey, secretKey, RegionEndpoint.USEast1))
      {
        var getObjectResponse = await client.GetObjectAsync(bucketName, jokeFileKey);

        using (var amazonStream = getObjectResponse.ResponseStream)
        using (var streamReader = new StreamReader(amazonStream))
        using (var jsonReader = new JsonTextReader(streamReader))
        {
          var serializer = new JsonSerializer();
          jokeList = serializer.Deserialize<List<string>>(jsonReader);
        }
      }

      var random = new Random();
      var joke = jokeList.ToArray()[random.Next(0, jokeList.Count)];

      return new APIGatewayProxyResponse
      {
        Body = $@"
        <!DOCTYPE html>
        <html lang='en'>
        <head>
          <meta charset='UTF-8'>
          <meta name='viewport' content='width=device-width, initial-scale=1.0'>
          <title>Mitch Hedberg Jokes</title>
          <style>
            body {{
              background: #FF8C61;
              color: white;
              font-family: 'Verdana', sans-serif;
              text-align: center;
              padding-top: 10%;
              margin: 0px 40px;
            }}

            .site-by {{
              font-size: smaller
            }}

            .jokes-by {{
              font-size: larger
            }}
          </style>
        </head>
        <body>
          <section>
            <h1>{joke}</h1>
            <p class='jokes-by'>Jokes by Mitch Hedberg<p>
            <p class='site-by'>Website by Kyle Hebert<p>
          </section>
        </body>
        </html>",
        StatusCode = 200,
        Headers = new Dictionary<string, string> { { "Content-Type", "text/html; charset=utf-8" } }
      };
    }
  }
}
