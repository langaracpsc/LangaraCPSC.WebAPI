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
        Unknown = -1,
        FileNotFoundError,
        InvalidKeyError,
        InvalidQueryException,
        InvalidParamatersError,
        Forbidden
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

        public HttpError(HttpErrorType type, string message) : base(HttpReturnType.Error, message)
        {
            this.Type = type;
            this.Message = message;
        }
    }
}