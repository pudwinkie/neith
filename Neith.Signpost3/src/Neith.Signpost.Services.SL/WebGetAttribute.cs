
namespace System.ServiceModel.Web
{
    public class WebGetAttribute : Attribute
    {
        public string UriTemplate { get; set; }
        public WebMessageBodyStyle BodyStyle { get; set; }
    }

    public enum WebMessageBodyStyle
    {
        Bare,
        Wrapped,
        WrappedRequest,
        WrappedResponse,
    }

}
