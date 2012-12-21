using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GoLib.SGF
{
    public static class SgfHelper
    {
        public static String ToString(Goban goban)
        {
            return SgfWriter.ToSGF(goban);
        }
    }
}
