using System;
using System.Collections;
using System.Security.Cryptography;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using DG.Tweening;

/// <summary>
///     Math Utils
///     General Utils
///     DateTime Utils
/// </summary>
namespace HP.Utils
{
    #region Math Utils
    public static class MathUtils
    {
        /// <summary>
        ///     Convert money from specific number to string ending with K with 2 decemal.
        ///     Paramerter factionNumber must be in range of [0, 3]
        /// </summary>
        public static string ConvertToK(int number, int fractionNumber = 0)
        {
            string str = string.Empty;
            float tempMoney;
            if (number >= 1000)
            {
                tempMoney = (float)number;
                tempMoney = (tempMoney / (Mathf.Pow(10, (3 - fractionNumber)))); //Output: 1233.21
                tempMoney = Mathf.Round(tempMoney); // Output: 1233
                tempMoney = fractionNumber == 0 ? tempMoney : tempMoney / Mathf.Pow(10, fractionNumber); // Output: 123.3
                str = $"{tempMoney}k";
            }
            else
            {
                str = number.ToString();
            }
            return str;
        }
        public static float Round(float number, int decimalPlaces = 0)
        {
            number *= Mathf.Pow(10, decimalPlaces);
            number = Mathf.Round(number);
            number /= Mathf.Pow(10, decimalPlaces);
            return number;
        }
        public static int ConvertToMultipleOfNumber(float num1, int num2)
        {
            int a = (int)num1 / num2;
            a *= num2;
            return a;
        }
        public static int ConvertToMultipleOfNumber(int num1, int num2)
        {
            int a = (int)num1 / num2;
            a *= num2;
            return a;
        }
        public static int RoundToTen(float number)
        {
            return Mathf.RoundToInt(number / 10f) * 10;
        }
        public static int RoundToTen(int number)
        {
            return Mathf.RoundToInt(number / 10f) * 10;
        }

        public static void QuickSort(int[] arr, int low, int high)
        {
            if (low < high)
            {
                int pivot = Partition(arr, low, high);
                QuickSort(arr, low, pivot - 1);
                QuickSort(arr, pivot + 1, high);
            }

            static int Partition(int[] arr, int low, int high)
            {
                int pivot = arr[high];
                int i = low - 1;
                for (int j = low; j < high; j++)
                {
                    if (arr[j] < pivot)
                    {
                        i++;
                        arr.Swap(i, j);
                    }
                }
                arr.Swap(i + 1, high);
                return i + 1;
            }
        }

        // For small and nearly sorted array
        public static void InsertionSort(int[] arr)
        {
            for (int i = 1; i < arr.Length; i++)
            {
                int key = arr[i];
                int j = i - 1;
                while (j >= 0 && arr[j] > key)
                {
                    arr[j + 1] = arr[j];
                    j--;
                }
                arr[j + 1] = key;
            }
        }

    }
    #endregion

    #region DateTime Utils
    public static class DateTimeUtils
    {
        public static bool CheckIfNewDay(DateTime yesterday, DateTime today)
        {
            TimeSpan timeSpan = today - yesterday;
            if (timeSpan.Days >= 1)
            {
                return true;
            }
            return false;
        }
        public static int CountDay(DateTime lastTime, DateTime _now)
        {
            return (_now - lastTime).Days;
        }
        public static int CountHour(DateTime lastTime, DateTime now)
        {
            return (now - lastTime).Hours;
        }
        public static int CountMinute(DateTime lastTime, DateTime now)
        {
            return (now - lastTime).Minutes;
        }
        public static int CountSecond(DateTime lastTime, DateTime now)
        {
            return (now - lastTime).Seconds;
        }
        public static int CountMillisecond(DateTime lastTime, DateTime now)
        {
            return (now - lastTime).Milliseconds;
        }

        public static DateTime ConvertStringToDateTime(string str)
        {
            DateTime.TryParse(str, out DateTime dateTime);
            return dateTime;
        }
    }
    #endregion

    #region Generic Utils
    public static class GenericUtils
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            var provider = new RNGCryptoServiceProvider();
            int n = list.Count;
            while (n > 1)
            {
                var box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                var k = (box[0] % n);
                n--;
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        public static void QuickShuffle<T>(this IList<T> list)
        {
            var rng = new Random();
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = rng.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        public static void Swap<T>(this List<T> list, int i, int j)
        {
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

        public static void Swap<T>(this T[] array, int i, int j)
        {
            T temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
        public static int GetLength<T>(this T _param)
        {
            string str = _param.ToString();
            return str.Length;
        }
        public static void LookAt2D(this Component t, Vector2 startDirection, Vector2 targetPos)
        {
            Vector2 dir = targetPos - (Vector2)t.transform.position;
            t.transform.rotation = FromToRotation(startDirection, dir.normalized);
        }
        public static Quaternion FromToRotation(Vector2 fromDirection, Vector2 toDirection)
        {
            var angle = Vector2.SignedAngle(fromDirection, toDirection);
            return Quaternion.Euler(0, 0, angle);
        }
        public static bool CompareLayer(this Component c, LayerMask layerMask)
        {
            return c.gameObject.CompareLayer(layerMask);
        }

        public static bool CompareLayer(this GameObject g, LayerMask layerMask)
        {
            return layerMask == (layerMask | (1 << g.layer));
        }
    }
    #endregion

    #region General Utils
    public static class GeneralUtils
    {
        /// <summary>
        /// Calculate the start position of the grid square that the make the grid centered
        /// </summary>
        /// <param name="_rows">Number of rows</param>
        /// <param name="_cols">Number of columns</param>
        /// <param name="_slotSizeX">Size X of 1 grid square</param>
        /// <param name="_slotSizeY">Size y of 1 grid square</param>
        /// <returns></returns>
        public static Vector2 SetUpStartSpawnPosOfGrid(int rows, int cols, float slotSizeX, float slotSizeY)
        {
            Vector2 startSpawnPos = new Vector2();
            if (rows % 2 == 0)
            {
                startSpawnPos.x = -(cols / 2f * slotSizeX - slotSizeX / 2);
            }
            else
            {
                startSpawnPos.x = -((cols - 1) / 2f * slotSizeX);
            }

            if (cols % 2 == 0)
            {
                startSpawnPos.y = rows / 2f * slotSizeY - slotSizeY / 2;
            }
            else
            {
                startSpawnPos.y = (rows - 1) / 2f * slotSizeY;
            }
            return startSpawnPos;
        }
        public static bool CheckIfListContainFullOfPair(List<int> list)
        {
            List<int> tempList = new List<int>(list);
            int index = 1;
            while (tempList.Count > 0)
            {
                if (tempList[0] == tempList[index])
                {
                    tempList.RemoveAt(index);
                    tempList.RemoveAt(0);
                    index = 1;
                }
                else if (index < tempList.Count - 1)
                {
                    index++;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Convert hexcode to Color
        /// </summary>
        private static Color HexToColor(string hex)
        {
            hex = hex.Replace("#", "");
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            return new Color(r, g, b, 1);
        }


        public static void TweenInt(int startValue, int endValue, int step, float delayTime, System.Action<int> cb = null)
        {
            int num = startValue;
            if (endValue < 0) endValue = 0;
            DOTween.To(() => num, x => num = x, endValue, step).SetDelay(delayTime).OnUpdate(() =>
            {
                cb?.Invoke(num);
            });
        }

        public static void TweenFloat(int startValue, int endValue, int step, float delayTime, System.Action<int> cb = null)
        {
            int num = startValue;
            if (endValue < 0) endValue = 0;
            DOTween.To(() => num, x => num = x, endValue, step).SetDelay(delayTime).OnUpdate(() =>
            {
                cb?.Invoke(num);
            });
        }

        /// <summary>
        ///     Shift the start value towards the end value without exceeding.
        /// </summary>
        public static float Approach(float start, float end, float shift)
        {
            return start < end ? Mathf.Min(start + shift, end) : Mathf.Max(start - shift, end);
        }

        /// <summary>
        ///     Shift the start angle towards the end angle (degrees).
        /// </summary>
        public static float ApproachAngle(float start, float end, float shift)
        {
            var deltaAngle = Mathf.DeltaAngle(start, end);
            if (-shift < deltaAngle && deltaAngle < shift)
            {
                return end;
            }

            return Mathf.Repeat(Approach(start, start + deltaAngle, shift), 360f);
        }

        /// <summary>
        ///     Shift the start value towards zero without exceeding.
        /// </summary>
        public static float Reduce(float start, float shift)
        {
            return Approach(start, 0f, shift);
        }

        /// <summary>
        ///     Shift the start angle towards zero (degrees).
        /// </summary>
        public static float ReduceAngle(float start, float shift)
        {
            return ApproachAngle(start, 0f, shift);
        }

        public static Vector3 GetMouseWorldPosition(Camera cam)
        {
            Vector3 mouseScreenPosition = Input.mousePosition;
            Vector3 mouseWorldPosition = cam.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, cam.transform.position.y));
            return mouseWorldPosition;
        }
        public static Vector3 GetEulerAngleFromVector(Vector2 vector)
        {
            float angle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
            return new Vector3(0, 0, angle);
        }
        public static float GetAngleFromVector(Vector2 vector)
        {
            return Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
        }
        public static Vector2 GetVectorFromAngle(float angle)
        {
            return new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        }
        #endregion
    }
}

