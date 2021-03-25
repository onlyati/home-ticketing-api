using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketWeb.Model
{
    public class JSCalls
    {
        private readonly IJSRuntime _js;

        public JSCalls(IJSRuntime js)
        {
            _js = js;
        }

        public async Task<BrowserDimension> GetBrowserDimensionAsync()
        {
            return await _js.InvokeAsync<BrowserDimension>("getDimensions");
        }

        public async Task<BoundingClientRect> GetBounds(string element)
        {
            return await _js.InvokeAsync<BoundingClientRect>("GetDivInformation", element);
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
