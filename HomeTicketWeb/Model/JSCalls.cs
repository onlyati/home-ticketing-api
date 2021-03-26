using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketWeb.Model
{
    public class JSCalls
    {
        private readonly IJSInProcessRuntime _js;

        public JSCalls(IJSInProcessRuntime js)
        {
            _js = js;
        }

        public BrowserDimension GetBrowserDimensionAsync()
        {
            return _js.Invoke<BrowserDimension>("getDimensions");
        }

        public BoundingClientRect GetBounds(string element)
        {
            return _js.Invoke<BoundingClientRect>("GetDivInformation", element);
        }
    }

    public class BrowserDimension
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class BoundingClientRect
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Top { get; set; }
        public double Right { get; set; }
        public double Bottom { get; set; }
        public double Left { get; set; }
    }
}
