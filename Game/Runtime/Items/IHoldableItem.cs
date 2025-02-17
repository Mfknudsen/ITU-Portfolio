#region Packages

using Runtime.Pokémon;

#endregion

namespace Runtime.Items
{
    public interface IHoldableItem
    {
        public bool IsUsableTarget(Pokemon pokemon);
    }
}