using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace DC.Player
{
    public class PlayerCamera : MonoBehaviour
    {
        [SerializeField] float length = 3;
        [SerializeField] float minLength = -1f;
        [SerializeField] float maxLength = 5f;
        [SerializeField] float[] cameraLength = new float[] { 3, -0.5f };
        [SerializeField] byte cameraType = 0;
        [SerializeField] float limitAngle = 45;
        [SerializeField] float speedScroll = 0.25f;
        [SerializeField] float speedInput = 0.5f;
        [SerializeField] float offsetY = 1.5f;
        [SerializeField] float eyeY = 0.5f;
        [SerializeField, Range(0, 1.0f)] float speedPos = 0.25f;

        Transform mainCamera;
        Transform transform_x;
        Transform transform_y;
        Transform player;

        int layerMask = 0;

        void Start()
        {
            InputF.action.Game.Camera.started += OnCamera;
            InputF.action.Game.Camera.performed += OnCamera;
            InputF.action.Game.Camera.canceled += OnCamera;

            InputF.action.Game.Scroll.started += OnScroll;
            InputF.action.Game.Scroll.performed += OnScroll;
            InputF.action.Game.Scroll.canceled += OnScroll;

            layerMask = LayerMask.GetMask("Default");
            transform_y = transform.GetChild(0);
            transform_x = transform.GetChild(0).GetChild(0);
            mainCamera = UnityEngine.Camera.main.transform;

            GetPlayer().Forget();
        }

        /// <summary>
        /// プレイヤーを探す
        /// </summary>
        /// <returns></returns>
        async UniTask GetPlayer()
        {
            GameObject obj;
            while(true)
            {
                obj = GameObject.FindWithTag("Player");
                if (obj != null) break;

                await UniTask.Yield();
            }

            var controller = obj.GetComponent<BaseController>();
            controller.UpdateHandler += UpdateCamera;
            player = obj.transform;
        }

        Vector3 p1;
        Vector3 pos, cpos;
        float xs = 0, ys = 0;
        RaycastHit hit;
        float ln = 3, lns = 3;
        float scroll;
        float rot_x, rot_y;
        Vector2 rotateVec;        

        public void UpdateCamera()
        {
            //----------------壁すり抜け回避----------------
            WallSlipPrevention();

            //----------------カメラの距離----------------
            CameraDistance();

            //----------------座標の更新----------------
            UpdatePosition();

            //----------------カメラ回転----------------
            RotateCamera();
        }

        /// <summary>
        /// 壁すり抜け回避
        /// </summary>
        void WallSlipPrevention()
        {
            if (Physics.CheckSphere(transform.position, 0.35f, layerMask))
            {
                ln = -0.1f;
                lns = ln;
            }
            else if (Physics.SphereCast(transform.position, 0.35f, -mainCamera.forward, out hit, maxLength, layerMask))
            {
                ln = hit.distance - 0.1f;
                if (ln > length) ln = length;
                lns = ln;
            }
            else
            {
                ln = length;
                lns = lns.Move(ln, speedInput);
            }

            cpos = mainCamera.localPosition;
            cpos.z = -lns;
            mainCamera.localPosition = cpos;
        }

        void OnDrawGizmos()
        {
            if (mainCamera == null)
                mainCamera = UnityEngine.Camera.main.transform;

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, 0.25f);
            Gizmos.DrawSphere(mainCamera.position, 0.25f);
        }

        /// <summary>
        /// 座標の更新
        /// </summary>
        void UpdatePosition()
        {
            pos = player.position;

            pos.y += offsetY;
            p1 = p1.Move(pos, speedPos);
            transform.position = p1;

        }

        /// <summary>
        /// カメラの回転
        /// </summary>
        void RotateCamera()
        {
            xs = xs.Move(rotateVec.y, speedInput);
            ys = ys.Move(rotateVec.x, speedInput);

            LimitAngle();

            transform_x.localRotation = Quaternion.Euler(rot_x, 0, 0);
            transform_y.localRotation = Quaternion.Euler(0, rot_y, 0);
        }

        /// <summary>
        /// 角度に制限をかける
        /// </summary>
        private void LimitAngle()
        {
            //0～360 -> -180 ～ 180 
            rot_x = (transform_x.localRotation.eulerAngles.x > 180f) ?
                transform_x.localRotation.eulerAngles.x - 360 : transform_x.localRotation.eulerAngles.x;
            rot_y = transform_y.localRotation.eulerAngles.y;
            rot_x -= xs;
            rot_y += ys;
            rot_x = Mathf.Clamp(rot_x, -limitAngle, limitAngle);
            //-180 ～ 180 -> 0～360
            rot_x = (rot_x < 0) ?
                rot_x + 360 : rot_x;
        }

        /// <summary>
        /// カメラの距離
        /// </summary>
        void CameraDistance()
        {
            //scroll = InputF.GetAxis(eInputMap.data.Scroll) * _speed;
            length -= scroll;
            length = Mathf.Clamp(length, minLength, maxLength);
        }

        /// <summary>
        /// [入力] カメラの回転
        /// </summary>
        void OnCamera(InputAction.CallbackContext context)
        {
            rotateVec = context.ReadValue<Vector2>();
        }

        /// <summary>
        /// [入力] カメラの距離
        /// </summary>
        void OnScroll(InputAction.CallbackContext context)
        {
            scroll = context.ReadValue<float>() * speedScroll;
        }
    }

}