﻿
namespace GoLib.SGF
{
    public static class SgfHelper
    {
        public static string ToString(Goban goban)
        {
            return SgfWriter.ToSGF(goban);
        }

        public static Goban FromString(string sgf)
        {
            return SgfParser.SgfDecode(sgf);
        }
    }
}
