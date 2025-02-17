#region Libraries

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#endregion

namespace Runtime.World.Overworld.Lights
{
    public sealed class DayTimeMapInformation : ScriptableObject
    {
        #region Values

        private int totalMapCount;

        private List<Texture2D> maps;

        #endregion

        #region Getters

        public int GetTotalMapCount() => this.totalMapCount;

        public List<Texture2D> GetTextureMaps() => this.maps;

        #endregion

        #region Setters

#if UNITY_EDITOR
        public void SetValues()
        {
            string assetPath = AssetDatabase.GetAssetPath(this);
            this.maps = AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Texture2D>().ToList();
        }
#endif

        #endregion
    }
}