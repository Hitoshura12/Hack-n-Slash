using System.Collections;
using System.Collections.Generic;
using Enemy;
using Player;
using UnityEngine;

namespace Player
{
    public class PlayerAttackManager : MonoBehaviour
    {
        private static readonly int ENEMY_LAYER = 1<< 7;

        [SerializeField] private PlayerData _playerData;
        [SerializeField] private Transform leftHand;
        [SerializeField] private Transform rightHand;
        public void PlayerHitLeft(int attackLeft)
        {
            PlayerHit( attackLeft,leftHand);
        }

        public void PlayerHitRight(int attackRight)
        {
            PlayerHit(attackRight, rightHand);
        }
        public void PlayerHit(int attackCount, Transform hand)
        {
            Debug.Log($"Punch :{attackCount}");

            Collider[] enemyColliders = Physics.OverlapSphere(hand.position, _playerData.attackRange,ENEMY_LAYER);
          
            foreach (var enemy in enemyColliders)
            { 
                Enemy.EnemyManager em = enemy.GetComponent<Enemy.EnemyManager>();
                if (em!=null)
                {
                    em.TakeDamage(_playerData.attackDamage* (attackCount+1));
                }  
            }
          
        }
    }
}

