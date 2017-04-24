using System;
using System.Collections.Generic;

namespace Pathfinder.Util
{
    public class JsonObject
    {
        public string src;
        public int start;
        public int end;

        public JsonObject(string src)
        {
            int cur = 0;
            this.src = src;
            this.start = cur;
            this.end = src.Length;
            Match('{', ref cur);
            SkipNested(ref cur);
            Match('}', ref cur);
            this.end = cur;
        }

        private JsonObject() {}


        public JsonObject this[string fieldName]
        {
            get
            {
                var cur = this.start;

                var fieldObj = new JsonObject();
                fieldObj.src = this.src;

                SkipWhite(ref cur);
                if (this.src[cur] == '{')
                {
                    cur++;
                    while (cur < this.end)
                    {
                        Match('\"', ref cur);
                        var isMatch = Compare(cur, fieldName);

                        SkipMatchUpTo('\"', ref cur);
                        Match(':', ref cur);

                        SkipWhite(ref cur);

                        fieldObj.start = cur;
                        SkipNested(ref cur);
                        fieldObj.end = BackupWhite(cur);

                        cur++;

                        if (isMatch)
                            return fieldObj;
                    }
                }

                return null;
            }
        }


        public JsonObject this[int index]
        {
            get
            {
                int cur = this.start;

                var fieldObj = new JsonObject();
                fieldObj.src = this.src;

                SkipWhite(ref cur);
                if (this.src[cur] == '[')
                {
                    cur++;
                    SkipWhite(ref cur);

                    while (index > 0)
                    {
                        SkipNested(ref cur);
                        if (this.src[cur] == ',')
                            cur++;
                        else if (this.src[cur] == ']')
                            return null;

                        index--;
                    }

                    if (this.src[cur] == ']')
                        return null;

                    SkipWhite(ref cur);
                    fieldObj.start = cur;
                    SkipNested(ref cur);
                    fieldObj.end = BackupWhite(cur);

                    return fieldObj;
                }

                return null;
            }
        }

        public JsonObject Next()
        {
            int cur = this.end;

            var fieldObj = new JsonObject();
            fieldObj.src = this.src;

            SkipWhite(ref cur);
            if (this.src[cur] != ',')
                return null;

            cur++;
            SkipWhite(ref cur);
            fieldObj.start = cur;
            SkipNested(ref cur);
            fieldObj.end = BackupWhite(cur);

            return fieldObj;
        }

        public int? AsInt
        {
            get
            {
                int res;
                return Int32.TryParse(src.Substring(start, end - start), out res) ? (int?)res : null;
            }
        }

        public string AsString
        {
            get
            {
                if (this.src[this.start] != '\"' || this.src[this.end - 1] != '\"')
                    return "";

                return this.src.Substring(this.start + 1, this.end - this.start - 2);
            }
        }

        public bool? AsBool
        {
            get
            {
                bool res;
                return Boolean.TryParse(src.Substring(start, end - start), out res) ? (bool?)res : null;
            }
        }

        public bool CompareAsString(string str)
        {
            if (this.src[this.start] != '\"' || this.src[this.end - 1] != '\"')
                return false;

            int cur = this.start + 1;
            int strCur = 0;
            while (cur < this.end && this.src[cur] != '\"' && strCur < str.Length)
            {
                if (this.src[cur] != str[strCur])
                    return false;

                cur++;
                strCur++;
            }

            if (this.src[cur] != '\"' || strCur < str.Length)
                return false;

            return true;
        }

        public string GetObjectString()
        {
            return this.src.Substring(this.start, this.end - this.start);
        }

        private void Match(char c, ref int cur)
        {
            while (cur < end && src[cur] <= ' ')
                cur++;

            if (src[cur] != c)
                throw new Exception("expected '" + c + "'");

            cur++;
        }

        private void SkipMatchUpTo(char c, ref int cur)
        {
            while (cur < end && src[cur] != c)
                cur++;

            if (cur >= end)
                throw new Exception("unexpected end");

            if (src[cur] != c)
                throw new Exception("expected '" + c + "'");

            cur++;
        }

        private void SkipWhite(ref int cur)
        {
            while (cur < src.Length && src[cur] <= ' ')
                cur++;
        }

        private int BackupWhite(int cur)
        {
            while (cur > 0 && src[cur - 1] <= ' ')
                cur--;

            return cur;
        }

        private void SkipNested(ref int cur)
        {
            int braceNesting = 0;
            int bracketNesting = 0;
            while (cur < src.Length)
            {
                if (cur >= src.Length)
                    throw new Exception("unexpected end");

                else if (src[cur] == '{')
                    braceNesting++;
                else if (src[cur] == '}')
                {
                    if (braceNesting == 0 && bracketNesting == 0)
                        break;
                    braceNesting--;
                }
                else if (src[cur] == '[')
                    bracketNesting++;
                else if (src[cur] == ']')
                {
                    if (braceNesting == 0 && bracketNesting == 0)
                        break;
                    bracketNesting--;
                }
                else if (src[cur] == ',' && braceNesting == 0 && bracketNesting == 0)
                    break;

                cur++;
            }
        }

        private bool Compare(int cur, string str)
        {
            int strCur = 0;
            while (cur < this.end && this.src[cur] != '\"' && strCur < str.Length)
            {
                if (this.src[cur] != str[strCur])
                    return false;

                cur++;
                strCur++;
            }

            if (this.src[cur] != '\"' || strCur < str.Length)
                return false;

            return true;
        }
    }
}