using System;
using System.Collections.Generic;
using System.Text;

namespace Alexa.NET.Annotations.Markers
{
    public class IntentAttribute:Attribute
    {
        public IntentAttribute(string intent)
        {
            Intent = intent;
        }

        public string Intent { get; set; }
    }
}
