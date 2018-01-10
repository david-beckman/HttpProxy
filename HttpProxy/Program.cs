namespace HttpProxy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Amazon.Lambda.APIGatewayEvents;
    using Amazon.Lambda.Core;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class Program
    {
        /// <summary>
        ///     The cancellation timeout in milliseconds (10s).
        /// </summary>
        private const int CancellationTimeoutInMilliseconds = 10000;

        private const string ContentTypeHttpHeaderName = "Content-Type";

        private const string ContentTypePropertyName = ContentTypeHttpHeaderName;

        private const string EntityBodyPropertyName = "entityBody";

        private const string MethodPropertyName = "method";

        private const string RequestUriPropertyName = "requestUri";

        private static readonly string[] SpecialPropertyNames =
            { ContentTypePropertyName, EntityBodyPropertyName, MethodPropertyName, RequestUriPropertyName };

        private static readonly HttpClient client = new HttpClient();

        public async Task<APIGatewayProxyResponse> Proxy(APIGatewayProxyRequest request, ILambdaContext context)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            HttpRequestMessage requestMessage;

            try
            {
                requestMessage = ParseRequest(request);
            }
            catch (Exception e)
            {
                return BuildResponse(
                    new ErrorResponse
                        {
                            StatusCode = HttpStatusCode.BadRequest,
                            Message = "Body is not valid JSON or does not match the expected schema.",
                            DeveloperMessage = e.ToString()
                        });
            }

            try
            {
                using (requestMessage)
                {
                    return await ProxyResponse(requestMessage);
                }
            }
            catch (Exception e)
            {
                return BuildResponse(
                    new ErrorResponse
                        {
                            StatusCode = HttpStatusCode.BadGateway,
                            Message = "An error occurred performing the proxy",
                            DeveloperMessage = e.ToString()
                        });
            }
        }

        private static APIGatewayProxyResponse BuildResponse(ErrorResponse errorResponse)
        {
            if (errorResponse == null)
            {
                throw new ArgumentNullException(nameof(errorResponse));
            }

            return new APIGatewayProxyResponse
                       {
                           StatusCode = (int)errorResponse.StatusCode,
                           Body = JsonConvert.SerializeObject(errorResponse),
                           Headers = new Dictionary<string, string>
                                         {
                                             {
                                                 ContentTypeHttpHeaderName,
                                                 "application/json"
                                             }
                                         }
                       };
        }

        private static HttpRequestMessage ParseRequest(APIGatewayProxyRequest request)
        {
            var body = JObject.Parse(request.Body);

            HttpRequestMessage requestMessage = null;

            try
            {
                requestMessage = new HttpRequestMessage(
                    new HttpMethod(body[MethodPropertyName]?.Value<string>()),
                    body[RequestUriPropertyName]?.Value<string>());

                var requestBody = body[EntityBodyPropertyName]?.Value<string>();
                if (!string.IsNullOrEmpty(requestBody))
                {
                    var contentType = body[ContentTypePropertyName]?.Value<string>();
                    requestMessage.Content = !string.IsNullOrEmpty(contentType)
                                                 ? new StringContent(requestBody, Encoding.UTF8, contentType)
                                                 : new StringContent(requestBody, Encoding.UTF8);
                }

                foreach (var property in body)
                {
                    if (SpecialPropertyNames.Contains(property.Key))
                    {
                        continue;
                    }

                    requestMessage.Headers.Add(property.Key, property.Value.Values<string>());
                }

                return requestMessage;
            }
            catch
            {
                requestMessage?.Dispose();
                throw;
            }
        }

        private static async Task<APIGatewayProxyResponse> ProxyResponse(HttpRequestMessage requestMessage)
        {
            using (var cancellationSource = new CancellationTokenSource(CancellationTimeoutInMilliseconds))
            {
                using (var response = await client.SendAsync(
                                          requestMessage,
                                          HttpCompletionOption.ResponseContentRead,
                                          cancellationSource.Token))
                {
                    var proxyResponse = new APIGatewayProxyResponse
                                            {
                                                Body =
                                                    await response.Content.ReadAsStringAsync(),
                                                StatusCode = (int)response.StatusCode
                                            };

                    if (response.Headers != null)
                    {
                        proxyResponse.Headers = new Dictionary<string, string>();
                        foreach (var header in response.Headers)
                        {
                            proxyResponse.Headers.Add(header.Key, string.Join(", ", header.Value));
                        }
                    }

                    return proxyResponse;
                }
            }
        }
    }
}
