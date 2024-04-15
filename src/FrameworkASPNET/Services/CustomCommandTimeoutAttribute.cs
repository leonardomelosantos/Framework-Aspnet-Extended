using System;

namespace FrameworkAspNetExtended.Services
{
    public class CustomCommandTimeoutAttribute : Attribute
    {
        public int Seconds { get; private set; }

        public CustomCommandTimeoutAttribute() { }

        /// <summary>
        /// Seconds for operation timeout.
        /// </summary>
        /// <param name="seconds"></param>
        public CustomCommandTimeoutAttribute(int seconds)
        {
            this.Seconds = seconds;
        }
    }
}
