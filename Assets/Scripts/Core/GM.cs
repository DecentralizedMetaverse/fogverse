
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DC
{
    /// <summary>
    /// メッセージ管理
    /// </summary>
    public static class GM
    {
        public static string password = "jf298j$ga\"2f)U)#";
        const string pathSaveFunctionList = "Assets/DB/function_list.txt";
        public static DB db;
        public static DB_GameManager mng;
        static Dictionary<string, List<Delegate>> functions = new();
        // static List<string> userFunctions = new();

        /// <summary>
        /// 初期化
        /// </summary>
        public static void Init()
        {
            functions.Clear();
            mng.Init();
        }

        /// <summary>
        /// [Lua呼び出し用] ユーザーメソッドの存在を確認する
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        //public static bool GetIsUserFunction(string key)
        //{
        //    return userFunctions.Contains(key);
        //}

        /// <summary>
        /// [Lua呼び出し用] ユーザーメソッドを呼び出す
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        //public static string ExecuteUserCommand(string command)
        //{
        //    string[] splitString = command.Split(" ");
        //    string functionName = splitString[0];
        //    List<string> args = new();
        //    string argsString = "";
        //    if (splitString.Length > 1)
        //    {

        //        for (int i = 1; i < splitString.Length; i++)
        //        {
        //            args.Add(splitString[i]);
        //        }

        //        argsString = String.Join(",", args.ToArray());
        //    }

        //    Debug.Log($"{functionName}({argsString})");
        //    if (GetIsUserFunction(functionName))
        //    {
        //        LuaManager.lua.DoString($"{functionName}({argsString})");
        //        return $"Command Executed \"{command}\"";
        //    }

        //    return $"Command Not Found \"{functionName}\"";
        //}

        /// <summary>
        /// 送信
        /// </summary>
        /// <param name="key">keyword</param>
        /// <param name="args">データ1,データ2,データ3,...</param>
        /// <returns>true:成功 false:失敗</returns>
        public static bool Msg(string key, params object[] args)
        {
            if (functions.ContainsKey(key))
            {
                for (int i = 0; i < functions[key].Count; i++)
                {
                    functions[key][i].DynamicInvoke(args);
#if UNITY_EDITOR
                    var msg = $"<color=#628cb8>実行:</color> <b><color=#ffa500>{key}</color> ";
#endif
                }
            }
            else
            {
                //存在しない場合
                Log($"<color=#ff4500>送信失敗 failed:</color> <b>{key}</b>");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 返り値ありMsg
        /// Msg<返り値の型>
        /// </summary>
        /// <typeparam name="T">返り値の型</typeparam>
        /// <param name="key"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static T Msg<T>(string key, params object[] args)
        {
            return (T)functions[key][0].DynamicInvoke(args);
        }        

        /// <summary>
        /// 返り値がある場合は、Msgを使用してください
        /// </summary>
        /// <param name="key"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static UniTask MsgAsync(string key, params object[] args)
        {
            return (UniTask)functions[key][0].DynamicInvoke(args);
        }

        /// <summary>
        /// 受信先追加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="function">ここでは「メソッド」を入力してください</param>
        /// <param name="isPublic">関数をユーザーにも公開するか</param>
        public static void Add(string key, Action function, bool isPublic = false)
        {
            AddFunction(key, function, isPublic);
        }

        /// <summary>
        /// 受信先追加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="function">ここでは「メソッド」を入力してください</param>
        /// <param name="isPublic">関数をユーザーにも公開するか</param>
        public static void Add<T>(string key, Action<T> function, bool isPublic = false)
        {
            AddFunction(key, function, isPublic);
        }

        /// <summary>
        /// 受信先追加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="function">ここでは「メソッド」を入力してください</param>
        /// <param name="isPublic">関数をユーザーにも公開するか</param>    
        public static void Add<T1, T2>(string key, Action<T1, T2> function, bool isPublic = false)
        {
            AddFunction(key, function, isPublic);
        }

        /// <summary>
        /// 受信先追加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="function">ここでは「メソッド」を入力してください</param>
        /// <param name="isPublic">関数をユーザーにも公開するか</param>
        public static void Add<T1, T2, T3>(string key, Action<T1, T2, T3> function, bool isPublic = false)
        {
            AddFunction(key, function, isPublic);
        }

        /// <summary>
        /// 受信先追加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="function">ここでは「メソッド」を入力してください</param>
        /// <param name="isPublic">関数をユーザーにも公開するか</param>
        public static void Add<T>(string key, Func<T> function, bool isPublic = false)
        {
            AddFunction(key, function, isPublic);

            if (typeof(T) == typeof(UniTask))
            {
                Func<IEnumerator> coroutine = () =>
                {
                    var func = function as Func<UniTask>;
                    return func().ToCoroutine();
                };
                AddFunction(key + "Async", coroutine, isPublic);
            }
        }

        /// <summary>
        /// 受信先追加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="function">ここでは「メソッド」を入力してください</param>
        /// <param name="isPublic">関数をユーザーにも公開するか</param>    
        public static void Add<T1, T2>(string key, Func<T1, T2> function, bool isPublic = false)
        {
            AddFunction(key, function, isPublic);

            if (typeof(T2) == typeof(UniTask))
            {
                Func<T1, IEnumerator> coroutine = (args) =>
                {
                    var func = function as Func<T1, UniTask>;
                    return func(args).ToCoroutine();
                };
                AddFunction(key + "Async", coroutine, isPublic);
            }
        }

        /// <summary>
        /// 受信先追加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="function">ここでは「メソッド」を入力してください</param>
        /// <param name="isPublic">関数をユーザーにも公開するか</param>    
        public static void Add<T1, T2, T3>(string key, Func<T1, T2, T3> function, bool isPublic = false)
        {
            AddFunction(key, function, isPublic);

            if (typeof(T3) == typeof(UniTask))
            {
                Func<T1, T2, IEnumerator> coroutine = (arg1, arg2) =>
                {
                    var func = function as Func<T1, T2, UniTask>;
                    return func(arg1, arg2).ToCoroutine();
                };
                AddFunction(key + "Async", coroutine, isPublic);
            }
        }

        /// <summary>
        /// 受信先追加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="function">ここでは「メソッド」を入力してください</param>
        /// <param name="isPublic">関数をユーザーにも公開するか</param>    
        public static void Add<T1, T2, T3, T4>(string key, Func<T1, T2, T3, T4> function, bool isPublic = false)
        {
            AddFunction(key, function, isPublic);

            if (typeof(T4) == typeof(UniTask))
            {
                Func<T1, T2, T3, IEnumerator> coroutine = (arg1, arg2, arg3) =>
                {
                    var func = function as Func<T1, T2, T3, UniTask>;
                    return func(arg1, arg2, arg3).ToCoroutine();
                };
                AddFunction(key + "Async", coroutine, isPublic);
            }
        }

        /// <summary>
        /// Addの共通処理をまとめたメソッド
        /// </summary>
        /// <param name="key"></param>
        /// <param name="function"></param>
        /// <param name="isPublic"></param>
        static void AddFunction(string key, Delegate function, bool isPublic = false)
        {
            if (!functions.ContainsKey(key))
            {
                functions.Add(key, new());
            }
            functions[key].Add(function);
            Log($"<color=#1e90ff>受信先追加:</color> <b>{key}</b>");

            // 多重登録防止
            if (IsUniTaskMethod(function))
            {
                return;
            }

            // Luaに登録
            if (isPublic)
            {
                LuaManager.RegisterLuaFunction(LuaManager.lua, key, function);
            }
            LuaManager.RegisterLuaFunction(LuaManager.luaInternal, key, function);
        }

        private static bool IsUniTaskMethod(Delegate function)
        {
            return function.Method.ReturnType == typeof(UniTask);
        }

        /// <summary>
        /// [Lua呼び出し用] メソッドを取得する
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static Delegate GetFunction(string key)
        {
            return functions[key].Last();
        }

        /// <summary>
        /// 受信先削除
        /// </summary>
        /// <param name="key"></param>
        /// <returns>true:成功 false:失敗</returns>
        public static bool Remove(string key, Action function)
        {
            if (!functions.ContainsKey(key))
            {
                Log($"<color=#ff4040>受信先削除失敗:</color> <b>{key}</b>");
                return false;
            }
            GM.functions[key].Remove(function);

            if (GM.functions[key].Count == 0)
                GM.functions.Remove(key);
            Log($"<color=#dda0dd>受信先削除:</color> <b>{key}</b>");
            return true;
        }

        /// <summary>
        /// 受信先削除
        /// </summary>
        /// <param name="key"></param>
        /// <returns>true:成功 false:失敗</returns>
        public static bool Remove<T>(string key, Action<T> function)
        {
            if (!functions.ContainsKey(key))
            {
                Log($"<color=#ff4040>受信先削除失敗:</color> <b>{key}</b>");
                return false;
            }
            GM.functions[key].Remove(function);

            if (GM.functions[key].Count == 0)
                GM.functions.Remove(key);
            Log($"<color=#dda0dd>受信先削除:</color> <b>{key}</b>");
            return true;
        }

        static ePause.mode _pause;
        /// <summary>
        /// ポーズモード
        /// 入力例(
        /// Gm.pose = ePose.mode.UIStop;
        /// Gm.pose = ePose.mode.GameStop;
        /// Gm.pose = ePose.mode.none;
        /// )
        /// </summary>
        public static ePause.mode pause
        {
            get { return _pause; }
            set
            {
                _pause = value;
                if (value == ePause.mode.GameStop)
                {
                    Debug.Log("GameStop");
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
                else if (value == ePause.mode.none)
                {
                    Debug.Log("none");
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
                InputF.SetOperation(value);
            }
        }

        /// <summary>
        /// ゲーム終了処理
        /// </summary>
        /// <returns></returns>
        public static void GameQuit()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
        Application.Quit();
#endif
        }

        /// <summary>
        /// ログ表示
        /// </summary>
        /// <param name="message"></param>
        public static void Log(string message)
        {
            if (mng != null && !mng.visibleLog) return;
            Debug.Log(message);
        }

        /// <summary>
        /// エラーログ表示
        /// </summary>
        /// <param name="message"></param>
        public static void LogError(string message)
        {
            if (!mng.visibleLog) return;
            Debug.LogError(message);
        }

        /// <summary>
        /// 警告ログ表示
        /// </summary>
        /// <param name="message"></param>
        public static void LogWarning(string message)
        {
            if (!mng.visibleLog) return;
            Debug.LogWarning(message);
        }


        /// <summary>
        /// 登録された関数を一覧としてテキストFileに書き出す
        /// </summary>
        public static void SaveFunctionList(DB_FunctionList db)
        {
            db.SaveFunctionList(functions);
        }
    }
}