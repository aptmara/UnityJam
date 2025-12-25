using System.Collections.Generic;

namespace UnityJam.Credits
{
    /// @brief クレジット表示の1項目。
    /// @author 山内陽
    [System.Serializable]
    public class CreditEntry
    {
        public string Description;
        public string Title;
        public string Name;

        public CreditEntry() {}

        public CreditEntry(string title, string name)
        {
            Title = title;
            Name = name;
        }
    }

    /// @brief クレジットのセクション。
    /// @author 山内陽
    [System.Serializable]
    public class CreditSection
    {
        public string RoleName;
        public List<CreditEntry> Entries = new List<CreditEntry>();

        public CreditSection() {}

        public CreditSection(string roleName)
        {
            RoleName = roleName;
        }
    }
}
