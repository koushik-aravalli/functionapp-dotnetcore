using System.Net;
namespace  Com.Sample.Responses
{
    public class PrincipalDataResponse{
        public string ObjectId { get; set; }
        public string TenantId { get; set; }
        public string DisplayName { get; set; }
        public bool IsLocal { get; set; }
        public CustomException Exception { get; set; }
    }

    public class CustomException
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
    }    
}