#region Packages

using System.Linq;
using Runtime.Battle.Systems.BattleStart;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

#endregion

namespace Runtime.Battle.Systems.Initializer
{
    public sealed class StaticBattleInitializer : BattleInitializer
    {
        #region Values

#if UNITY_EDITOR
        [SerializeField] private bool displayDebug;
#endif

        [SerializeField, BoxGroup, Required] private Transform center;

        [SerializeField, BoxGroup, Min(0)] private float radius;

        [SerializeField, BoxGroup("Pokemon Spots")]
        private Transform[] allyPokemonSpots;

        [SerializeField, BoxGroup("Pokemon Spots")]
        private Transform[] enemyPokemonSpots;

        [SerializeField, BoxGroup("Character Positions"), Required]
        private Transform manualPlayerPosition;

        [SerializeField, BoxGroup("Character Positions")]
        private Transform[] allyCharacterPositions;

        [SerializeField, BoxGroup("Character Positions")]
        private Transform[] enemyCharacterPositions;

#if UNITY_EDITOR
        [SerializeField, FoldoutGroup("Validate")]
        private LayerMask environmentLayer, propLayer;

        [SerializeField, FoldoutGroup("Validate")]
        private float characterCheckHeight, characterCheckWidth, pokemonCheckHeight, pokemonCheckWidth;
#endif

        #endregion

        #region Build In States

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (this.center == null) return;

            Vector3 centerPosition = this.center.position;

            if (this.manualPlayerPosition != null)
            {
                this.manualPlayerPosition.LookAt(
                    new Vector3(centerPosition.x,
                        this.manualPlayerPosition.position.y,
                        centerPosition.z));
            }

            foreach (Transform t in this.allyPokemonSpots
                         .Concat(this.enemyPokemonSpots)
                         .Concat(this.allyCharacterPositions)
                         .Concat(this.enemyCharacterPositions))
            {
                t.LookAt(new Vector3(centerPosition.x, t.position.y, centerPosition.z));
            }
        }

        private void OnDrawGizmos()
        {
            if (!this.displayDebug || this.center == null || this.radius == 0) return;

            BattleStarter battleStarter = this.GetComponentInParent<BattleStarter>();
            Color allowedColor = Color.green;

            if (battleStarter != null)
            {
                if (battleStarter.GetAllies().Length > this.allyCharacterPositions.Length ||
                    battleStarter.GetEnemies().Length > this.enemyCharacterPositions.Length)
                    allowedColor = Color.red;
            }

            Handles.color = Color.yellow;
            Vector3 pos = this.transform.position;
            Handles.DrawWireDisc(pos, Vector3.up, .1f);
            Handles.DrawWireDisc(pos, Vector3.up, .25f);
            Handles.color = Color.white;

            foreach (Transform t in this.allyPokemonSpots
                         .Concat(this.enemyPokemonSpots))
            {
                Handles.color = Color.white;
                if (Vector3.Distance(t.position, this.center.position) > this.radius)
                {
                    Handles.color = Color.red;
                    allowedColor = Color.red;
                }

                Handles.DrawWireDisc(t.position, Vector3.up, this.pokemonCheckWidth);
            }

            foreach (Transform t in this.allyCharacterPositions
                         .Concat(this.enemyCharacterPositions))
            {
                Handles.color = Color.white;
                if (Vector3.Distance(t.position, this.center.position) > this.radius)
                {
                    Handles.color = Color.red;
                    allowedColor = Color.red;
                }

                Vector3 position = t.position;
                Handles.DrawWireDisc(position, Vector3.up, this.characterCheckWidth / 2f);
                Handles.DrawWireDisc(position, Vector3.up, this.characterCheckWidth);
            }

            if (this.manualPlayerPosition == null ||
                Vector3.Distance(this.center.position, this.manualPlayerPosition.position) > this.radius)
                allowedColor = Color.red;

            if (this.manualPlayerPosition != null)
            {
                Vector3 position = this.manualPlayerPosition.position;
                Handles.color = (Color.yellow + Color.red) / 2;
                Handles.DrawWireDisc(position, Vector3.up, this.characterCheckWidth / 2f);
                Handles.color = Color.white;
                Handles.DrawWireDisc(position, Vector3.up, this.characterCheckWidth);
            }

            Handles.color = allowedColor;
            Handles.DrawWireDisc(
                this.center.position,
                Vector3.up,
                this.radius);
        }
#endif

        #endregion

        #region Getters

        public override Transform GetPlayerCharacterPosition() => this.manualPlayerPosition;

        public override Transform[] GetAllySpots() => this.allyPokemonSpots;

        public override Transform[] GetEnemySpots() => this.enemyPokemonSpots;

        public override Transform[] GetAllyCharacterPositions() => this.allyCharacterPositions;

        public override Transform[] GetEnemyCharacterPositions() => this.enemyCharacterPositions;

        #endregion

        public override void FindSetupBattleZone() =>
            this.found = true;

        #region Internal

#if UNITY_EDITOR
        [FoldoutGroup("Validate"), Button]
        private void ValidateArea()
        {
            foreach (Transform t in this.allyCharacterPositions.Concat(this.enemyCharacterPositions))
            {
                for (float i = 4; i > -4; i -= .5f)
                {
                    if (!Physics.Raycast(t.position + Vector3.up * i, -Vector3.up, out RaycastHit hit, .5f,
                            this.environmentLayer, QueryTriggerInteraction.Ignore)) continue;

                    if (!Physics.CheckCapsule(hit.point, hit.point + Vector3.up * this.characterCheckHeight,
                            this.characterCheckWidth, this.propLayer, QueryTriggerInteraction.Ignore)) continue;

                    Debug.Log("Invalid");
                    return;
                }
            }

            foreach (Transform t in this.allyPokemonSpots.Concat(this.enemyPokemonSpots))
            {
                for (float i = 4; i > -4; i -= .5f)
                {
                    if (!Physics.Raycast(t.position + Vector3.up * i, -Vector3.up, out RaycastHit hit, .5f,
                            this.environmentLayer, QueryTriggerInteraction.Ignore)) continue;

                    if (!Physics.CheckCapsule(hit.point, hit.point + Vector3.up * this.pokemonCheckHeight,
                            this.pokemonCheckWidth, this.propLayer, QueryTriggerInteraction.Ignore)) continue;

                    Debug.Log("Invalid");
                    return;
                }
            }

            Debug.Log("Valid");
        }

        [MenuItem("Tools/Mfknudsen/Create New Static Battle Initializer")]
        private static void SetupNewStaticInitializer()
        {
            GameObject obj = new GameObject("Static Battle Initializer");
            BattleInitializer battleInitializer = obj.AddComponent<StaticBattleInitializer>();

            if (Selection.activeGameObject is { } activeObject)
            {
                GameObjectUtility.SetParentAndAlign(obj, activeObject);

                if (activeObject.GetComponent<BattleStarter>() is { } battleStarter)
                    battleStarter.SetInitializer(battleInitializer);
            }

            Undo.RegisterCreatedObjectUndo(obj, "Created: " + obj.name);
            Selection.activeObject = obj;
        }
#endif

        #endregion
    }
}