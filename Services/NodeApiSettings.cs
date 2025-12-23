namespace digitalpayment3.Services
{
    public class NodeApiSettings
    {
        public string BaseUrl { get; set; } = "http://localhost:3000";
        public string GrpcUrl { get; set; } = "http://localhost:50051";
        public string SoapUrl { get; set; } = "http://localhost:3001";
    }
}
