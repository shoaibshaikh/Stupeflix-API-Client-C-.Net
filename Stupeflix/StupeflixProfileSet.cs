using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stupeflix
{
    public class StupeflixProfileSet
    {
        protected String[] profiles;

        public StupeflixProfileSet(String[] profiles)
        {
            this.profiles = profiles;
        }


        public override string ToString()
        {
            String ret = "<profiles>";
            for (int i = 0; i < this.profiles.Length; i++)
            {
                ret += "<profile name=\"" + this.profiles[i] + "\">";
                ret += "<stupeflixStore />";
                ret += "</profile>";
            }
            ret += "</profiles>";
            return ret;
        }

        
    }

}
