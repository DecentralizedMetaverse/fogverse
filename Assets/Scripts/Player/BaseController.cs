using DC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DC.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class BaseController : MonoBehaviour
    {
        public event Action UpdateHandler;

        [SerializeField] Animator anim;
        [SerializeField] float moveSpeed = 3500f;
        [SerializeField] float multiplyDash = 2.0f;
        [SerializeField] float jumpPower = 10f;
        [SerializeField] float divideMoveSpeed=0.1f;

        Rigidbody rb;
        int layerMask = 0;
        const float sizeCollider = 0.125f;
        Transform mainCamera;
        bool isGrounded;
        readonly int animSpeed = Animator.StringToHash("moveSpeed");

        public void Init()
        {
            layerMask = LayerMask.GetMask("Default");
            rb = GetComponent<Rigidbody>();

            if(UnityEngine.Camera.main == null)
            {
                GM.LogWarning("Camera not found");
                return;
            }
            mainCamera = UnityEngine.Camera.main.transform;
        }

        /// <summary>
        /// 移動する
        /// </summary>
        /// <param name="moveVector"></param>
        /// <param name="dash"></param>
        public virtual void Move(Vector2 moveVector, bool dash = false)
        {
            UpdateHandler?.Invoke();


            var move = CalcMovementFromCamera(moveVector);
            // move.y += CalcYForceFromTerrainHeight(move);

            move *= moveSpeed;

            if (isGrounded)
            {
                if (dash)
                {
                    // ダッシュ
                    move *= multiplyDash;
                }

                move -= rb.velocity * rb.mass * 9.81f;
            }

            if (moveVector != Vector2.zero)
            {
                // 入力がある場合
                var targetRotation = rb.velocity;
                targetRotation.y = 0;
                ChangeAngleFromInput(targetRotation); // 向きを変える
            }

            rb.AddForce(move);
            anim.SetFloat(animSpeed, rb.velocity.magnitude * divideMoveSpeed);
        }

        /// <summary>
        /// ジャンプ
        /// </summary>
        public void Jump()
        {
            if (!Physics.CheckSphere(transform.position, sizeCollider, layerMask)) return;
            rb.AddForce(transform.up * jumpPower, ForceMode.Impulse);
        }

        void OnCollisionEnter(Collision collision)
        {
            isGrounded = true;
        }
        
        void OnCollisionExit(Collision collision)
        {
            isGrounded = false;
        }


        // -----------------------------------------------------------
        // 地形の傾きからY軸の移動量を計算
        float CalcYForceFromTerrainHeight(Vector3 moveVec)
        {
            if (!isGrounded) return 0;

            RaycastHit hit;
            if (Physics.Raycast(
                transform.position + moveVec + Vector3.up,
                -transform.up,
                out hit,
                5.0f,
                layerMask
            ))
                return 1.0f - hit.distance;
            return 0;
        }

# if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Gizmos.DrawRay(transform.position + transform.forward + Vector3.up, -transform.up * 2.0f);
        }
#endif

        /// <summary>
        /// カメラの角度から移動ベクトルを計算
        /// </summary>
        Vector3 CalcMovementFromCamera(Vector2 moveInput)
        {
            if (mainCamera == null) return Vector3.zero;

            var forward = mainCamera.forward;
            forward.Normalize();
            forward.y = 0;
            return (mainCamera.right * moveInput.x) + (forward * moveInput.y);
        }

        /// <summary>
        /// 移動ベクトルをもとにして、キャラクターの向きを変える
        /// Change the character's orientation
        /// </summary>
        /// <param name="angle"></param>
        public void ChangeAngleFromInput(Vector3 angle, float speed = 10f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(angle), Time.deltaTime * speed);
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.position, sizeCollider);
        }
#endif
    }
}