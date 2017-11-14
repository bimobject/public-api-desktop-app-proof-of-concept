using System.Net;

namespace BIMobjectAPIDemoDesktopApp.Helpers
{
    public class ArrayResult<T>
    {
        public T[] Data { get; set; }
    }

    public class ObjectResult<T>
    {
        public T Data { get; set; }
    }

    public class Response<T>
    {
        public HttpStatusCode Status { get; set; }
        public T Result { get; set; }      
    }  
}
