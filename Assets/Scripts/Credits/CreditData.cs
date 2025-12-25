using System.Collections.Generic;

namespace UnityJam.Credits
{
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
