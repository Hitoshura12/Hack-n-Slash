using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        #region Attack

        private const float MIN_COMBO_DELAY = 0.1F;
        private const int COMBO_MAX_STEP = 2;
        private int _attackCount;
        private bool attacking;
        private Coroutine _comboAttackResetCoroutine;

        #endregion

        #region Input

        private InputActions _inputActions;
        private InputAction _moveAction;
        private InputAction _attackAction;

        #endregion

        #region Movement

        private Vector2 _move;
       [SerializeField] private PlayerData _playerData;
        private bool running;
        private Transform _animatorTransform;

        #endregion

        #region Animation

        private int _runningStringToHash = Animator.StringToHash("Running");
        private int _attackStepStringToHash = Animator.StringToHash("PunchCount");

        #endregion

        private CharacterController _characterController;
        [SerializeField] private Animator _animator;

        private void Awake()
        {
            _inputActions = new InputActions();
            _animatorTransform = _animator.transform;
            _characterController = GetComponent<CharacterController>();
            _attackCount = -1;
            attacking = false;
            _comboAttackResetCoroutine = null;
        }

        void Update()
        {
            if (attacking)
                return;

            _move = _moveAction.ReadValue<Vector2>();
            if (_move.sqrMagnitude > 0.01f)
            {
                if (!running)
                {
                    running = true;
                    _animator.SetBool(_runningStringToHash, true);
                }

                Vector3 v = new Vector3(_move.x, 0, _move.y);
                _characterController.Move(v * (_playerData.moveSpeed * Time.deltaTime));
                _animatorTransform.rotation = Quaternion.LookRotation(-v, Vector3.up);
            }
            else
            {
                running = false;
                _animator.SetBool(_runningStringToHash, false);
            }
        }

        private void OnEnable()
        {
            _moveAction = _inputActions.Player.Move;
            _moveAction.Enable();
            _attackAction = _inputActions.Player.Attack;
            _attackAction.performed += _OnAttackAction;
            _attackAction.Enable();
        }
        private void _OnAttackAction(InputAction.CallbackContext obj)
        {
            attacking = true;
            if (_attackCount == COMBO_MAX_STEP)
                return;
            float stateNormalizedTime = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            if (_attackCount == -1 || (stateNormalizedTime >= 0.1f && stateNormalizedTime <= 0.8f))
            {
                if (_comboAttackResetCoroutine!=null)
                {
                    StopCoroutine(_comboAttackResetCoroutine);
                }
                _attackCount++;
                _animator.SetBool(_runningStringToHash, false);
                _animator.SetInteger(_attackStepStringToHash, _attackCount);
                _comboAttackResetCoroutine = StartCoroutine(
                    Tools.Utils.WaitingForCurrentAnimation(_animator, () =>
                    {
                        _attackCount = -1;
                        _animator.SetInteger(_attackStepStringToHash,_attackCount);
                        attacking = false;
                    },stopAfterAnim: true));
            }
        }
        private IEnumerator _AttackResetComboCoroutine()
        { 
            
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(_animator.GetAnimatorTransitionInfo(0).duration);
            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(()=>_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f);
            _attackCount = -1;

            _animator.SetInteger(_attackStepStringToHash, _attackCount);
            _move = _moveAction.ReadValue<Vector2>();
            if (_move.sqrMagnitude > 0.01f && running)
            {
                _animator.SetBool(_runningStringToHash, true);
            }
            attacking = false;
        }

        

        private void OnDisable()
        {
            _moveAction.Disable();
            _attackAction.Disable();
        }
    }
}