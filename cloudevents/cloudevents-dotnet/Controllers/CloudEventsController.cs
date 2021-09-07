/*
Copyright 2020 The Knative Authors
Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
    https://www.apache.org/licenses/LICENSE-2.0
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using CloudEventsSample.Models;
using Microsoft.AspNetCore.Mvc;
using CloudNative.CloudEvents;
using Microsoft.Extensions.Logging;

namespace CloudEventsSample.Controllers
{
    [ApiController]
    [Route("")]
    public class CloudEventsController : ControllerBase
    {
        private const string CloudEventResponseType = "dev.knative.docs.sample";
        private const string CloudEventResponseUri =
            "https://github.com/knative/docs/docs/serving/samples/cloudevents/cloudevents-dotnet";

        private readonly ILogger<CloudEventsController> logger;

        public CloudEventsController(ILogger<CloudEventsController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CloudEvent receivedEvent)
        {
            try
            {
                    return this.ReceiveAndReply(receivedEvent);
            }
            catch (JsonException)
            {
                return this.BadRequest("Failed to read the JSON data.");
            }
        }

        /// <summary>
        /// This is called whenever an event is received if K_SINK environment variable is NOT set.
        /// Replies with a new event.
        /// </summary>
        private IActionResult ReceiveAndReply(CloudEvent receivedEvent)
        {
//             this.logger?.LogInformation($"Received event {JsonSerializer.Serialize(receivedEvent)}");

            this.logger?.LogInformation($"Received event {receivedEvent}");
            var content = GetResponseForEvent(receivedEvent);
//             this.logger?.LogInformation($"Content of the event {JsonSerializer.Serialize(content)}");
            this.HttpContext.Response.RegisterForDispose(content);
            return new CloudEventActionResult(HttpStatusCode.OK, content);
        }

      
        /// <summary>
        /// Respond back with the JSON serialized request.
        /// </summary>
        [HttpPost, Route("echo")]
        public ActionResult Echo([FromBody] CloudEvent receivedEvent)
        {
            this.logger.LogInformation($"Echo: {JsonSerializer.Serialize(receivedEvent)}");
            return this.Ok(JsonSerializer.Serialize(receivedEvent));
        }

        private static CloudEventContent GetResponseForEvent(CloudEvent receivedEvent)
        {
            var input = JsonSerializer.Deserialize<SampleInput>(receivedEvent.Data.ToString());
            var content = new CloudEventContent
            (
                new CloudEvent(CloudEventResponseType, new Uri(CloudEventResponseUri))
                {
                    DataContentType = new ContentType(MediaTypeNames.Application.Json),
                    Data = new SampleOutput {Message = $"Hello there v1 !!!, {input.Name}"},
                },
                ContentMode.Structured,
                new JsonEventFormatter()
            );
            return content;
        }
    }
}
