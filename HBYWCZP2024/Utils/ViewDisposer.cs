using System;
using System.Windows;

namespace HBYWCZP2024.Utils
{
    /// <summary>
    /// 统一释放视图：调用 IDisposable，再清理 DataContext 和整棵元素树的绑定。
    /// </summary>
    public static class ViewDisposer
    {
        public static void Release(FrameworkElement? root, Action<Exception>? onError = null)
        {
            if (root == null)
            {
                return;
            }

            try
            {
                if (root is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
            }

            try
            {
                root.DataContext = null;
                BindingCleaner.Clear(root);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
            }
        }
    }
}
