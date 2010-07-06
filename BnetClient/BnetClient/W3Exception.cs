using System;

namespace BnetClient
{
    class W3Exception : Exception
    {
        internal W3Exception(string text) 
            : base(text)
        {
        }
    }
}
