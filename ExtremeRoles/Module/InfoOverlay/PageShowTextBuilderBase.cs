using ExtremeRoles.Module.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExtremeRoles.Module.InfoOverlay
{
    public abstract class PageShowTextBuilderBase : IShowTextBuilder
    {
        protected int Page = 0;
        protected List<(string, int)> AllPage = new List<(string, int)>();
        public PageShowTextBuilderBase()
        {
            Clear();
        }

        public void Clear()
        {
            AllPage.Clear();
            Page = 0;
        }

        public void ChangePage(int count)
        {
            if (AllPage.Count == 0) { return; }
            Page = (Page + count) % AllPage.Count;

            if (Page < 0)
            {
                Page = AllPage.Count + Page;
            }
        }

        public abstract (string, string, string) GetShowText();
    }
}
