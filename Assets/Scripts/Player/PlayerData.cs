using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    [CreateAssetMenu(fileName = "PlayerData",menuName = "ScriptableObjects/PlayerStats")]
    public class PlayerData : ScriptableObject
    {
        public float moveSpeed;
        public float attackRange;
        public float attackDamage;
    }
}

