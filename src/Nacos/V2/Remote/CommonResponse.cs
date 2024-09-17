namespace Nacos.V2.Remote
{
    public abstract class CommonResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("resultCode")]
        public int ResultCode { get; set; } = 200;

        [System.Text.Json.Serialization.JsonPropertyName("errorCode")]
        public int ErrorCode { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("message")]
        public string Message { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("requestId")]
        public string RequestId { get; set; }

        public bool IsSuccess() => ResultCode == 200;

        public abstract string GetRemoteType();
    }
}
