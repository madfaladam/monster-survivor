using UnityEngine;

namespace MonsterSurvivor.Combat
{
    public interface IEnemyTargetProvider
    {
        EnemyController FindNearest(Vector2 origin, float range);
    }
}
