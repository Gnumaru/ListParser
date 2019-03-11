using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
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
        private string m_source;
        private TokenType m_type;
        private int m_start; // inclusive. first index belonging to token
        private int m_end; // exclusive. first index not belonging to token
        private string m_extracted;

        public TokenPos(string source, TokenType type, int start, int end) {
            m_source = source;
            m_type = type;
            m_start = start;
            m_end = end;
            m_extracted = null;
        }

        public TokenPos(TokenPos copyFrom, int end) {
            this.m_source = copyFrom.m_source;
            this.m_type = copyFrom.m_type;
            this.m_start = copyFrom.m_start;
            this.m_extracted = copyFrom.m_extracted;
            m_end = end;
        }

        public string getSource() {
            return m_source;
        }

        public TokenType getType() {
            return m_type;
        }

        public int getStart() {
            return m_start;
        }

        public int getEnd() {
            return m_end;
        }

        public int sectionLength() {
            return m_end - m_start;
        }

        public int sourceLength() {
            return m_source.Length;
        }

        public bool isDefault() {
            return m_source == null &&
                m_type == 0 &&
                m_start == 0 &&
                m_end == 0;
        }

        public string extract() {
            if (m_extracted != null)
                return m_extracted;

            var _start = m_start;
            var _end = m_end;

            if (_start >= _end)
                return "";

            m_extracted = m_source.Substring(_start, _end - _start);
            return m_extracted;
        }

        public ParsedString parse() {
            var str = this.extract();

            ulong ul;
            if (ulong.TryParse(str, out ul)) {
                return new ParsedString(ul);
            }

            long l;
            if (long.TryParse(str, out l)) {
                return new ParsedString(l);
            }

            double d;
            if (double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out d)) {
                return new ParsedString(d);
            }

            bool b;
            if (bool.TryParse(str, out b)) {
                return new ParsedString(b);
            }

            return new ParsedString(str);
        }
    }

    public struct ListOrValue {
        private List<ListOrValue> m_list;
        private TokenPos m_value;

        public ListOrValue(List<ListOrValue> list, TokenPos token) {
            m_list = list;
            m_value = token;
        }

        public int getTokenStart() {
            return m_value.getStart();
        }

        public int getTokenEnd() {
            return m_value.getEnd();
        }

        public string extract() {
            return m_value.extract();
        }

        public void add(ListOrValue listOrValue) {
            m_list.Add(listOrValue);
        }

        public int tokenSectionLength() {
            return m_value.sectionLength();
        }

        public int tokenSourceLength() {
            return m_value.sourceLength();
        }

        public bool isList() {
            return m_list != null;
        }

        public int count() {
            if (m_list == null)
                return -1;
            return m_list.Count;
        }

        public bool isDefault() {
            return m_list == null && m_value.isDefault();
        }

        public ListOrValue this[int i] {
            get { return getByIdx(i); }
        }

        public ListOrValue this[string key] {
            get { return getValueByKeyName(key); }
        }

        public ListOrValue getByIdx(int idx) {
            return m_list[idx];
        }

        public ListOrValue getValueByKeyName(string keyName) {
            var val = new ListOrValue();
            if (!isList())
                return val; // should be list to find a value by key name
            var len = m_list.Count;
            if (len % 2 != 0)
                return val; // an 'object' list shoul have a pair number of items
            for (var i = 0; i < len; i += 2) {
                var key = m_list[i];
                if (key.isList())
                    return new ListOrValue(); // if a given
                if (!val.isDefault())
                    continue; // even after finding the value should continue looking if the list is indeed a valid object list
                if (key.m_value.extract() == keyName)
                    val = m_list[i + 1];
            }
            return val; // the found item or default if none was found
        }

        public string toJson() {
            if (m_list == null) {
                var extracted = m_value.extract();
                if (extracted.StartsWith("\""))
                    return extracted;
                return $"\"{extracted}\"";
            }
            var len = m_list.Count;
            var sb = new StringBuilder();
            sb.Append("[");
            for (var i = 0; i < len; i++) {
                sb.Append(m_list[i].toJson());
                sb.Append(",");
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append("]");
            return sb.ToString();
        }

        public string toLisp() {
            return toLisp(false, 0, true, 1);
        }

        public string toLisp(bool prettyPrint) {
            return toLisp(prettyPrint, 0, true, 1);
        }

        public string toLisp(bool prettyPrint, int level, bool newline, int ident) {
            if (!prettyPrint)
                return m_value.getSource();
            if (ident < 1)
                ident = 1;

            if (m_list == null) {
                var extracted = m_value.extract();
                var identchars = level * ident;
                if (identchars < 1) {
                    identchars = ident - 1;
                }
                return $"{new string(' ', identchars)}{extracted}{(newline ? "\n" : "")}";
            }
            // else, it is a list
            var len = m_list.Count;
            var sb = new StringBuilder(64); // initial capacity of 64 chars
            sb.Append(new string(' ', level * ident));
            if (m_value.getType() != TokenType.document)
                sb.Append("(");
            level++;
            for (var i = 0; i < len; i++) {
                sb.Append(m_list[i].toLisp(true, i > 0 ? level : 0, i < len - 1, ident));
            }
            // sb.Append (new string (' ', level - 1));
            if (m_value.getType() != TokenType.document)
                sb.Append(")\n");
            return sb.ToString();
        }
    }

    public struct ParsedString {
        private ulong m_ulong;
        private long m_long; // and any other integer
        private double m_double; // and float
        private bool m_bool; // could use int for this, but leaving just for convenience
        private string m_string; // and also char
        private ParsedStringType m_type;

        public ParsedStringType getType() {
            return m_type;
        }

        public ulong getUlong() {
            return m_ulong;
        }

        public long getLong() {
            return m_long;
        }

        public double getDouble() {
            return m_double;
        }

        public bool getBool() {
            return m_bool;
        }

        public string getString() {
            return m_string;
        }

        public ParsedString(ulong val) {
            m_type = ParsedStringType._ulong;
            m_ulong = val;
            m_long = 0;
            m_double = 0;
            m_bool = false;
            m_string = null;
        }

        public ParsedString(long val) {
            m_type = ParsedStringType._long;
            m_ulong = 0;
            m_long = val;
            m_double = 0;
            m_bool = false;
            m_string = null;
        }

        public ParsedString(double val) {
            m_type = ParsedStringType._double;
            m_ulong = 0;
            m_long = 0;
            m_double = val;
            m_bool = false;
            m_string = null;
        }

        public ParsedString(bool val) {
            m_type = ParsedStringType._double;
            m_ulong = 0;
            m_long = 0;
            m_double = 0;
            m_bool = val;
            m_string = null;
        }

        public ParsedString(string val) {
            m_type = ParsedStringType._double;
            m_ulong = 0;
            m_long = 0;
            m_double = 0;
            m_bool = false;
            m_string = val;
        }

        public bool isDefault() {
            return m_type == 0
                && m_ulong == 0
                && m_long == 0
                && m_double == 0
                && m_bool == false
                && m_string == null
            ;
        }
    }

    public static class LispParser {

        // since lists can be nested, liststar and listend must be different chars
        public const char listStart = '('; // cannot be same of list end
        public const char listEnd = ')'; // cannot be same of list start
        public const char stringStart = '"';
        public const char stringEnd = '"';
        public const char escape = '\\'; // can be used only inside quoted strings

        public static TokenPos[] tokenize(string src) {
            var count = tokenize(src, null);
            if (count < 0)
                return null;
            var tokens = new TokenPos[count];
            count = tokenize(src, tokens);
            return tokens;
        }

        public static int tokenize(string self, TokenPos[] tokens) {
            if (self == null || self == "")
                return -1;
            var curState = ParseState.insideBody;
            var lastToken = TokenType.none;
            var strLen = self.Length;
            var listStartCount = 0;
            var listEndCount = 0;
            var tokenIdx = 0;
            var nextIsScaped = false;
            Stack<int> listStartStack = null;
            var tokensLen = 0;
            if (tokens != null) {
                tokensLen = tokens.Length;
                listStartStack = new Stack<int>(tokensLen); // TODO think of an optimized way without heap allocation
            }

            for (var i = 0; i < strLen; i++) {
                var cur = self[i];

                switch (curState) {
                    case ParseState.insideBody:
                        switch (cur) {
                            case listStart:

                                listStartCount++;
                                if (tokens != null) {
                                    listStartStack.Push(tokenIdx);
                                    tokens[tokenIdx] = new TokenPos(self, TokenType.list, i, 0);
                                    //tokens[tokenIdx] = new TokenPos() { m_source = self, m_type = TokenType.list, m_start = i };
                                }
                                tokenIdx++;
                                lastToken = TokenType.list;
                                break;

                            case listEnd:
                                listEndCount++;
                                if (tokens != null) {
                                    var idx = listStartStack.Pop();
                                    tokens[idx] = new TokenPos(tokens[idx], i + 1);
                                    //tokens[idx].m_end = i + 1;
                                }
                                lastToken = TokenType.list;
                                break;

                            case stringStart:

                                curState = ParseState.insideQuotedString;
                                if (tokens != null) {
                                    tokens[tokenIdx] = new TokenPos(self, TokenType.quotedString, i, 0);
                                    //tokens[tokenIdx] = new TokenPos() { m_source = self, m_type = TokenType.quotedString, m_start = i };
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
                                        tokens[tokenIdx - 1] = new TokenPos(tokens[tokenIdx - 1], i);
                                        //tokens[tokenIdx - 1].m_end = i;
                                    }
                                    if (tokens != null) {
                                        tokens[tokenIdx] = new TokenPos(self, TokenType.neutral, i, 0);
                                        //tokens[tokenIdx] = new TokenPos() { m_source = self, m_type = TokenType.neutral, m_start = i };
                                    }
                                    tokenIdx++;
                                    lastToken = TokenType.neutral;
                                    break;
                                }
                                // else, it is starting a identifier
                                if (tokens != null && lastToken == TokenType.neutral) { // close last neutral if any
                                    tokens[tokenIdx - 1] = new TokenPos(tokens[tokenIdx - 1], i);
                                    //tokens[tokenIdx - 1].m_end = i;
                                }

                                if (tokens != null) {
                                    tokens[tokenIdx] = new TokenPos(self, TokenType.unquotedString, i, 0);
                                    //tokens[tokenIdx] = new TokenPos() { m_source = self, m_type = TokenType.unquotedString, m_start = i };
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
                                    tokens[tokenIdx - 1] = new TokenPos(tokens[tokenIdx - 1], i + 1);
                                    //tokens[tokenIdx - 1].m_end = i + 1;
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
                                    tokens[tokenIdx - 1] = new TokenPos(tokens[tokenIdx - 1], i);
                                    //tokens[tokenIdx - 1].m_end = i;
                                }
                                tokenIdx++;
                                i--; // must reevaluate character
                                curState = ParseState.insideBody;
                                break;

                            case listEnd:
                                if (tokens != null) { // close last unquoted string
                                    tokens[tokenIdx - 1] = new TokenPos(tokens[tokenIdx - 1], i);
                                    //tokens[tokenIdx - 1].m_end = i;
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
                                    tokens[tokenIdx - 1] = new TokenPos(tokens[tokenIdx - 1], i);
                                    //tokens[tokenIdx - 1].m_end = i;
                                }

                                if (tokens != null) {
                                    tokens[tokenIdx] = new TokenPos(self, TokenType.neutral, i, 0);
                                    //tokens[tokenIdx] = new TokenPos() { m_source = self, m_type = TokenType.neutral, m_start = i };
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
                            tokens[tokenIdx - 1] = new TokenPos(tokens[tokenIdx - 1], i);
                            //tokens[tokenIdx - 1].m_end = i;
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

            if (tokens != null && tokensLen > 0 && tokens[tokensLen - 1].getEnd() < 1) { // close last token
                tokens[tokensLen - 1] = new TokenPos(tokens[tokensLen - 1], strLen);
                //tokens[tokensLen - 1].m_end = strLen;
            }

            return tokenIdx;
        }

        public static ListOrValue parseRoot(TokenPos[] tokens) {
            //ListOrValue retVal = new ListOrValue();
            if (tokens == null || tokens.Length < 1)
                return new ListOrValue();

            var tok = new TokenPos(
                tokens[0].getSource(),
                TokenType.document,
                0,
                tokens[0].sourceLength()
            //retVal.tokenSectionLength()
            //retVal.m_value.m_source.Length
            );

            ListOrValue retVal = new ListOrValue(new List<ListOrValue>(), tok);
            //retVal.m_value.m_type = TokenType.document;
            //retVal.m_value.m_start = 0;
            //retVal.m_value.m_source = tokens[0].m_source;
            //retVal.m_value.m_end = retVal.m_value.m_source.Length;
            //retVal.m_list = new List<ListOrValue>();

            var tokensLength = tokens.Length;
            int idx = 0;

            while (idx < tokensLength) {
                var item = parseToken(tokens, idx, out idx);
                if (item.isDefault())
                    continue;
                retVal.add(item);
            }
            if (retVal.count() < 1)
                return new ListOrValue();

            return retVal;
        }

        public static ListOrValue parseToken(TokenPos[] tokens, int curIdx, out int nextIdx) {
            nextIdx = curIdx + 1;
            //ListOrValue curListOrVal = new ListOrValue();
            var curTok = tokens[curIdx];
            var tokensLength = tokens.Length;

            if (curTok.getType() == TokenType.neutral)
                return new ListOrValue();

            //ListOrValue curListOrVal = new ListOrValue(null, curTok);
            //curListOrVal.m_value = curTok;
            if (curTok.getType() != TokenType.list)
                return new ListOrValue(null, curTok);

            // is list
            ListOrValue curListOrVal = new ListOrValue(new List<ListOrValue>(), curTok);
            //curListOrVal.m_list = new List<ListOrValue>();
            while (nextIdx < tokensLength) {
                if (tokens[nextIdx].getStart() >= curTok.getEnd())
                    break;
                var item = parseToken(tokens, nextIdx, out nextIdx);
                if (item.isDefault())
                    continue;
                curListOrVal.add(item);
            }
            return curListOrVal;
        }

        public static ListOrValue fromLisp(string src) {
            var tokens = tokenize(src);
            var ret = parseRoot(tokens);
            return ret;
        }

        public static StringBuilder toLisp(object obj) {
            if (object.ReferenceEquals(obj, null))
                return null;
            var refs = new Dictionary<object, int>();
            var sb = new StringBuilder();
            var fieldsDict = new Dictionary<Type, List<FieldInfo>>();
            toLisp(obj, refs, sb, fieldsDict);
            return sb;
        }

        public static void toLisp(object obj, Dictionary<object, int> references, StringBuilder sb, Dictionary<Type, List<FieldInfo>> fieldsDict) {
            if (object.ReferenceEquals(obj, null)) {
                sb.Append("nil");
                return;
            }
            var type = obj.GetType();
            if (!type.IsValueType) {
                if (references.ContainsKey(obj)) {
                    sb.Append("objGraphCycleNotSupported");
                    return;
                }
                references.Add(obj, 0);
            }

            if (isPrimitive(type)) {
                sb.Append(obj.ToString());
                sb.Append(' ');
            } else if (type.IsArray) {
                Array arr = (Array)obj;
                sb.Append('(');
                sb.Append('(');
                var rank = arr.Rank;
                for (var i = 0; i < rank; i++) {
                    sb.Append(arr.GetLength(i));
                    sb.Append(' ');
                }
                sb.Append(')');
                foreach (var item in arr) {
                    toLisp(item, references, sb, fieldsDict);
                }
                sb.Append(')');
            } else {
                List<FieldInfo> fieldsList;
                if (fieldsDict.ContainsKey(obj.GetType()))
                    fieldsList = fieldsDict[type];
                else {
                    fieldsList = getFields(type);
                    fieldsDict.Add(type, fieldsList);
                }
                sb.Append('(');
                foreach (var field in fieldsList) {
                    sb.Append(field.Name);
                    sb.Append(' ');
                    toLisp(field.GetValue(obj), references, sb, fieldsDict);
                }
                sb.Append(')');
            }
        }

        public static string primitiveToString(object obj) {
            if (obj is float)
                return ((float)obj).ToString(CultureInfo.InvariantCulture);

            else if (obj is double)
                return ((double)obj).ToString(CultureInfo.InvariantCulture);

            else if (obj is Type) {
                var t = (Type)obj;
                return $"{t.Assembly.GetName().Name}.{t.ToString()}";
            }

            return obj.ToString();
        }

        public static List<FieldInfo> getFields(Type type) {
            var fields = new List<FieldInfo>();
            if (isPrimitive(type))
                return fields;
            var baseType = type;
            while (baseType != typeof(object) && baseType != typeof(ValueType)) {
                fields.AddRange(
                    baseType.GetFields(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic)
                );
                baseType = baseType.BaseType;
            }
            return fields;
        }

        public static bool isPrimitive(Type type) {
            return type == typeof(byte) ||
                type == typeof(sbyte) ||
                type == typeof(short) ||
                type == typeof(ushort) ||
                type == typeof(int) ||
                type == typeof(uint) ||
                type == typeof(long) ||
                type == typeof(ulong) ||
                type == typeof(float) ||
                type == typeof(double) ||
                type == typeof(char) ||
                type == typeof(bool) ||
                type == typeof(string) ||
                type == typeof(Type);
        }
    }
}
