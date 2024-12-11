using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using HBYWCZP2024.Attritubes;
using HBYWCZP2024.Utils;
using LanguageExt;
using Microsoft.IdentityModel.Logging;
using XCZZJC2024.Utils;

namespace HBYWCZP2024.View.User;

[PowerView(PowerEnum.权限配置表)]
public partial class PowerSettingWindows : Window
{
    public PowerSettingWindows()
    {
        InitializeComponent();
    }



    private Dictionary<PowerEnum, List<CheckBox>> _checkBoxesMap = [];
    private void PowerSettingWindows_OnLoaded(object sender, RoutedEventArgs e)
    {
        foreach (var pow in MainStorage.Saves.PowerTable)
        {
            if (typeof(PowerEnum).GetMember(pow.Key.ToString())[0].GetCustomAttribute<NotShowAttribute>() != null) continue;
            _checkBoxesMap[pow.Key] = [];
            StackPanel st = new StackPanel()
            {
                // Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            TextBlock powerText = new()
            {
                Text = Utils.Utils.GetPowerName(pow.Key), // pow.Key.ToString(),
                FontSize = 24,
                Margin = new Thickness(0, 10, 0, 0)
            };
            StackPanel st1 = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            foreach (var item in Enum.GetValues<RoleEnum>().SkipLast(1))
            {
                CheckBox checkBox = new CheckBox()
                {
                    IsChecked = pow.Value[item],
                    Content = Utils.Utils.GetRoleName(item)
                };
                _checkBoxesMap[pow.Key].Add(checkBox);
                st1.Children.Add(checkBox);
            }

            //for (int i = 0; i < pow.Value.Count; i++)
            //{
            //    CheckBox checkBox = new CheckBox()
            //    {
            //        IsChecked = pow.Value[i],
            //        Content = LoginHelper.GetPowerName(i + 1)
            //    };
            //    _checkBoxesMap[pow.Key].Add(checkBox);
            //    st1.Children.Add(checkBox);
            //}

            st.Children.Add(powerText);
            st.Children.Add(st1);
            PowerPanel.Children.Add(st);
        }
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        // 鏅�氬寲
        var aa = Enum.GetValues<RoleEnum>().SkipLast(1).ToList();
        foreach (var keyValuePair in _checkBoxesMap)
        {


            for (var i = 0; i < keyValuePair.Value.Count; i++)
            {
                MainStorage.Saves.PowerTable[keyValuePair.Key][aa[i]] = keyValuePair.Value[i].IsChecked ?? false;
            }
        }
        Close();
    }
}
