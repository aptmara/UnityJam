using System;
using System.Collections.Generic;

namespace UnityJam.Credits
{
    /// @brief クレジットテキストの解析。
    /// @author 山内陽
    public static class CreditTextParser
    {
        public static List<CreditSection> Parse(string rawText)
        {
            var sections = new List<CreditSection>();
            if (string.IsNullOrEmpty(rawText)) return sections;

            string[] lines = rawText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            CreditSection currentSection = null;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    string role = line.Substring(1, line.Length - 2);
                    currentSection = new CreditSection { RoleName = role };
                    sections.Add(currentSection);
                }
                else
                {
                    if (currentSection == null) {
                        currentSection = new CreditSection { RoleName = "Credits" };
                        sections.Add(currentSection);
                    }
                    string desc = line;
                    string titleName = "";
                    if (i + 1 < lines.Length) {
                        string next = lines[i+1].Trim();
                        if (!string.IsNullOrEmpty(next) && !next.StartsWith("[")) {
                            titleName = next;
                            i++;
                        }
                    }
                    var entry = new CreditEntry();
                    entry.Description = desc;
                    if (!string.IsNullOrEmpty(titleName)) {
                        int spaceIdx = titleName.IndexOf(' ');
                        if (spaceIdx > 0) {
                            entry.Title = titleName.Substring(0, spaceIdx);
                            entry.Name = titleName.Substring(spaceIdx + 1);
                        } else {
                            entry.Name = titleName;
                        }
                    }
                    currentSection.Entries.Add(entry);
                }
            }
            return sections;
        }
    }
}
