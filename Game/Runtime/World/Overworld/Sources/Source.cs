#region Packages

using Runtime.Pokémon;
using UnityEngine;

#endregion

namespace Runtime.World.Overworld.Sources
{
    public abstract class Source : MonoBehaviour
    {
        public abstract void Consume(Pokemon pokemon);
    }
}