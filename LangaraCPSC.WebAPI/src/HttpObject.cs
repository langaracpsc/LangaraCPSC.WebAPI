using Newtonsoft.Json;

namespace LangaraCPSC.WebAPI
{
    /// <summary>
    /// Represents the type of HTTP response.
    /// </summary>
    public enum HttpReturnType
    {
        /// <summary>
        /// Indicates an error response.
        /// </summary>
        Error,

        /// <summary>
        /// Indicates a successful response.
        /// </summary>
        Success
    }

    /// <summary>
    /// Represents the type of HTTP error.
    /// </summary>
    public enum HttpErrorType
    {
        /// <summary>
        /// Indicates an unknown error.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// Indicates a file not found error.
        /// </summary>
        FileNotFoundError,

        /// <summary>
        /// Indicates an invalid key error.
        /// </summary>
        InvalidKeyError,

        /// <summary>
        /// Indicates an invalid query exception.
        /// </summary>
        InvalidQueryException,

        /// <summary>
        /// Indicates an invalid parameters error.
        /// </summary>
        InvalidParamatersError,

        /// <summary>
        /// Indicates a forbidden error.
        /// </summary>
        Forbidden
    }

    /// <summary>
    /// Represents an HTTP response object.
    /// </summary>
    public class HttpObject
    {
        /// <summary>
        /// The type of HTTP response.
        /// </summary>
        public HttpReturnType Type;

        /// <summary>
        /// The payload of the HTTP response.
        /// </summary>
        public object Payload;

        /// <summary>
        /// Converts the current instance of the <see cref="HttpObject"/> class to a JSON string.
        /// </summary>
        /// <returns>A JSON string representation of the <see cref="HttpObject"/> instance.</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpObject"/> class.
        /// </summary>
        /// <param name="type">The type of HTTP response.</param>
        /// <param name="payload">The payload of the HTTP response.</param>
        public HttpObject(HttpReturnType type = HttpReturnType.Success, object payload = null)
        {
            this.Type = type;
            this.Payload = payload;
        }
    }

    /// <summary>
    /// Represents an HTTP error response object.
    /// </summary>
    public class HttpError : HttpObject
    {
        /// <summary>
        /// The type of HTTP error.
        /// </summary>
        public HttpErrorType Type;

        /// <summary>
        /// The error message.
        /// </summary>
        public string Message;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpError"/> class.
        /// </summary>
        /// <param name="type">The type of HTTP error.</param>
        /// <param name="message">The error message.</param>
        public HttpError(HttpErrorType type, string message) : base(HttpReturnType.Error, message)
        {
            this.Type = type;
            this.Message = message;
        }
    }
}