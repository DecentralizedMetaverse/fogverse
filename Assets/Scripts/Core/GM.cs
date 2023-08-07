
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
    /// ���b�Z�[�W�Ǘ�
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
        /// ������
        /// </summary>
        public static void Init()
        {
            functions.Clear();
            mng.Init();
        }

        /// <summary>
        /// [Lua�Ăяo���p] ���[�U�[���\�b�h�̑��݂��m�F����
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        //public static bool GetIsUserFunction(string key)
        //{
        //    return userFunctions.Contains(key);
        //}

        /// <summary>
        /// [Lua�Ăяo���p] ���[�U�[���\�b�h���Ăяo��
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
        /// ���M
        /// </summary>
        /// <param name="key">keyword</param>
        /// <param name="args">�f�[�^1,�f�[�^2,�f�[�^3,...</param>
        /// <returns>true:���� false:���s</returns>
        public static bool Msg(string key, params object[] args)
        {
            if (functions.ContainsKey(key))
            {
                for (int i = 0; i < functions[key].Count; i++)
                {
                    functions[key][i].DynamicInvoke(args);
#if UNITY_EDITOR
                    var msg = $"<color=#628cb8>���s:</color> <b><color=#ffa500>{key}</color> ";
#endif
                }
            }
            else
            {
                //���݂��Ȃ��ꍇ
                Log($"<color=#ff4500>���M���s failed:</color> <b>{key}</b>");
                return false;
            }

            return true;
        }

        /// <summary>
        /// �Ԃ�l����Msg
        /// Msg<�Ԃ�l�̌^>
        /// </summary>
        /// <typeparam name="T">�Ԃ�l�̌^</typeparam>
        /// <param name="key"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static T Msg<T>(string key, params object[] args)
        {
            return (T)functions[key][0].DynamicInvoke(args);
        }        

        /// <summary>
        /// �Ԃ�l������ꍇ�́AMsg���g�p���Ă�������
        /// </summary>
        /// <param name="key"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static UniTask MsgAsync(string key, params object[] args)
        {
            return (UniTask)functions[key][0].DynamicInvoke(args);
        }

        /// <summary>
        /// ��M��ǉ�
        /// </summary>
        /// <param name="key"></param>
        /// <param name="function">�����ł́u���\�b�h�v����͂��Ă�������</param>
        /// <param name="isPublic">�֐������[�U�[�ɂ����J���邩</param>
        public static void Add(string key, Action function, bool isPublic = false)
        {
            AddFunction(key, function, isPublic);
        }

        /// <summary>
        /// ��M��ǉ�
        /// </summary>
        /// <param name="key"></param>
        /// <param name="function">�����ł́u���\�b�h�v����͂��Ă�������</param>
        /// <param name="isPublic">�֐������[�U�[�ɂ����J���邩</param>
        public static void Add<T>(string key, Action<T> function, bool isPublic = false)
        {
            AddFunction(key, function, isPublic);
        }

        /// <summary>
        /// ��M��ǉ�
        /// </summary>
        /// <param name="key"></param>
        /// <param name="function">�����ł́u���\�b�h�v����͂��Ă�������</param>
        /// <param name="isPublic">�֐������[�U�[�ɂ����J���邩</param>    
        public static void Add<T1, T2>(string key, Action<T1, T2> function, bool isPublic = false)
        {
            AddFunction(key, function, isPublic);
        }

        /// <summary>
        /// ��M��ǉ�
        /// </summary>
        /// <param name="key"></param>
        /// <param name="function">�����ł́u���\�b�h�v����͂��Ă�������</param>
        /// <param name="isPublic">�֐������[�U�[�ɂ����J���邩</param>
        public static void Add<T1, T2, T3>(string key, Action<T1, T2, T3> function, bool isPublic = false)
        {
            AddFunction(key, function, isPublic);
        }

        /// <summary>
        /// ��M��ǉ�
        /// </summary>
        /// <param name="key"></param>
        /// <param name="function">�����ł́u���\�b�h�v����͂��Ă�������</param>
        /// <param name="isPublic">�֐������[�U�[�ɂ����J���邩</param>
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
        /// ��M��ǉ�
        /// </summary>
        /// <param name="key"></param>
        /// <param name="function">�����ł́u���\�b�h�v����͂��Ă�������</param>
        /// <param name="isPublic">�֐������[�U�[�ɂ����J���邩</param>    
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
        /// ��M��ǉ�
        /// </summary>
        /// <param name="key"></param>
        /// <param name="function">�����ł́u���\�b�h�v����͂��Ă�������</param>
        /// <param name="isPublic">�֐������[�U�[�ɂ����J���邩</param>    
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
        /// ��M��ǉ�
        /// </summary>
        /// <param name="key"></param>
        /// <param name="function">�����ł́u���\�b�h�v����͂��Ă�������</param>
        /// <param name="isPublic">�֐������[�U�[�ɂ����J���邩</param>    
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
        /// Add�̋��ʏ������܂Ƃ߂����\�b�h
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
            Log($"<color=#1e90ff>��M��ǉ�:</color> <b>{key}</b>");

            // ���d�o�^�h�~
            if (IsUniTaskMethod(function))
            {
                return;
            }

            // Lua�ɓo�^
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
        /// [Lua�Ăяo���p] ���\�b�h���擾����
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static Delegate GetFunction(string key)
        {
            return functions[key].Last();
        }

        /// <summary>
        /// ��M��폜
        /// </summary>
        /// <param name="key"></param>
        /// <returns>true:���� false:���s</returns>
        public static bool Remove(string key, Action function)
        {
            if (!functions.ContainsKey(key))
            {
                Log($"<color=#ff4040>��M��폜���s:</color> <b>{key}</b>");
                return false;
            }
            GM.functions[key].Remove(function);

            if (GM.functions[key].Count == 0)
                GM.functions.Remove(key);
            Log($"<color=#dda0dd>��M��폜:</color> <b>{key}</b>");
            return true;
        }

        /// <summary>
        /// ��M��폜
        /// </summary>
        /// <param name="key"></param>
        /// <returns>true:���� false:���s</returns>
        public static bool Remove<T>(string key, Action<T> function)
        {
            if (!functions.ContainsKey(key))
            {
                Log($"<color=#ff4040>��M��폜���s:</color> <b>{key}</b>");
                return false;
            }
            GM.functions[key].Remove(function);

            if (GM.functions[key].Count == 0)
                GM.functions.Remove(key);
            Log($"<color=#dda0dd>��M��폜:</color> <b>{key}</b>");
            return true;
        }

        static ePause.mode _pause;
        /// <summary>
        /// �|�[�Y���[�h
        /// ���͗�(
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
        /// �Q�[���I������
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
        /// ���O�\��
        /// </summary>
        /// <param name="message"></param>
        public static void Log(string message)
        {
            if (mng != null && !mng.visibleLog) return;
            Debug.Log(message);
        }

        /// <summary>
        /// �G���[���O�\��
        /// </summary>
        /// <param name="message"></param>
        public static void LogError(string message)
        {
            if (!mng.visibleLog) return;
            Debug.LogError(message);
        }

        /// <summary>
        /// �x�����O�\��
        /// </summary>
        /// <param name="message"></param>
        public static void LogWarning(string message)
        {
            if (!mng.visibleLog) return;
            Debug.LogWarning(message);
        }


        /// <summary>
        /// �o�^���ꂽ�֐����ꗗ�Ƃ��ăe�L�X�gFile�ɏ����o��
        /// </summary>
        public static void SaveFunctionList(DB_FunctionList db)
        {
            db.SaveFunctionList(functions);
        }
    }
}