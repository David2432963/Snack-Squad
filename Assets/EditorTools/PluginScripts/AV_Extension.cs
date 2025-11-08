using System.Collections.Generic;
using UnityEngine;

namespace AV
{
    public static class AV_Extension
    {
        public static T RandomItem<T>(this T[] array)
        {
            return array[Random.Range(0, array.Length)];
        }

        public static T RandomItem<T>(this List<T> list)
        {
            return list[Random.Range(0, list.Count)];
        }

        public static List<T> RandomListNotDuplicateItem<T>(this List<T> list, int amount)
        {
            List<T> res = new List<T>();
            List<int> tmp = new List<int>();

            for (var i = 0; i < list.Count; i++) tmp.Add(i);
            for (var i = 0; i < amount; i++) res.Add(list[tmp.RandomItem()]);
            return res;
        }
        public static List<T> RandomListCanDuplicateItem<T>(this List<T> list, int amount)
        {
            List<T> res = new List<T>();
            for (var i = 0; i < amount; i++) res.Add(list.RandomItem());
            return res;
        }

        public static T FindMax<T>(this List<T> list) where T : System.IComparable
        {
            T max = list[0];
            for (var i = 1; i < list.Count; i++)
            {
                if (list[i].CompareTo(max) > 0)
                {
                    max = list[i];
                }
            }
            return max;
        }
        public static T FindMax<T>(this T[] list) where T : System.IComparable
        {
            T max = list[0];
            for (var i = 1; i < list.Length; i++)
            {
                if (list[i].CompareTo(max) > 0)
                {
                    max = list[i];
                }
            }
            return max;
        }
        public static T FindMin<T>(this List<T> list) where T : System.IComparable
        {
            T min = list[0];
            for (var i = 1; i < list.Count; i++)
            {
                if (list[i].CompareTo(min) < 0)
                {
                    min = list[i];
                }
            }
            return min;
        }
        public static T FindMin<T>(this T[] array) where T : System.IComparable
        {
            T min = array[0];
            for (var i = 1; i < array.Length; i++)
            {
                if (array[i].CompareTo(min) < 0)
                {
                    min = array[i];
                }
            }
            return min;
        }

        public static float DistanceX(this Transform trsThis, Transform trs2)
        {
            return Mathf.Abs(trsThis.position.x - trs2.position.x);
        }
        public static float DistanceY(this Transform trsThis, Transform trs2)
        {
            return Mathf.Abs(trsThis.position.y - trs2.position.y);
        }
    }


}