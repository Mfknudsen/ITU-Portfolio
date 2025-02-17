#region SDK

using System;
using UnityEngine;

#endregion

namespace Runtime.Pokémon
{
    public enum TypeName
    {
        Normal,
        Fire,
        Water,
        Electric,
        Grass,
        Ice,
        Fighting,
        Poison,
        Ground,
        Flying,
        Psychic,
        Bug,
        Rock,
        Ghost,
        Dragon,
        Dark,
        Steel,
        Fairy
    }

    [CreateAssetMenu(fileName = "Type", menuName = "Pokemon/Create new Type", order = 4)]
    public class Type : ScriptableObject
    {
        #region Values

        [SerializeField] private TypeName typeName = 0;
        [SerializeField] private Color typeColor = Color.white;
        [SerializeField] private Type[] noEffect = Array.Empty<Type>();
        [SerializeField] private Type[] resistances = Array.Empty<Type>();
        [SerializeField] private Type[] weakness = Array.Empty<Type>();

        #endregion

        #region Getters

        public TypeName GetTypeName()
        {
            return this.typeName;
        }

        public Color GetTypeColor()
        {
            return this.typeColor;
        }

        public bool GetNoEffect(TypeName input)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (Type t in this.noEffect)
            {
                if (t.GetTypeName() == input)
                    return true;
            }

            return false;
        }

        public int GetResistance(TypeName input)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (Type t in this.resistances)
            {
                if (t.typeName == input)
                    return 1;
            }

            return 0;
        }

        public int GetWeakness(TypeName input)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (Type t in this.weakness)
            {
                if (t.typeName == input)
                    return 1;
            }

            return 0;
        }

        #endregion
    }
}