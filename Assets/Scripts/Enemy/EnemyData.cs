using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    [CreateAssetMenu(fileName = "EnemyData",menuName = "ScriptableObjects/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        public float healthPoints;
        public float fovRadius;
        public float moveSpeed;
        public float attackRadius;
        public float attackRate;
    }
}

