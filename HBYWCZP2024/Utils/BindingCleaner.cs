using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace HBYWCZP2024.Utils
{
    /// <summary>
    /// 页面/控件释放时统一清理 DataContext 和整棵元素树的 Binding，
    /// 避免旧页面继续持有 ViewModel、业务对象或事件源。
    /// </summary>
    public static class BindingCleaner
    {
        public static void Clear(DependencyObject root)
        {
            if (root == null)
            {
                return;
            }

            System.Collections.Generic.HashSet<DependencyObject> visited = [];
            System.Collections.Generic.Queue<DependencyObject> queue = new();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                DependencyObject current = queue.Dequeue();
                if (!visited.Add(current))
                {
                    continue;
                }

                BindingOperations.ClearAllBindings(current);
                if (current is FrameworkElement element)
                {
                    element.DataContext = null;
                }

                if (current is Visual)
                {
                    int visualCount = VisualTreeHelper.GetChildrenCount(current);
                    for (int i = 0; i < visualCount; i++)
                    {
                        if (VisualTreeHelper.GetChild(current, i) is DependencyObject child)
                        {
                            queue.Enqueue(child);
                        }
                    }
                }

                foreach (object child in LogicalTreeHelper.GetChildren(current))
                {
                    if (child is DependencyObject dependencyObject)
                    {
                        queue.Enqueue(dependencyObject);
                    }
                }
            }
        }
    }
}
