using UnityEngine;
using UnityEngine.AI;

namespace Enemy
{
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyManager : MonoBehaviour
    {
        private enum State
        {
            Idle,
            MoveTo,
            Return,
            Attack,
            Die
        }
        [SerializeField] private EnemyData _enemyData;

        #region FSM

        private State _currentState;

        #endregion

        #region Animation

        [SerializeField] private Animator _enemyAnimator;
        private int _runningAnimationToHash;
        private int _animAttackToHash;
        private int _animTakeHitToHash;

        #endregion
        #region Miscellanous

        private float _currentHealth;
        private float _attackDelay;
        private Transform _target;
        private NavMeshAgent _agent;
        private Vector3 _spawnPointPosition;
        private Coroutine _attackRoutine;
        private bool _canAttack;
        #endregion

        private void Awake()
        {
            _currentState = State.Idle;
            GetComponent<SphereCollider>().radius = _enemyData.fovRadius;
            _currentHealth = _enemyData.healthPoints;
            _agent = GetComponent<NavMeshAgent>();
            _spawnPointPosition = transform.position; 
             _runningAnimationToHash =Animator.StringToHash("Running");
             _animAttackToHash = Animator.StringToHash("Attack");
             _animTakeHitToHash = Animator.StringToHash("TakeHit");
             _attackRoutine = null;
        }

        private void Update()
        {
      // Debug.Log(_agent.isStopped);
            if (_currentState==State.MoveTo || _currentState == State.Return)
            {
                _MoveToOrReturn();
            }else if (_currentState==State.Attack)
            {
                _Attack();
            }
        }

        private void _Attack()
        {
            if (!_canAttack)
                return;
            
            if ((_target.position-transform.position).magnitude> _enemyData.attackRadius)
            {
                _currentState = State.MoveTo;
                _agent.destination = _target.position;
                _enemyAnimator.SetBool(_runningAnimationToHash,true);
                _enemyAnimator.ResetTrigger(_animAttackToHash);
                //_agent.isStopped = false;
                return;
            }

            transform.rotation = Quaternion.LookRotation(_target.position - transform.position, Vector3.up);
            
            if (_attackDelay>=_enemyData.attackRate)
            {
                //Attack
                _enemyAnimator.SetTrigger(_animAttackToHash);
                _attackDelay = 0f;
                _attackRoutine=  StartCoroutine(Tools.Utils.WaitingForCurrentAnimation(_enemyAnimator, () =>
                {
                    _agent.isStopped = false;
                    _attackRoutine = null;
                    
                    Debug.Log(_agent.isStopped);
                },
                    waitForAnimName: "Melee Attack")); //without this waitForAnimName,
                                                       //the agent doesn't resume moving at the end of the animation
                                                       //It's not in the tutorial but dunno if it was forgotten

            }
            else
            {
                _attackDelay += Time.deltaTime;
            }
        }
        
        // private void OnDrawGizmos()
        // {
        //     if (_target!=null)
        //     {
        //         Gizmos.color = Color.red;
        //         Gizmos.DrawLine(transform.position, _target.position);
        //     }
        //    
        // }

        private void _MoveToOrReturn()
        {
            if (_agent.isStopped)
            {
                return;  
            }
            if (_currentState==State.Return && _agent.remainingDistance<0.1)
            {
                //Idle state switch
                _currentState = State.Idle;
                _agent.destination = transform.position;
                _agent.velocity = Vector3.zero;
                _enemyAnimator.SetBool(_runningAnimationToHash, false);
                _target = null;
            }
            else if(_currentState==State.MoveTo && _agent.remainingDistance<= _enemyData.attackRadius)
            {
                if (_target!=null && _target.CompareTag("Player"))
                {
                    _currentState = State.Attack;
                    _agent.destination = transform.position;
                    _agent.velocity = Vector3.zero;
                    _enemyAnimator.SetBool(_runningAnimationToHash, false);
                    _attackDelay = 0f;
                    _canAttack = true;
                    _agent.isStopped = true;
                }
            }
            else if (_target!=null)
            {
                _agent.destination = _target.position;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _target = other.transform;
                _currentState = State.MoveTo;
                _agent.destination = _target.position;
                transform.rotation = Quaternion.LookRotation(_target.position -transform.position, Vector3.up);
                _enemyAnimator.SetBool(_runningAnimationToHash,true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _target = null;
                _currentState = State.Return;
                _agent.destination = _spawnPointPosition;
                _agent.velocity = Vector3.zero;
                transform.rotation = Quaternion.LookRotation(_spawnPointPosition - transform.position, Vector3.up);
                _enemyAnimator.SetBool(_runningAnimationToHash,true);
            }
        }

        public void TakeDamage(float damage)
        {
            _currentHealth-= damage;
          
            if (_currentHealth<=0)
            {
                Destroy(gameObject);
            }
            else
            {
                _enemyAnimator.SetTrigger(_animTakeHitToHash);
                _canAttack = false;
                _attackDelay = 0f;
                if (_attackRoutine!=null)
                {
                    StopCoroutine(_attackRoutine);
                }

                StartCoroutine(Tools.Utils.WaitingForCurrentAnimation(_enemyAnimator, () =>
                {
                    _canAttack = true;
                }));
            }
        }
    }
}

