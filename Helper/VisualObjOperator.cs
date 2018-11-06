using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace io.borgle.Controls.Helper
{
    /// <summary>
    /// 视觉对象操作类
    /// </summary>
    public static class VisualObjOperator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static double GetDpiFactor(this Visual target)
        {
            var source = PresentationSource.FromVisual(target);
            return source == null ? 1.0 : 1 / source.CompositionTarget.TransformToDevice.M11;
        }

        /// <summary>
        /// 从一个可视化对象中找到符合名称的第一个子对象
        /// </summary>
        /// <typeparam name="T">子对象的类型</typeparam>
        /// <param name="obj">父对象</param>
        /// <param name="childName">子对象名称</param>
        /// <returns></returns>
        public static T FindFirstVisualChild<T>(this DependencyObject obj, string childName = null) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T && (child.GetValue(FrameworkElement.NameProperty).ToString() == childName || string.IsNullOrEmpty(childName)))
                {
                    return (T)child;
                }
                else
                {
                    T childOfChild = FindFirstVisualChild<T>(child, childName);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 从一个可视化对象中找到其子对象集合
        /// </summary>
        /// <typeparam name="T">子对象类型</typeparam>
        /// <param name="obj">父对象</param>
        /// <param name="name">子对象名称</param>
        /// <returns></returns>
        public static List<T> GetChildObjects<T>(this DependencyObject obj, string name = null) where T : FrameworkElement
        {
            DependencyObject child = null;
            List<T> childList = new List<T>();
            for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(obj) - 1; i++)
            {
                child = VisualTreeHelper.GetChild(obj, i);
                if (child is T && (((T)child).Name == name || string.IsNullOrEmpty(name)))
                {
                    childList.Add((T)child);
                }
                childList.AddRange(GetChildObjects<T>(child, ""));
            }
            return childList;
        }

        /// <summary>
        /// 找到该可视化对象的父对象
        /// </summary>
        /// <typeparam name="T">父对象类型</typeparam>
        /// <param name="obj">当前可视化对象</param>
        /// <returns></returns>
        public static T FindVisualParent<T>(this DependencyObject obj) where T : class
        {
            while (obj != null)
            {
                if (obj is T)
                    return obj as T;

                obj = VisualTreeHelper.GetParent(obj);
            }

            return null;
        }

        /// <summary>
        /// 递归获取找到该可视化对象的父树对象
        /// </summary>
        public static IEnumerable<DependencyObject> FindVisualParents(this DependencyObject obj)
        {
            while (true)
            {
                obj = VisualTreeHelper.GetParent(obj);
                if (obj == null) { break; }
                yield return obj;
            }

            yield break;
        }
    }
}
