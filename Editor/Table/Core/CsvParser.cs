using System.Collections.Generic;
using System.Text;

namespace AchEngine.Editor.Table
{
    public static class CsvParser
    {
        public static List<string[]> Parse(string csvText)
        {
            var rows = new List<string[]>();
            var fields = new List<string>();
            var field = new StringBuilder();
            bool inQuotes = false;
            int i = 0;

            while (i < csvText.Length)
            {
                char c = csvText[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < csvText.Length && csvText[i + 1] == '"')
                        {
                            field.Append('"');
                            i += 2;
                        }
                        else
                        {
                            inQuotes = false;
                            i++;
                        }
                    }
                    else
                    {
                        field.Append(c);
                        i++;
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        inQuotes = true;
                        i++;
                    }
                    else if (c == ',')
                    {
                        fields.Add(field.ToString());
                        field.Clear();
                        i++;
                    }
                    else if (c == '\r' || c == '\n')
                    {
                        fields.Add(field.ToString());
                        field.Clear();

                        if (fields.Count > 0 && !(fields.Count == 1 && string.IsNullOrEmpty(fields[0])))
                            rows.Add(fields.ToArray());
                        fields.Clear();

                        if (c == '\r' && i + 1 < csvText.Length && csvText[i + 1] == '\n')
                            i += 2;
                        else
                            i++;
                    }
                    else
                    {
                        field.Append(c);
                        i++;
                    }
                }
            }

            fields.Add(field.ToString());
            if (fields.Count > 0 && !(fields.Count == 1 && string.IsNullOrEmpty(fields[0])))
                rows.Add(fields.ToArray());

            return rows;
        }
    }
}
