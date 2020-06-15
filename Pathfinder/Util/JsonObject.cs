using System;

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
            start = cur;
            end = src.Length;
            Match('{', ref cur);
            SkipNested(ref cur);
            Match('}', ref cur);
            end = cur;
        }

        private JsonObject() {}


        public JsonObject this[string fieldName]
        {
            get
            {
                var cur = start;

                var fieldObj = new JsonObject { src = src };

                SkipWhite(ref cur);
                if (src[cur] == '{')
                {
                    cur++;
                    while (cur < end)
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
                int cur = start;

                var fieldObj = new JsonObject { src = src };

                SkipWhite(ref cur);
                if (src[cur] == '[')
                {
                    cur++;
                    SkipWhite(ref cur);

                    while (index > 0)
                    {
                        SkipNested(ref cur);
                        if (src[cur] == ',')
                            cur++;
                        else if (src[cur] == ']')
                            return null;

                        index--;
                    }

                    if (src[cur] == ']')
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
            int cur = end;

            var fieldObj = new JsonObject { src = src };

            SkipWhite(ref cur);
            if (src[cur] != ',')
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
                return int.TryParse(src.Substring(start, end - start), out int res) ? (int?)res : null;
            }
        }

        public string AsString
        {
            get
            {
                if (src[start] != '\"' || src[end - 1] != '\"')
                    return "";

                return src.Substring(start + 1, end - start - 2);
            }
        }

        public bool? AsBool
        {
            get
            {
                return bool.TryParse(src.Substring(start, end - start), out bool res) ? (bool?)res : null;
            }
        }

        public bool CompareAsString(string str)
        {
            if (src[start] != '\"' || src[end - 1] != '\"')
                return false;

            int cur = start + 1;
            int strCur = 0;
            while (cur < end && src[cur] != '\"' && strCur < str.Length)
            {
                if (src[cur] != str[strCur])
                    return false;

                cur++;
                strCur++;
            }

            if (src[cur] != '\"' || strCur < str.Length)
                return false;

            return true;
        }

        public string GetObjectString() => src.Substring(start, end - start);

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

                if (src[cur] == '{')
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
            while (cur < end && src[cur] != '\"' && strCur < str.Length)
            {
                if (src[cur] != str[strCur])
                    return false;

                cur++;
                strCur++;
            }

            if (src[cur] != '\"' || strCur < str.Length)
                return false;

            return true;
        }
    }
}