using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketWeb.Model
{
    /*********************************************************************************************/
    /* Some function is not available from Blazor, and some javascript needs to be used          */
    /*********************************************************************************************/
    public class JSCalls
    {
        // JSProcessRuntime which will run the JS command syncronousily
        private readonly IJSInProcessRuntime _js;

        public JSCalls(IJSInProcessRuntime js)
        {
            _js = js;
        }

        // Get the size of the browser window in pixels
        public BrowserDimension GetBrowserDimension()
        {
            return _js.Invoke<BrowserDimension>("getDimensions");
        }

        // Get information about the selected elements: widht, hight, distance from top/bottom/left/right, etc
        public BoundingClientRect GetBounds(string element)
        {
            return _js.Invoke<BoundingClientRect>("GetDivInformation", element);
        }

        // Save into the local storage
        public void SetLocalStorage(string key, string value)
        {
            _js.InvokeVoid("localStorage.setItem", key, value);
        }

        // Load value from browser storage
        public string GetLocalStorage(string key)
        {
            return _js.Invoke<string>("localStorage.getItem", key);
        }

        // Remove browser storage
        public void RemoveLocalStorage(string key)
        {
            _js.InvokeVoid("localStorage.removeItem", key);
        }
    }

    /*********************************************************************************************/
    /* Class to represent screen size                                                            */
    /*********************************************************************************************/
    public class BrowserDimension
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }

    /*********************************************************************************************/
    /* Class to represent information about an element                                           */
    /*********************************************************************************************/
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
