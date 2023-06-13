using System.Text.Json.Serialization;
using OpenDatabase.Json;

using Newtonsoft.Json;

namespace LangaraCPSC.WebAPI
{
    public enum HttpReturnType
    {
        Error,
        Success
    }

    public enum HttpErrorType
    {
        FileNotFoundError,
        InvalidKeyError,
        InvalidQueryException
    }

    public class HttpObject
    {
        public HttpReturnType Type;

        public object Payload;

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public HttpObject(HttpReturnType type = HttpReturnType.Success, object payload = null)
        {
            this.Type = type;
            this.Payload = payload;
        }
    }

    public class HttpError : HttpObject
    {
        public HttpErrorType Type;

        public string Message;

        public HttpError(HttpErrorType type, string message) : base(HttpReturnType.Error)
        {
            this.Type = type;
            this.Message = message;
        }
    }
}