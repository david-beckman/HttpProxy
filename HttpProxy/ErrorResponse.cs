namespace HttpProxy
{
    using System.Net;

    using Newtonsoft.Json;

    /// <summary>
    ///     API Gateway Error Response
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        ///     The developer message.
        /// </summary>
        private string developerMessage;

        /// <summary>
        ///     Gets or sets the developer message.
        /// </summary>
        /// <value>
        ///     The developer message.
        /// </value>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DeveloperMessage
        {
            get => this.developerMessage ?? this.Message;
            set => this.developerMessage = value;
        }

        /// <summary>
        ///     Gets or sets the message.
        /// </summary>
        /// <value>
        ///     The message.
        /// </value>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        /// <summary>
        ///     Gets or sets the status code.
        /// </summary>
        /// <value>
        ///     The status code.
        /// </value>
        public HttpStatusCode StatusCode { get; set; }
    }
}