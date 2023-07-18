using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using System.IO;
using Newtonsoft.Json;

//WinWin
public static class fg
{
    public static byte saveMax = 30;
    public static byte itemMax = 60;
    public static int moneyMax = 1000000000;
    public static int valueMax = 9999;
    public static float quality = 0.004f;
    public static short damageMax = 10000;
    public enum dir
    {
        up, down, right, left, up_right, up_left, down_right, down_left
    }

    /// <summary>
    /// 解像度
    /// </summary>
    public static int[] screenW = new int[] { 1920, 1600, 1280, 960 };
    public static int[] screenH = new int[] { 1080, 900, 720, 540 };

    /*ベクトルから角度を取得*/
    //ベクトルからラジアン
    public static float ToRad(this Vector3 vector)
    {
        return Mathf.Atan2(vector.z, vector.x);
    }
    public static float ToDeg(this Vector3 vector)
    {
        return Mathf.Atan2(vector.z, vector.x) * Mathf.Rad2Deg;
    }
    public static void ToRotation(this Transform tra, Vector3 rotation)
    {
        tra.transform.rotation = Quaternion.Euler(tra.transform.rotation.eulerAngles - rotation);
    }
    public static Quaternion ToRotation(this Vector3 rotation)
    {
        return Quaternion.Euler(rotation);
    }
    public static void SetRotation(this Transform tra, Vector3 rotation)
    {
        tra.transform.rotation = Quaternion.Euler(rotation);
    }
    public static void set(this Text txt, string a)
    {
        txt.text = a;
    }
    public static int Int(this string str)
    {
        return int.Parse(str);
    }
    public static byte Byte(this string str)
    {
        return byte.Parse(str);
    }

    public static float[] ToFloat(this string[] item, float value = 0)
    {
        return Array.ConvertAll(item, s =>
        {
            float tmp = 0;
            if (!float.TryParse(s, out tmp)) tmp = value;
            return tmp;
        });
    }

    public static string ToSplitString(this Vector3 item)
    {
        return $"{item.x},{item.y},{item.z}";
    }

    public static string ToSplitStringOPT(this Vector3 item)
    {
        return $"{item.x:F1},{item.y:F1},{item.z:F1}";
    }

    public static Vector3 ToVector3(this string str)
    {
        var pos = str.Split(',');
        return new Vector3(float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2]));
    }

    /// <summary>
    /// フラグ確認
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public static bool GetFlag(this byte i, byte n)
    {
        if ((i & (1 << n - 1)) >= 1) return true;
        else return false;
    }
    public static bool GetFlag(this uint i, byte n)
    {
        if ((i & (1 << n - 1)) >= 1) return true;
        else return false;
    }
    public static bool GetFlag(this ulong i, byte n)
    {
        if ((i & (ulong)(1 << n - 1)) >= 1) return true;
        else return false;
    }
    public static byte SetFlag(this byte i, byte n, bool b)
    {
        if (b) return i |= (byte)(1 << n);
        else return i &= (byte)(~(1 << n));
    }
    public static uint SetFlag(this uint i, byte n, bool b)
    {
        if (b) return i |= (uint)(1 << n);
        else return i &= (uint)(~(1 << n));
    }
    public static ulong SetFlag(this ulong i, byte n, bool b)
    {
        if (b) return i |= (ulong)1 << n;
        else return i &= (ulong)(~(1 << n));
    }


    //回転系
    static Quaternion temp;
    static Quaternion temp2;
    //public static Quaternion Lerp(this Quaternion a, Vector3 b, out bool flag, float speed = 1)
    //{
    //    temp = Quaternion.LookRotation(b);
    //    temp2 = a * Quaternion.Inverse(temp);
    //    if (temp2.x < 0.0001f && temp2.y < 0.0001f && temp2.z < 0.0001f) flag = true;
    //    else flag = false;
    //    return Quaternion.Lerp(a, temp, Time.deltaTime * speed);
    //}
    public static Quaternion Lerp(this Quaternion a, Vector3 b, float speed = 1.0f)
    {
        return Quaternion.Lerp(a, Quaternion.LookRotation(b), Time.deltaTime * 60f * speed);
    }
    public static Quaternion Lerp(this Quaternion a, Quaternion b, float speed = 1.0f)
    {
        return Quaternion.Lerp(a, b, Time.deltaTime * speed * 60f);
    }
    public static Quaternion Slerp(this Quaternion a, Vector3 b, float speed = 1.0f)
    {
        return Quaternion.Slerp(a, Quaternion.LookRotation(b), Time.deltaTime * speed * 60f);
    }
    public static Quaternion Slerp(this Quaternion a, Quaternion b, float speed = 1.0f)
    {
        return Quaternion.Slerp(a, b, Time.deltaTime * speed * 60f);
    }
    public static Vector3 y0(this Vector3 pos)
    {
        pos.y = 0;

        return pos;
    }
    public static int LineCount(this string str)
    {
        return str.Split('\n').Length;
    }

    /*UI*/
    /// <summary>
    /// 上下の登録
    /// </summary>
    /// <param name="bt"></param>
    public static void SetNav(Button[] bt)
    {
        SetNavUD(bt[0], bt[bt.Length - 1], bt[1]);
        for (byte i = 1; i < bt.Length - 1; i++)
        {
            SetNavUD(bt[i], bt[i - 1], bt[i + 1]);
        }
        SetNavUD(bt[bt.Length - 1], bt[bt.Length - 2], bt[0]);
    }
    //bt ターゲット, bt1 上, bt2 下
    static Navigation nav;
    /// <summary>
    /// ボタンの遷移先を登録
    /// </summary>
    /// <param name="bt">ターゲット</param>
    /// <param name="bt1">上</param>
    /// <param name="bt2">下</param>
    public static void SetNavUD(Button bt, Button bt1, Button bt2)
    {
        nav = bt.navigation;
        nav.mode = Navigation.Mode.Explicit;
        nav.selectOnUp = bt1;
        nav.selectOnDown = bt2;
        bt.navigation = nav;
    }
    /// <summary>
    /// ボタンの遷移先を登録
    /// </summary>
    /// <param name="bt">ターゲット</param>
    /// <param name="bt1">左</param>
    /// <param name="bt2">右</param>
    public static void SetNavRL(Button bt, Button bt1, Button bt2)
    {
        nav = bt.navigation;
        nav.mode = Navigation.Mode.Explicit;
        nav.selectOnLeft = bt1;
        nav.selectOnRight = bt2;
        bt.navigation = nav;
    }
    /// <summary>
    /// 格子状に登録
    /// </summary>
    /// <param name="bt"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public static void SetNavGrid(Button[] bt, byte width, byte height)
    {
        for (byte index = 0; index < bt.Length; index++)
        {
            if (!bt[index].gameObject.activeSelf) break;
            Navigation nav = bt[index].navigation;
            nav.mode = Navigation.Mode.Explicit;
            /*上*/
            if (index >= width) nav.selectOnUp = bt[index - width];
            /*下*/
            if (bt[index + width].gameObject.activeSelf) nav.selectOnDown = bt[index + width];
            /*左*/
            if (index != 0) nav.selectOnLeft = bt[index - 1];
            /*右*/
            if (bt[index + 1].gameObject.activeSelf) nav.selectOnRight = bt[index + 1];
            else
            {
                //最後のアイテムだった時の処理
                nav.selectOnRight = bt[0];
                Navigation nav2;
                //1行目は何もしない
                if (index >= width)
                {
                    //同じ行内にあるボタンの設定を変える
                    nav.selectOnDown = bt[index % width];
                    for (byte i = 0; i < index % width + 1; i++)
                    {
                        //同じ行のボタンの下を登録
                        if (i > 0)
                        {
                            nav2 = bt[index - i].navigation;
                            nav2.selectOnDown = bt[(index - i) % width];
                            bt[index - i].navigation = nav2;            //ボタンに登録
                        }
                        // 1行目のボタンの上を登録
                        nav2 = bt[(index - i) % width].navigation;
                        //index 0の場合
                        if ((index - i) % width == 0) nav2.selectOnLeft = bt[index];    //最後の場所を登録
                        nav2.selectOnUp = bt[index - i];
                        bt[(index - i) % width].navigation = nav2;  //ボタンに登録
                    }
                }
                else
                {
                    nav2 = bt[0].navigation;
                    nav2.selectOnLeft = bt[index];
                    bt[0].navigation = nav2;
                }
            }
            /*ボタンに登録して終了*/
            bt[index].navigation = nav;
        }
    }
    public static void SetNavVertical(Button[] bt)
    {
        byte i = 1;
        for (; i < bt.Length - 1; i++)
        {
            if (!bt[i + 1].gameObject.activeSelf) break;
            SetNavUD(bt[i], bt[i - 1], bt[i + 1]);
        }
        SetNavUD(bt[0], bt[i], bt[1]);
        SetNavUD(bt[i], bt[i - 1], bt[0]);
    }
    public static void SetNavHorizontal(Button[] bt)
    {
        byte i = 1;
        for (; i < bt.Length - 1; i++)
        {
            if (!bt[i + 1].gameObject.activeSelf) break;
            SetNavRL(bt[i], bt[i - 1], bt[i + 1]);
        }
        SetNavRL(bt[0], bt[i], bt[1]);
        SetNavRL(bt[i], bt[i - 1], bt[0]);
    }
    public static int WeightRandom(int[] rate)
    {
        int max = 0;
        //トータルを計算
        for (byte i = 0; i < rate.Length; i++)
        {
            max += rate[i];
        }
        var rnd = UnityEngine.Random.Range(0, max);
        for (sbyte i = 0; i < rate.Length; i++)
        {
            if (rnd < rate[i])
            {
                return i;   //決定
            }
            rnd -= rate[i];
        }
        return -1;
    }
    public static int Add(this string[] text, string addText)
    {
        for (int i = 0; i < text.Length; i++)
        {
            if (string.IsNullOrEmpty(text[i])) { text[i] = addText; return i; }
        }
        return -1;
    }
    public static int Add(this int[] list, int add)
    {
        for (int i = 0; i < list.Length; i++)
        {
            if (list[i] == 0) { list[i] = add; return i; }
        }
        return -1;
    }
    public static int Find(this int[] list, int add)
    {
        for (int i = 0; i < list.Length; i++)
        {
            if (list[i] == add) { return i; }
        }
        return -1;
    }
    public static string[] Remove(this string[] text, string removeText)
    {
        List<string> newText = new List<string>();
        int i = 0;
        int strFlag = 0;
        bool doOnece = false;
        foreach (var str in text)
        {
            i = newText.Count - 1;
            if (str == "\"")
            {
                strFlag++;
                if (strFlag == 2) strFlag = 0;//文字列の読み取りの終了
                else
                {
                    //文字列の読み取りの開始
                    newText.Add("");
                }
                doOnece = false;//最初に要素が空があるのでスキップ
                continue;
            }
            if (strFlag == 1)
            {
                //文字列の読み取り中
                if (doOnece && str == "") newText[i] += " ";
                else newText[i] += str;
                doOnece = true;
            }
            else if (!string.IsNullOrEmpty(str))
            {
                newText.Add(str);
            }
        }
        return newText.ToArray();
    }
    public static void Remove(this int[] item, int index)
    {
        //削除
        item[index] = 0;
        //詰める
        for (int j = index; j + 1 < item.Length; j++)
        {
            if (item[j + 1] == 0) return;
            item[j] = item[j + 1];
            item[j + 1] = 0;
        }
    }

    public static float Move(this float value, float newValue, float speed = 1.0f)
    {
        //if (value < newPos) value += (newPos - value) * speed;
        //else if (value > newPos) value -= (value - newPos) * speed;
        //return value;

        if (speed > 1f) speed = 1f;
        var p = (newValue - value) * (speed * Time.deltaTime * 60f);
        return value + p;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="newPos"></param>
    /// <param name="speed">0 ~ 1</param>
    /// <returns></returns>
    public static Vector3 Move(this Vector3 pos, Vector3 newPos, float speed = 1.0f)
    {
        if (speed > 1f) speed = 1f;
        var p = (newPos - pos) * (speed * Time.deltaTime * 60f);
        return pos + p;

        //if (pos == newPos) return pos;

        //var t = Mathf.Clamp(Time.deltaTime * posSpeed, 0, 1);
        //return Vector3.Lerp(pos, newPos, t);

        //posSpeed *= Time.deltaTime * 120f;
        //if (pos.x < newPos.x) pos.x += (newPos.x - pos.x) * posSpeed;
        //else if (pos.x > newPos.x) pos.x -= (pos.x - newPos.x) * posSpeed;

        //if (pos.y < newPos.y) pos.y += (newPos.y - pos.y) * posSpeed;
        //else if (pos.y > newPos.y) pos.y -= (pos.y - newPos.y) * posSpeed;

        //if (pos.z < newPos.z) pos.z += (newPos.z - pos.z) * posSpeed;
        //else if (pos.z > newPos.z) pos.z -= (pos.z - newPos.z) * posSpeed;
        //return pos;
    }

    public delegate void OnEvent(int i);
    public static void Select(string[] select, OnEvent OnClick, Transform parent, Button prefab, UI_SetNavigation setNavigation = null)
    {
        //初期化(ボタンを全て削除)
        foreach (Transform child in parent)
        {
            //if (child.gameObject.name == "Text") continue;
            UnityEngine.Object.Destroy(child.gameObject);
        }
        Button[] buttons = new Button[select.Length];
        for (byte i = 0; i < select.Length; i++)
        {
            buttons[i] = UnityEngine.Object.Instantiate(prefab, parent);
            buttons[i].transform.GetChild(0).GetComponent<Text>().text = select[i];
            byte n = i;
            buttons[i].onClick.AddListener(() => OnClick(n));
        }
        if (setNavigation != null) setNavigation.SetNavVertical(buttons);
        buttons[0].Select();
    }
    //子階層からタグを探す
    public static Transform FindWithTagFromChild(Transform root, string tag)
    {
        Debug.Log(tag);
        foreach (Transform tra in root.GetComponentsInChildren<Transform>())
            if (tra.CompareTag(tag)) return tra;

        return null;
    }
    public static void ResetTransform(this Transform transform)
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    /// <summary>
    /// 子要素全削除
    /// </summary>
    /// <param name="parent"></param>
    public static void DestroyChildren(this Transform parent)
    {
        foreach (Transform child in parent)
        {
            UnityEngine.Object.Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Fileをbyte配列に変換する
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static byte[] ConvertToByte(string path)
    {
        if (!File.Exists(path)) return null;
        var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        byte[] byteData = new byte[fs.Length];
        fs.Read(byteData, 0, byteData.Length);
        fs.Close();

        return byteData;
    }

    /// <summary>
    /// byte配列をFileに変換する
    /// </summary>
    /// <param name="path"></param>
    /// <param name="byteData"></param>
    public static void ConvertToFile(string path, byte[] byteData)
    {
        var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        fs.Write(byteData, 0, byteData.Length);
        fs.Close();
    }

    /// <summary>
    /// 座標からChunk取得
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="divideSize"></param>
    /// <returns></returns>
    public static (int, int) GetChunk2(Vector3 pos, float divideSize)
    {
        int x = (int)(pos.x * divideSize);
        int z = (int)(pos.z * divideSize);

        if (pos.x < 0) x--;
        if (pos.z < 0) z--;

        return (x, z);
    }

    /// <summary>
    /// If key already exists, overwrite it
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="self"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void ForceAdd<T1, T2>(this Dictionary<T1, T2> self, T1 key, T2 value)
    {
        if (!self.ContainsKey(key))
        {
            self.Add(key, value);
        }
        else
        {
            self[key] = value;
        }
    }

    /// <summary>
    /// Dictionary型を文字列に変換する
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="self"></param>
    /// <returns></returns>
    public static string GetString<T1, T2>(this Dictionary<T1, T2> self)
    {
        return JsonConvert.SerializeObject(self);
    }

    public static Dictionary<T1, T2> GetDict<T1, T2>(this string self)
    {
        if (string.IsNullOrEmpty(self)) return new Dictionary<T1, T2>();

        return JsonConvert.DeserializeObject<Dictionary<T1, T2>>(self);
    }

    public static Dictionary<T1, T2> ToDictionary<T1, T2>(this object model)
    {
        var serializedModel = JsonConvert.SerializeObject(model);
        return JsonConvert.DeserializeObject<Dictionary<T1, T2>>(serializedModel);
    }

    /// <summary>
    /// 文字列の長さを制限する
    /// </summary>
    /// <param name="text"></param>
    /// <param name="maxLength"></param>
    /// <returns></returns>
    public static string TruncateText(this string text, int maxLength)
    {
        if (text.Length > maxLength)
        {
            text = text.Substring(0, maxLength);
            text += "...";
        }
        return text;
    }
}