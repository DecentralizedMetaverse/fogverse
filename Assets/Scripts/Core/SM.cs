using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System;

namespace DC
{
    /// <summary>
    /// Sceneの読み込み・破棄を管理するクラス
    /// </summary>
    public class SM : MonoBehaviour
    {
        static int previous;

        private void Awake()
        {
            previous = 0;
            SceneManager.sceneUnloaded += SceneUnloaded;
        }

        async void Start()
        {
            LoadGameManager();
            // メソッドの登録
            GM.Add<string>("Scene", LoadSceneGroup);
            GM.Add<string, string>("SceneSingle", LoadSceneSingle);
            
            await UniTask.Yield();
            // 初期シーン読み込み
            Load(GM.mng.firstScene).Forget();
        }

        /// <summary>
        /// ゲーム管理Sceneをロードする
        /// </summary>
        private static void LoadGameManager()
        {
            // ゲーム管理シーン読み込み index:0はロード済みなので飛ばす
            for (var i = 1; i < GM.mng.data[0].sceneName.Length; i++)
            {
                if (GM.mng.data[0].sceneName[i] == "")
                    continue;

                SceneManager.LoadScene(/*"Scenes/" + */GM.mng.data[0].sceneName[i], LoadSceneMode.Additive);
            }
        }

        /// <summary>
        /// アンロード時に呼び出されるメソッド
        /// </summary>
        /// <param name="thisScene"></param>
        void SceneUnloaded(Scene thisScene)
        {
            GM.Msg("Unloaded", thisScene.name);
        }

        /// <summary>
        /// SceneGroupをロード・アンロード
        /// </summary>
        /// <param name="loadScene"></param>
        async void LoadSceneGroup(string loadScene)
        {
            await Load((eScene.Scene)Enum.Parse(typeof(eScene.Scene), loadScene));
        }

        /// <summary>
        /// Sceneを1つロード・アンロード
        /// </summary>
        /// <param name="arts"></param>
        void LoadSceneSingle(string dt2, string dt21)
        {
            if (dt2 == "add")
            {
                SceneManager.LoadScene(dt21, LoadSceneMode.Additive);
            }
            else if (dt2 == "unload")
            {
                SceneManager.UnloadSceneAsync(dt21);
            }
            else if (dt2 == "load")
            {
                SceneManager.LoadScene(dt21);
            }
        }

        /// <summary>
        /// SceneGroupをロード
        /// </summary>
        /// <param name="scene"></param>
        async UniTask Load(eScene.Scene scene)
        {
            if (scene == 0)
            {
                GM.LogError("ゲーム管理は読み込まないでください");
                return;
            }

            await GM.MsgAsync("ShowLoading");   // ロード画面表示
            InternalLoadSceneGroup(scene);      // Sceneロード
            GM.Msg("CloseLoading");
        }

        /// <summary>
        /// [内部処理] SceneGroupをロード・アンロード
        /// </summary>
        void InternalLoadSceneGroup(eScene.Scene loadScene)
        {
            // ロードScene
            for (var i = 0; i < GM.mng.data[(int)loadScene].sceneName.Length; i++)
            {
                var val = GM.mng.data[(int)loadScene].sceneName[i];

                if (val == null || (i < GM.mng.data[previous].sceneName.Length &&
                    val == GM.mng.data[previous].sceneName[i]))
                    continue;

                SceneManager.LoadScene(val, LoadSceneMode.Additive);
                GM.Log("Loaded: " + val);
            }

            // アンロードScene
            if (previous != 0)
            {
                for (var i = 0; i < GM.mng.data[(int)previous].sceneName.Length; i++)
                {
                    var val = GM.mng.data[(int)previous].sceneName[i];

                    if (val == null || (i < GM.mng.data[(int)loadScene].sceneName.Length &&
                        val == GM.mng.data[(int)loadScene].sceneName[i]))
                        continue;

                    GM.Log("UnLoad: " + val);
                    SceneManager.UnloadSceneAsync(val);
                }
            }
            previous = (int)loadScene;
        }
    }
}