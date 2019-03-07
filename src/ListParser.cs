using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace lieutenantgames.listparser {
    public enum ParseState : int {
        all = -1,
        none = 0,

        insideBody,
        insideQuotedString,
        insideUnquotedString,
        insideNeutral,
    }
    public enum TokenType : int {
        all = -1,
        none = 0,

        document,
        list,
        quotedString,
        unquotedString,
        neutral,
    }

    public enum ParsedStringType {
        all = -1,
        none = 0,

        _ulong,
        _long,
        _double,
        _bool,
        _string,
    }

    public struct TokenPos {
        public string m_source;
        public TokenType m_type;
        public int m_start; // inclusive. first index belonging to token
        public int m_end; // exclusive. first index not belonging to token

        public bool isDefault () {
            return m_source == null &&
                m_type == 0 &&
                m_start == 0 &&
                m_end == 0;
        }

        public string extract () {
            return extract (true);
        }
        public string extract (bool unquote) {
            var _start = m_start;
            var _end = m_end;
            if (unquote) {
                if (m_source[m_start] == '"') {
                    _start++;
                    _end--;
                }
            }
            if (_start >= _end)
                return "";

            return m_source.Substring (_start, _end - _start);
        }

        public ParsedString parse () {
            ParsedString retVal = new ParsedString();
            var str = this.extract ();

            ulong ul;
            if (ulong.TryParse (str, out ul)) {
                retVal._ulong = ul;
                retVal.parsedType = ParsedStringType._ulong;
                return retVal;
            }

            long l;
            if (long.TryParse (str, out l)) {
                retVal._long = l;
                retVal.parsedType = ParsedStringType._long;
                return retVal;
            }

            double d;
            if (double.TryParse (str, NumberStyles.Float, CultureInfo.InvariantCulture, out d)) {
                retVal._double = d;
                retVal.parsedType = ParsedStringType._double;
                return retVal;
            }

            bool b;
            if (bool.TryParse (str, out b)) {
                retVal._bool = b;
                retVal.parsedType = ParsedStringType._bool;
                return retVal;
            }

            retVal._string = str;
            retVal.parsedType = ParsedStringType._string;
            return retVal;
        }
    }

    public struct ListOrValue {
        public List<ListOrValue> m_list;
        public TokenPos m_value;
        public bool isDefault () {
            return m_list == null && m_value.isDefault ();
        }
        public string toJson () {
            if (m_list == null) {
                var extracted = m_value.extract ();
                if (extracted.StartsWith ("\""))
                    return extracted;
                return $"\"{extracted}\"";
            }
            var len = m_list.Count;
            var sb = new StringBuilder ();
            sb.Append ("[");
            for (var i = 0; i < len; i++) {
                sb.Append (m_list[i].toJson ());
                sb.Append (",");
            }
            sb.Remove (sb.Length - 1, 1);
            sb.Append ("]");
            return sb.ToString ();
        }
        public string toLisp () {
            return toLisp (false, 0, true, 1);
        }

        public string toLisp (bool prettyPrint) {
            return toLisp (prettyPrint, 0, true, 1);
        }

        public string toLisp (bool prettyPrint, int level, bool newline, int ident) {
            if (!prettyPrint)
                return m_value.m_source;
            if (ident < 1)
                ident = 1;

            if (m_list == null) {
                var extracted = m_value.extract (false);
                var identchars = level * ident;
                if (identchars < 1) {
                    identchars = ident - 1;
                }
                return $"{new string (' ', identchars)}{extracted}{(newline ? "\n" : "")}";
            }
            // else, it is a list
            var len = m_list.Count;
            var sb = new StringBuilder (64); // initial capacity of 64 chars
            sb.Append (new string (' ', level * ident));
            if (m_value.m_type != TokenType.document)
                sb.Append ("(");
            level++;
            for (var i = 0; i < len; i++) {
                sb.Append (m_list[i].toLisp (true, i > 0 ? level : 0, i < len - 1, ident));
            }
            // sb.Append (new string (' ', level - 1));
            if (m_value.m_type != TokenType.document)
                sb.Append (")\n");
            return sb.ToString ();
        }
    }

    public struct ParsedString {
        public ulong _ulong;
        public long _long;
        public double _double;
        public bool _bool;
        public string _string;
        public ParsedStringType parsedType;
    }
    // TODO rename from ListParser to ListProcessor
    public static class ListParser {

        // since lists can be nested, liststar and listend must be different chars
        public const char listStart = '('; // cannot be same of list end
        public const char listEnd = ')'; // cannot be same of list start
        public const char stringStart = '"';
        public const char stringEnd = '"';
        public const char escape = '\\'; // only inside escaped strings

        // returns the number of tokens or -1 on error

        public static TokenPos[] tokenize (string src) {
            var count = tokenize (src, null);
            if (count < 0)
                return null;
            var tokens = new TokenPos[count];
            count = tokenize (src, tokens);
            return tokens;
        }
        public static int tokenize (string self, TokenPos[] tokens) {
            if (self == null || self == "")
                return -1;
            // var tokenCount = 0; // TODO delete
            // var lastState = ParseState.insideBody;
            var curState = ParseState.insideBody;
            var lastToken = TokenType.none;
            var strLen = self.Length;
            // var neutralLen = neutral.Length;
            // var isInsideString = false;
            var listStartCount = 0;
            var listEndCount = 0;
            var tokenIdx = 0;
            var nextIsScaped = false;
            Stack<int> listStartStack = null;
            var tokensLen = 0;
            if (tokens != null) {
                tokensLen = tokens.Length;
                listStartStack = new Stack<int> (tokensLen); // TODO think of an optimized way without heap allocation
            }

            for (var i = 0; i < strLen; i++) {
                var cur = self[i];

                switch (curState) {
                case ParseState.insideBody:
                    switch (cur) {
                    case listStart:

                        listStartCount++;
                        if (tokens != null) {
                            listStartStack.Push (tokenIdx);
                            tokens[tokenIdx] = new TokenPos () { m_source = self, m_type = TokenType.list, m_start = i };
                        }
                        tokenIdx++;
                        lastToken = TokenType.list;
                        break;

                    case listEnd:
                        listEndCount++;
                        if (tokens != null) {
                            var idx = listStartStack.Pop ();
                            tokens[idx].m_end = i + 1;
                        }
                        lastToken = TokenType.list;
                        break;

                    case stringStart:

                        curState = ParseState.insideQuotedString;
                        if (tokens != null) {
                            tokens[tokenIdx] = new TokenPos () { m_source = self, m_type = TokenType.quotedString, m_start = i };
                        }
                        tokenIdx++;
                        lastToken = TokenType.quotedString;
                        break;

                    case escape:
                        return -1; // syntax error, can only use scape char inside identifier or string

                    default:
                        // it could be entering a neutral section
                        if (cur < 33) { // 33 is the first printable ascii char, the exclamation mark '!'

                            curState = ParseState.insideNeutral;
                            if (tokens != null && lastToken == TokenType.unquotedString) { // close last unquoted string if any
                                tokens[tokenIdx - 1].m_end = i;
                            }
                            if (tokens != null) {
                                tokens[tokenIdx] = new TokenPos () { m_source = self, m_type = TokenType.neutral, m_start = i };
                            }
                            tokenIdx++;
                            lastToken = TokenType.neutral;
                            break;
                        }
                        // else, it is starting a identifier
                        if (tokens != null && lastToken == TokenType.neutral) { // close last neutral if any
                            tokens[tokenIdx - 1].m_end = i;
                        }

                        if (tokens != null) {
                            tokens[tokenIdx] = new TokenPos () { m_source = self, m_type = TokenType.unquotedString, m_start = i };
                        }
                        tokenIdx++;

                        lastToken = TokenType.unquotedString;
                        curState = ParseState.insideUnquotedString;
                        break;
                    }
                    break;

                case ParseState.insideQuotedString:
                    switch (cur) {
                    case escape:
                        if (nextIsScaped) {
                            nextIsScaped = false;
                            break;
                        }
                        nextIsScaped = true;
                        break;

                    case stringEnd:
                        if (nextIsScaped) {
                            nextIsScaped = false;
                            break;
                        }
                        if (tokens != null) {
                            tokens[tokenIdx - 1].m_end = i + 1;
                        }
                        curState = ParseState.insideBody;
                        lastToken = TokenType.quotedString;
                        break;

                    default:
                        break; // normal string characters
                    }
                    break;

                case ParseState.insideUnquotedString:
                    switch (cur) {
                    case listStart:
                        if (tokens != null) { // close last unquoted string
                            tokens[tokenIdx - 1].m_end = i;
                        }
                        tokenIdx++;
                        i--; // must reevaluate character
                        curState = ParseState.insideBody;
                        break;

                    case listEnd:
                        if (tokens != null) { // close last unquoted string
                            tokens[tokenIdx - 1].m_end = i;
                        }
                        i--; // must reevaluate character
                        curState = ParseState.insideBody;
                        break;

                    case stringStart:
                        return -1;

                    case escape:
                        return -1;

                    default:
                        if (cur > 32)
                            break;
                        // could be starting a neutral //

                        if (tokens != null && lastToken == TokenType.unquotedString) { // close last unquoted if any
                            tokens[tokenIdx - 1].m_end = i;
                        }

                        if (tokens != null) {
                            tokens[tokenIdx] = new TokenPos () { m_source = self, m_type = TokenType.neutral, m_start = i };
                        }
                        tokenIdx++;
                        curState = ParseState.insideNeutral;
                        lastToken = TokenType.unquotedString;
                        break;

                    }
                    break;

                case ParseState.insideNeutral:
                    if (cur < 33)
                        break;

                    if (tokens != null) { // close last neutral
                        tokens[tokenIdx - 1].m_end = i;
                    }
                    i--; // must reevaluate character
                    curState = ParseState.insideBody;
                    lastToken = TokenType.neutral;
                    break;
                }
            }

            if (curState == ParseState.insideQuotedString)
                return -1; // unclosed enquoted string

            if (listStartCount != listEndCount)
                return -1; // unatched open/close list

            if (tokens != null && tokensLen > 0 && tokens[tokensLen - 1].m_end < 1) // close last token
                tokens[tokensLen - 1].m_end = strLen;

            return tokenIdx;
        }

        public static ListOrValue parseRoot (TokenPos[] tokens) {
            ListOrValue retVal = new ListOrValue();
            if (tokens == null || tokens.Length < 1)
                return retVal;

            retVal.m_value.m_type = TokenType.document;
            retVal.m_value.m_start = 0;
            retVal.m_value.m_source = tokens[0].m_source;
            retVal.m_value.m_end = retVal.m_value.m_source.Length;
            retVal.m_list = new List<ListOrValue> ();

            var tokensLength = tokens.Length;
            int idx = 0;

            // retVal.list = new List<ListOrValue> ();
            while (idx < tokensLength) {
                var item = parseToken (tokens, idx, out idx);
                if (item.isDefault ())
                    continue;
                retVal.m_list.Add (item);
            }
            if (retVal.m_list.Count < 1)
                return new ListOrValue();

            return retVal;
        }

        public static ListOrValue parseToken (TokenPos[] tokens, int curIdx, out int nextIdx) {
            nextIdx = curIdx + 1;
            ListOrValue curListOrVal = new ListOrValue();
            var curTok = tokens[curIdx];
            var tokensLength = tokens.Length;

            if (curTok.m_type == TokenType.neutral)
                return curListOrVal;

            curListOrVal.m_value = curTok;
            if (curTok.m_type != TokenType.list)
                return curListOrVal;

            // is list
            curListOrVal.m_list = new List<ListOrValue> ();
            while (nextIdx < tokensLength) {
                if (tokens[nextIdx].m_start >= curTok.m_end)
                    break;
                var item = parseToken (tokens, nextIdx, out nextIdx);
                if (item.isDefault ())
                    continue;
                curListOrVal.m_list.Add (item);
            }
            return curListOrVal;
        }

        // public static void parse (TokenPos[] tokens, int index, out int nextIdx, out ListOrValue retVal) {
        //     nextIdx = -1;
        //     retVal = default;
        //     if (tokens == null)
        //         return;
        //     var tokensLen = tokens.Length;
        //     if (tokensLen < 1)
        //         return;
        //     // retVal.value = tokens[index];

        //     var curTok = tokens[index++];
        //     while (curTok.type != TokenType.list &&
        //         curTok.type != TokenType.quotedString &&
        //         curTok.type != TokenType.unquotedString
        //     ) {
        //         curTok = tokens[index++];
        //     }
        //     // retVal = new ListOrValue ();// todo comment

        //     if (curTok.type == TokenType.list) {
        //         retVal.list = new List<ListOrValue> ();
        //         nextIdx = index; // - 1;
        //         if (nextIdx >= tokensLen) {
        //             retVal.value = curTok;
        //             return;
        //         } else {
        //             // TokenPos nextTok;
        //             var nextTok = tokens[nextIdx]; // peek next token just to know if list ended
        //             while (nextTok.start < curTok.end && nextIdx < tokensLen) {
        //                 if (nextTok.type != TokenType.neutral) {
        //                     ListOrValue nxtLstItm;
        //                     parse (tokens, nextIdx, out nextIdx, out nxtLstItm);
        //                     retVal.list.Add (nxtLstItm);
        //                     if (nextIdx >= tokensLen)
        //                         break;
        //                 } else {
        //                     nextIdx++;
        //                 }

        //                 nextTok = tokens[nextIdx];
        //             }
        //         }

        //     } else {
        //         nextIdx = index;
        //     }

        //     retVal.value = curTok;
        // }

        public static ListOrValue tokenizeAndParse (string src) {
            var tokens = tokenize (src);
            var ret = parseRoot (tokens);
            return ret;
            // int nextIdx;
            // ListOrValue retval;
            // parse (tokens, 0, out nextIdx, out retval);
            // return retval;
        }
    }
}
