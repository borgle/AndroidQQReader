using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace io.borgle.Controls.Helper
{
    public class DataGridColumnHelper
    {
        #region 动态列模式依赖属性
        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static object GetColumnsSource(DependencyObject obj)
        {
            return (object)obj.GetValue(ColumnsSourceProperty);
        }
        public static void SetColumnsSource(DependencyObject obj, object value)
        {
            obj.SetValue(ColumnsSourceProperty, value);
        }
        // Using a DependencyProperty as the backing store for ColumnsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnsSourceProperty =
            DependencyProperty.RegisterAttached("ColumnsSource",
                typeof(object), typeof(DataGridColumnHelper), new UIPropertyMetadata(null, ColumnsSourceChanged));
        #endregion

        #region 依赖属性发生变化时候的事件
        private static void ColumnsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            DataGrid gridView = obj as DataGrid;
            if (gridView != null)
            {
                gridView.Columns.Clear();

                if (e.OldValue != null)
                {
                    ICollectionView view = CollectionViewSource.GetDefaultView(e.OldValue);
                    if (view != null)
                        RemoveHandlers(gridView, view);
                }

                if (e.NewValue != null)
                {
                    ICollectionView view = CollectionViewSource.GetDefaultView(e.NewValue);
                    if (view != null)
                    {
                        AddHandlers(gridView, view);
                        CreateColumns(gridView, view);
                    }
                }
            }
        }
        #endregion


        private static IDictionary<ICollectionView, List<DataGrid>> _gridViewsByColumnsSource =
            new Dictionary<ICollectionView, List<DataGrid>>();
        private static List<DataGrid> GetGridViewsForColumnSource(ICollectionView columnSource)
        {
            List<DataGrid> gridViews;
            if (!_gridViewsByColumnsSource.TryGetValue(columnSource, out gridViews))
            {
                gridViews = new List<DataGrid>();
                _gridViewsByColumnsSource.Add(columnSource, gridViews);
            }
            return gridViews;
        }

        private static void AddHandlers(DataGrid gridView, ICollectionView view)
        {
            var gridViews = GetGridViewsForColumnSource(view);
            if (gridViews.Count == 0)
            {
                //同一个CollectionView只添加一次事件
                view.CollectionChanged += OnCollectionViewChanged;
            }
            gridViews.Add(gridView);
        }
        private static void RemoveHandlers(DataGrid gridView, ICollectionView view)
        {
            view.CollectionChanged -= OnCollectionViewChanged;
            GetGridViewsForColumnSource(view).Remove(gridView);
        }

        private static void CreateColumns(DataGrid gridView, ICollectionView view)
        {
            foreach (var item in view)
            {
                DataGridTemplateColumn column = CreateColumn(gridView, item);
                gridView.Columns.Add(column);
            }
        }
        private static DataGridTemplateColumn CreateColumn(DataGrid gridView, object columnSource)
        {
            DataGridTemplateColumn column = new DataGridTemplateColumn();
            var info = columnSource as ColumnInfo;

            if (!string.IsNullOrEmpty(info.HeaderText))
            {
                column.Header = info.HeaderText;
            }
            if (info.DefaultWidth > 0)
            {
                column.Width = info.DefaultWidth;
            }
            if (string.IsNullOrEmpty(info.SortMemberPath))
            {
                column.SortMemberPath = info.DisplayMemberPath;
            }
            else
            {
                column.SortMemberPath = info.SortMemberPath;
                column.SortDirection = info.SortDirection;
            }


            DataTemplate template = new DataTemplate();
            FrameworkElementFactory factory = new FrameworkElementFactory(typeof(TextBlock));
            factory.SetValue(TextBlock.HorizontalAlignmentProperty, info.HorizontalAlignment);
            if (info.Foreground != null)
            {
                factory.SetValue(TextBlock.ForegroundProperty, info.Foreground);
            }
            if (info.ForegroundTriggerParameters != null && !string.IsNullOrEmpty(info.ForegroundMemberPath))
            {
                foreach (var o in info.ForegroundTriggerParameters)
                {
                    var trigger = new DataTrigger();
                    trigger.Binding = new Binding(info.ForegroundMemberPath);
                    trigger.Value = o.Key;
                    var setter = new Setter();
                    setter.Property = TextBlock.ForegroundProperty;
                    setter.Value = o.Value;
                    trigger.Setters.Add(setter);
                    template.Triggers.Add(trigger);
                }
            }

            var binding = new Binding(info.DisplayMemberPath);
            if (!string.IsNullOrEmpty(info.StringFormat))
            {
                binding.StringFormat = string.Format("{{0:{0}}}", info.StringFormat);
            }
            if (info.Converter != null)
            {
                binding.Converter = info.Converter;
                binding.ConverterParameter = info.ConverterParameter;
            }
            factory.SetBinding(TextBlock.TextProperty, binding);
            template.VisualTree = factory;

            column.CellTemplate = template;

            return column;
        }
        /// <summary>
        /// 当列对象发生变化时的事件
        /// </summary>
        private static void OnCollectionViewChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ICollectionView view = sender as ICollectionView;
            var gridViews = GetGridViewsForColumnSource(view);
            if (gridViews == null || gridViews.Count == 0)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var gridView in gridViews)
                    {
                        for (int i = 0; i < e.NewItems.Count; i++)
                        {
                            var column = CreateColumn(gridView, e.NewItems[i]);
                            gridView.Columns.Insert(e.NewStartingIndex + i, column);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    foreach (var gridView in gridViews)
                    {
                        var columns = new List<DataGridColumn>();
                        for (int i = 0; i < e.OldItems.Count; i++)
                        {
                            var column = gridView.Columns[e.OldStartingIndex + i];
                            columns.Add(column);
                        }
                        for (int i = 0; i < e.NewItems.Count; i++)
                        {
                            var column = columns[i];
                            gridView.Columns.Insert(e.NewStartingIndex + i, column);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var gridView in gridViews)
                    {
                        for (int i = 0; i < e.OldItems.Count; i++)
                        {
                            gridView.Columns.RemoveAt(e.OldStartingIndex);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (var gridView in gridViews)
                    {
                        for (int i = 0; i < e.NewItems.Count; i++)
                        {
                            var column = CreateColumn(gridView, e.NewItems[i]);
                            gridView.Columns[e.NewStartingIndex + i] = column;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (var gridView in gridViews)
                    {
                        gridView.Columns.Clear();
                        CreateColumns(gridView, sender as ICollectionView);
                    }
                    break;
                default:
                    break;
            }
        }

        public class ColumnInfo
        {
            public string HeaderText { get; set; }
            public string DisplayMemberPath { get; set; }
            public string StringFormat { get; set; }
            public string SortMemberPath { get; set; }
            public Double DefaultWidth { get; set; }
            public HorizontalAlignment HorizontalAlignment { get; set; }
            public SolidColorBrush Foreground { get; set; }
            public IValueConverter Converter { get; set; }
            public object ConverterParameter { get; set; }
            public ListSortDirection? SortDirection { get; set; }
            public string ForegroundMemberPath { get; set; }
            public Dictionary<char, SolidColorBrush> ForegroundTriggerParameters { get; set; }
        }
    }
}