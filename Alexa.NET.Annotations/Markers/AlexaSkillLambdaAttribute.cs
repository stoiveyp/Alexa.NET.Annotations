using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alexa.NET.Annotations.Markers
{
    public class AlexaSkillAttribute : Attribute
    {
        public AlexaSkillAttribute(){}

        public AlexaSkillAttribute(Type type){}
    }
}
