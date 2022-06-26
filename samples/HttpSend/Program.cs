// Copyright (c) Cloud Native Foundation.
// Licensed under the Apache 2.0 license.
// See LICENSE file in the project root for full license information.

using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Http;
using CloudNative.CloudEvents.NewtonsoftJson;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;

namespace HttpSend
{
    // This application uses the McMaster.Extensions.CommandLineUtils library for parsing the command
    // line and calling the application code. The [Option] attributes designate the parameters.
    class Program
    {
        [Option(Description = "CloudEvents 'source'", LongName = "source",
            ShortName = "s")]
        private string Source { get; } = "urn:example-com:mysource:abc";

        [Option(Description = "CloudEvents 'type'", LongName = "type", ShortName = "t")]
        private string Type { get; } = "com.example.myevent";
        [Option(Description = "CloudEvents 'subject'", LongName = "subject", ShortName = "")]
        private string Subject { get; } = "dotnetSampleApp";

        [Required,Option(Description = "HTTP(S) address to send the event to", LongName = "url", ShortName = "u"),]
        private Uri Url { get; }

        [Option(Description = "Accept any SSL/TLS certificate including self-signed")]
        private bool Insecure { get; }
        [Option(Description = "HTTP Basic Auth username", LongName = "username", ShortName = "U")]
        private string User { get; }
        [Option(Description = "HTTP Basic Auth password", LongName = "password", ShortName = "P")]
        private string Password { get; }

        public static Task<int> Main(string[] args) => CommandLineApplication.ExecuteAsync<Program>(args);
        private async Task OnExecuteAsync()
        {
            var cloudEvent = new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Type = Type,
                Source = new Uri(Source),
                DataContentType = MediaTypeNames.Application.Json,
                Subject = Subject,
                Data = JsonConvert.SerializeObject("hey there!")
            };

            var content = cloudEvent.ToHttpContent(ContentMode.Structured, new JsonEventFormatter());

            var httpClientHandler = new HttpClientHandler();
 
            if (Insecure)
            {
                httpClientHandler.ServerCertificateCustomValidationCallback =
                    (message, cert, chain, sslPolicyErrors) => true;
            }

            if (User != null)
            {
                httpClientHandler.UseDefaultCredentials = true;
                httpClientHandler.Credentials = new NetworkCredential(User, Password);
            }
            
            var httpClient = new HttpClient(httpClientHandler);
            var result = (await httpClient.PostAsync(this.Url, content));
            
            Console.WriteLine(result.StatusCode);
        }
    }
}