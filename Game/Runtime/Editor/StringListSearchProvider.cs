#region Packages

using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

#endregion

namespace Runtime.Editor
{
    //https://www.youtube.com/watch?v=0HHeIUGsuW8&t=505s
    public class StringListSearchProvider : ScriptableObject, ISearchWindowProvider
    {
        #region Build In States

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry> {new SearchTreeGroupEntry(new GUIContent("List"), 0)};
            return searchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            return true;
        }

        #endregion
    }
}
