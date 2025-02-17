#region Libraries

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Runtime.Battle.Systems.BattleStart;
using Runtime.Core;
using Runtime.World.Overworld;
using UnityEngine;

#endregion

namespace Runtime.Algorithms.BattleSetup
{
    public static class SetupBattleWorldLayout
    {
        #region Values

        private const int ALLOWED_OBSTRUCTED_PERCENTAGE_VALUE = 20;

        private const float DISTANCE_BETWEEN_CHECKS = 3,
            RAY_CHECK_DISTANCE = 10,
            SPOT_CHECK_RADIUS = 2.5f,
            SPOT_CHECK_HEIGHT = 7.5f;

        private static readonly Vector2[] CheckPattern =
        {
            // ----0----
            new Vector2(0, 1),
            // --0-0-0--
            new Vector2(0, 2), new Vector2(1, 2), new Vector2(-1, 2),
            // -O-O-O-O-
            new Vector2(0.5f, 3), new Vector2(-0.5f, 3), new Vector2(1.5f, 3), new Vector2(-1.5f, 3),
            // 0-0-0-0-0
            new Vector2(0, 4), new Vector2(1, 4), new Vector2(-1, 4), new Vector2(2, 4), new Vector2(-2, 4)
        };

        private static Vector2 centerBoxDimension = new Vector2(6, 3),
            secondaryBoxDimension = new Vector2(1, 1.5f);

        private static readonly LayerMask GroundHitLayer = LayerMask.GetMask($"Ground");

        private static bool _getInProgress,
            _tileInfoUpdateRequested;

        private static TileOptimizedInformation _currentTileInformation,
            _toBeSetTileInformation;

        #endregion

        #region Setters

        /// <summary>
        ///     Set the information to be checked against.
        ///     Will instantly set if "GetFreeformBattleLayout" hasn't been called and are not done.
        /// </summary>
        /// <param name="set">New information to be set</param>
        public static void SetOptimizedWorldInformation(TileOptimizedInformation set)
        {
            if (_getInProgress)
            {
                _toBeSetTileInformation = set;
                _tileInfoUpdateRequested = true;
                return;
            }

            _currentTileInformation = set;
        }

        #endregion

        #region Out

        /// <summary>
        ///     Get the best battle layout.
        /// </summary>
        /// <param name="battleStarter">Battlestarter calling the function</param>
        /// <param name="centerStartPosition">Position between the battlestarter and the player</param>
        /// <param name="playerPosition">Players position</param>
        /// <param name="playerAlliedSpotCount">Number of pokemon spots for the player and player allies</param>
        /// <param name="enemySpotCount">Number of pokemon spots for the enemies</param>
        /// <returns>Async UniTask containing a battle layout</returns>
        public static async UniTask<BattleLayout> GetFreeformBattleLayout(BattleStarter battleStarter,
            Vector3 centerStartPosition, Vector3 playerPosition,
            int playerAlliedSpotCount,
            int enemySpotCount)
        {
            _getInProgress = true;

            GetDirectionalSpotCheck(playerAlliedSpotCount, enemySpotCount,
                out Vector2[] alliedDirectionChecks, out Vector2[] enemyDirectionChecks);

            float lowestValue = -1;
            BattleLayout lowestLayout = new BattleLayout(false);
            if (CheckSpot(out BattleLayout battleLayout, out float obstructedValue,
                    centerStartPosition, (playerPosition - centerStartPosition).XZ().normalized,
                    alliedDirectionChecks, enemyDirectionChecks))
            {
                if (obstructedValue < ALLOWED_OBSTRUCTED_PERCENTAGE_VALUE)
                    return battleLayout;

                lowestValue = obstructedValue;
                lowestLayout = battleLayout;
            }

            DirectionsFromOriginalCenter(centerStartPosition, playerPosition, out Vector2[] forwardDirections,
                out Vector2[] rightDirections);

            int[] indexCheckPerDirection = new int[8];

            while (indexCheckPerDirection[0] < CheckPattern.Length)
            {
                for (int directionIndex = 0; directionIndex < forwardDirections.Length; directionIndex++)
                {
                    Vector2 forward = forwardDirections[directionIndex],
                        right = rightDirections[directionIndex];

                    Vector2 offset = CheckPattern[indexCheckPerDirection[directionIndex]];

                    Vector3 newCenter = centerStartPosition +
                                        new Vector3(forward.x, 0, forward.y) * offset.y * DISTANCE_BETWEEN_CHECKS +
                                        new Vector3(right.x, 0, right.y) * offset.x * DISTANCE_BETWEEN_CHECKS;

                    if (CheckSpot(out battleLayout, out obstructedValue, newCenter, forward,
                            alliedDirectionChecks, enemyDirectionChecks))
                    {
                        if (obstructedValue < ALLOWED_OBSTRUCTED_PERCENTAGE_VALUE)
                            return battleLayout;

                        if (obstructedValue < lowestValue)
                        {
                            lowestLayout = battleLayout;
                            lowestValue = obstructedValue;
                        }
                    }

                    indexCheckPerDirection[directionIndex]++;
                }

                await UniTask.NextFrame();
            }

            _getInProgress = false;

            if (_tileInfoUpdateRequested)
                _currentTileInformation = _toBeSetTileInformation;

            return lowestLayout;
        }

        #endregion

        #region Internal

        /// <summary>
        ///     Check a new battle spot.
        /// </summary>
        /// <param name="battleLayout">Out value of a new battlelayout</param>
        /// <param name="obstructedValue">Out value of the obstructed value to determine which layout to be used</param>
        /// <param name="checkCenter">Center position of the layout to check</param>
        /// <param name="forward">Direction from the original center</param>
        /// <param name="alliedDirectionChecks">Directions for the allied pokemon spots to check</param>
        /// <param name="enemyDirectionChecks">Directions for the enemy pokemon spots to check</param>
        /// <returns></returns>
        private static bool CheckSpot(out BattleLayout battleLayout, out float obstructedValue,
            Vector3 checkCenter, Vector2 forward, Vector2[] alliedDirectionChecks, Vector2[] enemyDirectionChecks)
        {
            battleLayout = new BattleLayout(false);
            obstructedValue = 0;

            Quaternion rotation = Quaternion.LookRotation(new Vector3(forward.x, 0, forward.y), Vector3.up);
            for (int i = 0; i < 8; i++)
            {
                rotation *= Quaternion.Euler(new Vector3(0, 45, 0));
                Vector3 hitPosition;

                CheckCenterForProps(checkCenter, rotation.ForwardFromRotation().XZ(),
                    rotation.RightFromRotation().XZ());

                foreach (Vector2 a in alliedDirectionChecks)
                {
                    if (!CheckRayHitGround(checkCenter + new Vector3(a.x, 0, a.y), out hitPosition))
                        return false;

                    if (!CheckSpotForProps(hitPosition))
                        return false;
                }

                foreach (Vector2 a in enemyDirectionChecks)
                {
                    if (!CheckRayHitGround(checkCenter + new Vector3(a.x, 0, a.y), out hitPosition))
                        return false;

                    if (!CheckSpotForProps(hitPosition))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Get 8 different directions for use with the check pattern.
        /// </summary>
        /// <param name="originalCenter">The original center between the player and battlestarter</param>
        /// <param name="playerPosition">The player position</param>
        /// <param name="forwardDirections">Out array of directions from the center towards new spots to check</param>
        /// <param name="rightDirections">Out array of right relative to the relevant forward directions</param>
        private static void DirectionsFromOriginalCenter(Vector3 originalCenter, Vector3 playerPosition,
            out Vector2[] forwardDirections, out Vector2[] rightDirections)
        {
            Quaternion rotation = Quaternion.LookRotation(playerPosition - originalCenter, Vector3.up);

            Vector2 forward = rotation.ForwardFromRotation().XZ(),
                right = rotation.RightFromRotation().XZ();

            forwardDirections = new[]
            {
                forward,
                (forward + right).normalized, (forward - right).normalized,
                right, -right,
                (-forward + right).normalized, (-forward - right).normalized,
                -forward
            };

            rightDirections = new Vector2[forwardDirections.Length];

            Vector3 originalNoY = new Vector3(originalCenter.x, 0, originalCenter.z);

            for (int i = 0; i < forwardDirections.Length; i++)
                rightDirections[i] = Quaternion
                    .LookRotation(
                        new Vector3(forwardDirections[i].x, 0, forwardDirections[i].y) - originalNoY,
                        Vector3.up).RightFromRotation().XZ();
        }

        /// <summary>
        ///     Direction for each pokemon spot position based on the giving number of allied and enemy count
        /// </summary>
        /// <param name="alliedCount">Spot count for the player and player allies</param>
        /// <param name="enemyCount">Spots count for the enemies</param>
        /// <param name="allied">Out result direction for the player and player ally spots</param>
        /// <param name="enemy">Out result direction for the enemy spots</param>
        private static void GetDirectionalSpotCheck(int alliedCount, int enemyCount, out Vector2[] allied,
            out Vector2[] enemy)
        {
            allied = alliedCount switch
            {
                1 => new[] { Vector2.up },
                2 => new[] { (Vector2.up + Vector2.right).normalized, (Vector2.up + Vector2.left).normalized },
                _ => new[]
                {
                    (Vector2.up + Vector2.right * 1.5f).normalized,
                    -Vector2.up,
                    (Vector2.up + Vector2.left * 1.5f).normalized
                }
            };

            enemy = enemyCount switch
            {
                1 => new[] { -Vector2.up },
                2 => new[] { -(Vector2.up + Vector2.right).normalized, -(Vector2.up + Vector2.left).normalized },
                _ => new[]
                {
                    -(Vector2.up + Vector2.right * 1.5f).normalized,
                    -Vector2.up,
                    -(Vector2.up + Vector2.left * 1.5f).normalized
                }
            };
        }

        private static bool CheckRayHitGround(Vector3 position, out Vector3 hitPosition)
        {
            if (Physics.Raycast(position, -Vector3.up, out RaycastHit groundHit, RAY_CHECK_DISTANCE, GroundHitLayer,
                    QueryTriggerInteraction.Ignore))
            {
                hitPosition = groundHit.point;
                return true;
            }

            // ReSharper disable once Unity.PreferNonAllocApi
            RaycastHit[] groundHits = Physics.SphereCastAll(position, SPOT_CHECK_RADIUS, -Vector3.up,
                RAY_CHECK_DISTANCE,
                GroundHitLayer);

            if (groundHits.Length > 0)
            {
                Vector2 positionXZ = position.XZ();
                float currentLowestDistance = (groundHits[0].point.XZ() - positionXZ).QuickSquareRootMagnitude();
                hitPosition = groundHits[0].point;

                for (int i = 1; i < groundHits.Length; i++)
                {
                    float d = (groundHits[i].point.XZ() - positionXZ).QuickSquareRootMagnitude();

                    if (d > currentLowestDistance) continue;

                    currentLowestDistance = d;
                    hitPosition = groundHits[i].point;
                }

                return true;
            }

            hitPosition = Vector3.zero;
            return false;
        }

        /// <summary>
        ///     Check if the area of the pokemon spot is filled with props from the environment
        /// </summary>
        /// <param name="position">The position of the spot</param>
        /// <returns>True if the removed area of the spot cylinder is less then "ALLOWED_OBSTRUCTED_PERCENTAGE_VALUE"</returns>
        private static bool CheckSpotForProps(Vector3 position)
        {
            IReadOnlyList<OptimizedTreeInstance>[,] props = _currentTileInformation.GetTreeInstances;
            int xID = Mathf.FloorToInt(position.x -
                                       _currentTileInformation.GetLowestX() /
                                       _currentTileInformation.GetGroupingSize()),
                yID = Mathf.FloorToInt(position.z -
                                       _currentTileInformation.GetLowestY() /
                                       _currentTileInformation.GetGroupingSize());

            xID = Mathf.Clamp(xID, 0, props.GetLength(0));
            yID = Mathf.Clamp(yID, 0, props.GetLength(1));

            List<OptimizedTreeInstance> relevantProps = new List<OptimizedTreeInstance>();
            for (int x = -1; x <= 1 && xID + x >= 0 && xID + x < props.GetLength(0); x++)
            for (int y = -1; y <= 1 && yID + y >= 0 && yID + y < props.GetLength(1); y++)
                relevantProps.AddRange(props[xID + x, yID + y]);

            const float spotArea = SPOT_CHECK_RADIUS * Mathf.PI * SPOT_CHECK_HEIGHT;
            float removeArea = 0;

            foreach (OptimizedTreeInstance i in relevantProps)
            {
                if (new Vector2(position.x - i.position.x, position.x - i.position.y)
                        .QuickSquareRootMagnitude() > SPOT_CHECK_RADIUS + i.radius)
                    continue;

                removeArea += 0.75f * MathC.QuickCircleIntersectCircleArea(position, i.position,
                    SPOT_CHECK_RADIUS, i.radius, SPOT_CHECK_HEIGHT, i.height);
            }

            return 1f / spotArea * removeArea * 100f < ALLOWED_OBSTRUCTED_PERCENTAGE_VALUE;
        }

        /// <summary>
        /// </summary>
        /// <param name="position"></param>
        /// <param name="forward"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private static bool CheckCenterForProps(Vector3 position, Vector2 forward, Vector2 right)
        {
            Vector2 posXZ = position.XZ();
            IReadOnlyList<OptimizedTreeInstance>[,] props = _currentTileInformation.GetTreeInstances;
            int xID = Mathf.FloorToInt(position.x -
                                       _currentTileInformation.GetLowestX() /
                                       _currentTileInformation.GetGroupingSize()),
                yID = Mathf.FloorToInt(position.z -
                                       _currentTileInformation.GetLowestY() /
                                       _currentTileInformation.GetGroupingSize());

            xID = Mathf.Clamp(xID, 0, props.GetLength(0));
            yID = Mathf.Clamp(yID, 0, props.GetLength(1));

            List<OptimizedTreeInstance> relevantProps = new List<OptimizedTreeInstance>();
            for (int x = -2; x <= 2 && xID + x >= 0 && xID + x < props.GetLength(0); x++)
            for (int y = -2; y <= 2 && yID + y >= 0 && yID + y < props.GetLength(1); y++)
                relevantProps.AddRange(props[xID + x, yID + y]);

            foreach (OptimizedTreeInstance optimizedTreeInstance in relevantProps)
            {
                Vector2 newOrigin = posXZ -
                                    forward * (centerBoxDimension.y * .5f + optimizedTreeInstance.radius) -
                                    right * (centerBoxDimension.x * .5f + optimizedTreeInstance.radius);
                Vector2 rightPoint = newOrigin + right * (centerBoxDimension.x + optimizedTreeInstance.radius * 2f),
                    forwardPoint = newOrigin + forward * (centerBoxDimension.y + optimizedTreeInstance.radius * 2f);

                float x = MathC.ClosestPointValue(posXZ, newOrigin, rightPoint),
                    y = MathC.ClosestPointValue(posXZ, newOrigin, forwardPoint);

                if (x == 0 || x == 1 || y == 0 || y == 1)
                    continue;
            }

            return true;
        }

        #endregion
    }
}