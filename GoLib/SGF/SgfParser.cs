using GoLib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GoLib.SGF
{
    enum Tokens
    {
        None = 0,
        CurlyOpen,
        CurlyClose,
        SemiColon,
        SquaredOpen,
        SquaredClose,
        UcLetter
    }

    delegate void MoveAction(Move move, char[] sgf, ref int index);
    delegate void GameInfoAction(GameInfo gameInfo, char[] sgf, ref int index);

    // TODO: transformer en classe non-static
    static class SgfParser
    {
        public static Goban SgfDecode(string sgf)
        {
            if (sgf == null) throw new ArgumentNullException("sgf");

            char[] charArray = sgf.ToCharArray();
            int index = 0;
            return ParseGameTree(charArray, ref index);
        }

        private static Goban ParseGameTree(char[] sgf, ref int index)
        {
            Check(sgf, ref index, Tokens.CurlyOpen);

            var gameInfo = ParseGameInfoProperties(sgf, ref index);

            var token = LookAhead(sgf, index);
            if (token == Tokens.CurlyClose)
            {
                return new Goban(gameInfo);
            }
            else
            {
                var moves = ParseMoves(sgf, ref index);

                Check(sgf, ref index, Tokens.CurlyClose);

                var goban = new Goban(gameInfo);
                // TODO: Add moves to goban
                return goban;
            }
        }

        private static GameInfo ParseGameInfoProperties(char[] sgf, ref int index)
        {
            Check(sgf, ref index, Tokens.SemiColon);

            var gameInfo = new GameInfo();

            while (LookAhead(sgf, index) == Tokens.UcLetter)
            {
                var ident = ParsePropIdent(sgf, ref index);
                AddGameInfoProperty(gameInfo, ident, sgf, ref index);
            }
            return gameInfo;
        }

        private static Tree<Move> ParseMoves(char[] sgf, ref int index)
        {
            var moves = new Tree<Move>();
            var current = moves;

            if (LookAhead(sgf, index) == Tokens.SemiColon)
            {
                while (LookAhead(sgf, index) == Tokens.SemiColon)
                {
                    NextToken(sgf, ref index);
                    var move = ParseMove(sgf, ref index);
                    current.Add(move);
                    current = current.Next;
                }

                if (LookAhead(sgf, index) != Tokens.CurlyOpen)
                {
                    return moves;
                }
            }

            Check(sgf, ref index, Tokens.CurlyOpen, false);

            while (LookAhead(sgf, index) == Tokens.CurlyOpen)
            {
                NextToken(sgf, ref index);
                var variation = ParseVariation(sgf, ref index);
                current.AddTree(variation);
            }
            return moves;
        }

        private static Tree<Move> ParseVariation(char[] sgf, ref int index)
        {
            //Check(sgf, ref index, Tokens.SemiColon);
            var moves = ParseMoves(sgf, ref index);
            Check(sgf, ref index, Tokens.CurlyClose);

            return moves;
        }

        private static Move ParseMove(char[] sgf, ref int index)
        {
            var move = new Move();
            while (LookAhead(sgf, index) == Tokens.UcLetter)
            {
                var ident = ParsePropIdent(sgf, ref index);
                AddMoveProperty(move, ident, sgf, ref index);
            }
            return move;
        }

        private static int ParseNumber(char[] sgf, ref int index)
        {
            EatWhitespace(sgf, ref index);

            int lastIndex = LastIndex(sgf, index);
            int length = (lastIndex - index) + 1;
            int number = number = int.Parse(new string(sgf, index, length));
            index += length;

            return number;
        }

        private static double ParseReal(char[] sgf, ref int index)
        {
            EatWhitespace(sgf, ref index);

            int lastIndex = LastIndex(sgf, index);
            int length = (lastIndex - index) + 1;
            double real = real = double.Parse(new string(sgf, index, length), System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture);
            index += length;

            return real;
        }

        private static int ParseDouble(char[] sgf, ref int index)
        {
            EatWhitespace(sgf, ref index);

            char c = sgf[index];
            if (c == '1' || c == '2')
            {
                index++;
                return (int)(c - '0');
            }
            else
            {
                throw new Exception(GetError(sgf, index, "Expected double"));
            }
        }

        private static Colour ParseColor(char[] sgf, ref int index)
        {
            EatWhitespace(sgf, ref index);

            char c = sgf[index];
            if (c == 'B')
            {
                index++;
                return Colour.Black;
            }
            else if (c == 'W')
            {
                index++;
                return Colour.White;
            }
            else
            {
                throw new Exception(GetError(sgf, index, "Expected color"));
            }
        }

        private static Point ParsePoint(char[] sgf, ref int index)
        {
            EatWhitespace(sgf, ref index);

            if (sgf.Length - index < 2)
            {
                throw new Exception(GetError(sgf, index, "Expected point"));
            }

            char x = sgf[index];
            char y = sgf[index + 1];
            Point point = point = Point.Parse(x, y);
            index += 2;

            return point; // TODO: check limits
        }

        private static string ParseText(char[] sgf, ref int index)
        {
            var s = new StringBuilder();

            for (; index < sgf.Length; ++index)
            {
                char c = sgf[index];
                if (c == ']')
                {
                    break;
                }
                else if (c == '\\')
                {
                    if (index == sgf.Length)
                    {
                        throw new Exception(GetError(sgf, index, "A string must end with ']'"));
                    }
                    s.Append(sgf[++index]);
                }
                else
                {
                    s.Append(c);
                }
            }

            return s.ToString();
        }

        private static string ParsePropIdent(char[] sgf, ref int index)
        {
            EatWhitespace(sgf, ref index);

            char c1 = sgf[index++];
            if (LookAhead(sgf, index) == Tokens.UcLetter)
            {
                char c2 = sgf[index++];
                return c1.ToString() + c2.ToString();
            }
            else
            {
                return c1.ToString();
            }
        }

        #region parse util

        private static Tokens LookAhead(char[] sgf, int index)
        {
            int saveIndex = index;
            return NextToken(sgf, ref saveIndex);
        }

        private static Tokens NextToken(char[] sgf, ref int index)
        {
            EatWhitespace(sgf, ref index);

            if (index >= sgf.Length)
            {
                return Tokens.None;
            }

            char c = sgf[index++];
            switch (c)
            {
                case '(':
                    return Tokens.CurlyOpen;
                case ')':
                    return Tokens.CurlyClose;
                case ';':
                    return Tokens.SemiColon;
                case '[':
                    return Tokens.SquaredOpen;
                case ']':
                    return Tokens.SquaredClose;
            }

            if (char.IsUpper(c))
                return Tokens.UcLetter;

            return Tokens.None;
        }

        private static void Check(char[] sgf, ref int index, Tokens expected, bool peek = true)
        {
            var found = LookAhead(sgf, index);
            if (found != expected)
                throw new Exception(GetError(sgf, index, "Expected: " + expected + " found: " + found));
            if (peek)
                NextToken(sgf, ref index);
        }

        private static void EatWhitespace(char[] sgf, ref int index)
        {
            while (index < sgf.Length && " \t\n\r".IndexOf(sgf[index]) != -1)
            {
                ++index;
            }
        }

        private static int LastIndex(char[] sgf, int index)
        {
            int lastIndex = index;

            while (lastIndex < sgf.Length && "0123456789+-.".IndexOf(sgf[lastIndex]) != -1)
            {
                ++lastIndex;
            }
            return lastIndex - 1;
        }

        #endregion

        #region Properties

        private static Dictionary<string, MoveAction> _moveActions;
        private static Dictionary<string, GameInfoAction> _gameInfoActions;

        private static void AddGameInfoProperty(GameInfo gameInfo, string ident, char[] sgf, ref int index)
        {
            Check(sgf, ref index, Tokens.SquaredOpen);

            var action = _gameInfoActions[ident];
            if (action != null)
            {
                action(gameInfo, sgf, ref index);
            }
            else
            {
                string b = ParseText(sgf, ref index);
                Warning(sgf, index, "Unsupported property: " + b);
                // TODO: Add to an internal dictionary
            }

            Check(sgf, ref index, Tokens.SquaredClose);
        }

        private static void AddMoveProperty(Move move, string ident, char[] sgf, ref int index)
        {
            Check(sgf, ref index, Tokens.SquaredOpen);

            var action = _moveActions[ident];
            if (action != null)
            {
                action(move, sgf, ref index);
            }
            else
            {
                string b = ParseText(sgf, ref index);
                Warning(sgf, index, "Unsupported property: " + b);
                // TODO: Add to an internal dictionary
            }

            Check(sgf, ref index, Tokens.SquaredClose);
        }

        static SgfParser()
        {
            _gameInfoActions = new Dictionary<string, GameInfoAction>();
            _moveActions = new Dictionary<string, MoveAction>();

            // Root properties
            _gameInfoActions["SZ"] = ParseSize;
            _gameInfoActions["GM"] = ParseGameId;
            _gameInfoActions["FF"] = ParseFileFormat;
            _gameInfoActions["CA"] = ParseCharset;
            _gameInfoActions["AP"] = ParseApplication;
            _gameInfoActions["ST"] = ParseStyle;

            // Game-Info properties
            _gameInfoActions["RU"] = ParseRules;
            _gameInfoActions["RE"] = ParseResult;
            _gameInfoActions["HA"] = ParseHandicap;
            _gameInfoActions["KM"] = ParseKomi;

            _gameInfoActions["AB"] = ParseHandicapBlack;
            _gameInfoActions["AW"] = ParseHandicapWhite;

            _gameInfoActions["PB"] = ParsePlayerBlack;
            _gameInfoActions["PW"] = ParsePlayerWhite;



            // Move properties
            _moveActions["B"] = ParseBlackMove;
            _moveActions["W"] = ParseWhiteMove;
            _moveActions["MN"] = null; // Move number
            _moveActions["KO"] = null; // Ko

            // Setup-Properties
            _moveActions["AB"] = ParseAddBlack;
            _moveActions["AW"] = ParseAddWhite;
            _moveActions["AE"] = ParseAddEmpty;
            _moveActions["PL"] = null; // Player to play

            // Node-Annotations
            _moveActions["TB"] = null; // Territories Black
            _moveActions["TW"] = null; // Territories White
            _moveActions["C"] = null; // Comments
            _moveActions["DM"] = null; // Position is even
            _moveActions["GB"] = null; // Good for Black
            _moveActions["GW"] = null; // Good for White
            _moveActions["HO"] = null; // Hotspot
            _moveActions["N"] = null; // Node Name
            _moveActions["UC"] = null; // Unclear
            _moveActions["V"] = null; // Value

            // Markup Properties
            _moveActions["CR"] = null; // Circle
            _moveActions["LB"] = null; // Label
            _moveActions["MA"] = null; // Mark
            _moveActions["SQ"] = null; // Square
            _moveActions["TR"] = null; // Triangle
        }

        #endregion

        #region GameInfo properties

        private static void ParseRules(GameInfo gameInfo, char[] sgf, ref int index)
        {
            string value = ParseText(sgf, ref index);
            try
            {
                Rules rule = (Rules)Enum.Parse(typeof(Rules), value, true);
                gameInfo.Rule = rule;
            }
            catch (Exception)
            {
                throw new Exception(GetError(sgf, index, "Suported rules: chinese and japanese"));
            }
        }

        // simpletext
        private static void ParseResult(GameInfo gameInfo, char[] sgf, ref int index)
        {
            throw new NotImplementedException("");
        }

        private static void ParseHandicap(GameInfo gameInfo, char[] sgf, ref int index)
        {
            int handicap = ParseNumber(sgf, ref index);
            if (0 <= handicap && handicap <= 9)
            {
                gameInfo.Handicap = handicap;
            }
            else
            {
                throw new Exception(GetError(sgf, index, "Handicap must be in [0-9] range"));
            }
        }

        private static void ParseKomi(GameInfo gameInfo, char[] sgf, ref int index)
        {
            double komi = ParseReal(sgf, ref index);

            if (komi == Math.Floor(komi) || komi - 0.5 == Math.Floor(komi))
            {
                gameInfo.Komi = komi;
            }
            else
            {
                throw new Exception(GetError(sgf, index, "Komi must be set to x.0 or x.5, x an integer"));
            }
        }

        // list of point
        private static void ParseHandicapBlack(GameInfo gameInfo, char[] sgf, ref int index)
        {
            throw new NotImplementedException("ParseHandicapBlack");
        }

        // list of point
        private static void ParseHandicapWhite(GameInfo gameInfo, char[] sgf, ref int index)
        {
            throw new NotImplementedException("");
        }

        private static void ParsePlayerBlack(GameInfo gameInfo, char[] sgf, ref int index)
        {
            gameInfo.Players[0].Name = ParseText(sgf, ref index);
        }

        private static void ParsePlayerWhite(GameInfo gameInfo, char[] sgf, ref int index)
        {
            gameInfo.Players[1].Name = ParseText(sgf, ref index);
        }

        #endregion

        #region Root properties

        private static void ParseSize(GameInfo gameInfo, char[] sgf, ref int index)
        {
            int size = ParseNumber(sgf, ref index);
            if (size == 9 || size == 13 || size == 19)
            {
                gameInfo.Size = size;
            }
            else
            {
                throw new Exception(GetError(sgf, index, "Supported sizes: 9x9, 13x13 or 19x19"));
            }
        }

        private static void ParseGameId(GameInfo gameInfo, char[] sgf, ref int index)
        {
            if (ParseNumber(sgf, ref index) != 1)
                throw new Exception(GetError(sgf, index, "GM property must be set to 1 (Go Game)"));
        }

        private static void ParseFileFormat(GameInfo gameInfo, char[] sgf, ref int index)
        {
            if (ParseNumber(sgf, ref index) != 4)
                throw new Exception(GetError(sgf, index, "FF property must be set to '4'"));
        }

        private static void ParseCharset(GameInfo gameInfo, char[] sgf, ref int index)
        {
            ParseText(sgf, ref index);
        }

        private static void ParseApplication(GameInfo gameInfo, char[] sgf, ref int index)
        {
            ParseText(sgf, ref index); // Useless, will be overwriten by "Kifu" once game saved
        }

        private static void ParseStyle(GameInfo gameInfo, char[] sgf, ref int index)
        {
            ParseNumber(sgf, ref index); // Useless, will be onverwriten by "4" once game saved
        }

        #endregion

        #region Move properties

        // point
        private static void ParseBlackMove(Move move, char[] sgf, ref int index)
        {
            var point = ParsePoint(sgf, ref index);
            move.Stone = new Stone(Colour.Black, point);
        }

        // point
        private static void ParseWhiteMove(Move move, char[] sgf, ref int index)
        {
            var point = ParsePoint(sgf, ref index);
            move.Stone = new Stone(Colour.White, point);
        }

        #endregion

        #region setup properties

        private static void ParseAddBlack(Move move, char[] sgf, ref int index)
        {
            throw new NotImplementedException("");
        }

        private static void ParseAddWhite(Move move, char[] sgf, ref int index)
        {
            throw new NotImplementedException("");
        }

        private static void ParseAddEmpty(Move move, char[] sgf, ref int index)
        {
            throw new NotImplementedException("");
        }

        #endregion

        #region other properties

        // créer ces methodes pour moves aussi ?
        // -> imbriguer un move dans GameInfo

        private static void ParseComment(Move move, char[] sgf, ref int index)
        {
            throw new NotImplementedException("");
        }

        private static void ParseLabel(Move move, char[] sgf, ref int index)
        {
            throw new NotImplementedException("");
        }

        private static void ParseCircle(Move move, char[] sgf, ref int index)
        {
            throw new NotImplementedException("");
        }

        private static void ParseSquare(Move move, char[] sgf, ref int index)
        {
            throw new NotImplementedException("");
        }

        private static void ParseTriangle(Move move, char[] sgf, ref int index)
        {
            throw new NotImplementedException("");
        }

        private static void ParseMark(Move move, char[] sgf, ref int index)
        {
            throw new NotImplementedException("");
        }

        #endregion

        #region Error handling

        private static string GetError(char[] sgf, int index, string message)
        {
            int line, column;
            CursorPosition(sgf, index, out line, out column);
            return "Error: " + message + " line : " + line + " column " + column;
        }

        private static void Warning(char[] sgf, int index, string message)
        {
            int line, column;
            CursorPosition(sgf, index, out line, out column);
            Debug.WriteLine(message + " line : " + line + " column " + column);
        }

        private static void CursorPosition(char[] sgf, int index, out int line, out int column)
        {
            line = 1;
            column = 1;
            for (int i = 0; i <= index; ++i)
            {
                char c = sgf[i];
                if (c == '\n')
                {
                    line++;
                    column = 1;
                    if (i <= index && sgf[i + 1] == '\r')
                    {
                        i++;
                    }
                }
                else if (c == '\r')
                {
                    line++;
                    column = 1;
                    if (i <= index && sgf[i + 1] == '\n')
                    {
                        i++;
                    }
                }
                else
                {
                    column++;
                }
            }
        }

        #endregion
    }
}
