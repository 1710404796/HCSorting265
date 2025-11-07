using HBYWCZP2024.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System;

namespace HBYWCZP2024.View.CZPallet
{
 public partial class StationLevelConfigWindow : Window
 {
 public ObservableCollection<StationLevelMapItem> Datas { get; set; }

 public StationLevelConfigWindow()
 {
 InitializeComponent();
 DataContext = this;

 Datas = new ObservableCollection<StationLevelMapItem>(MainStorage.Saves.StationLevelMap);
 }

 private void Add_Click(object sender, RoutedEventArgs e)
 {
 Datas.Add(new StationLevelMapItem());
 }

 private void Remove_Click(object sender, RoutedEventArgs e)
 {
 if (Grid.SelectedItem is StationLevelMapItem item)
 {
 Datas.Remove(item);
 }
 }

 private void Up_Click(object sender, RoutedEventArgs e)
 {
 var idx = Grid.SelectedIndex;
 if (idx >0)
 {
 var item = Datas[idx];
 Datas.RemoveAt(idx);
 Datas.Insert(idx -1, item);
 Grid.SelectedIndex = idx -1;
 }
 }

 private void Down_Click(object sender, RoutedEventArgs e)
 {
 var idx = Grid.SelectedIndex;
 if (idx >=0 && idx < Datas.Count -1)
 {
 var item = Datas[idx];
 Datas.RemoveAt(idx);
 Datas.Insert(idx +1, item);
 Grid.SelectedIndex = idx +1;
 }
 }

 private void Save_Click(object sender, RoutedEventArgs e)
 {
 // Validate: station unique and filled
 if (Datas.Any(d => d.Station <=0))
 {
 MessageBox.Show("存在未填写或无效的工位号，请填写正整数工位号。");
 return;
 }
 if (Datas.Any(d => d.Level <0))
 {
 MessageBox.Show("存在未填写或无效的等级，请填写非负整数。");
 return;
 }

 var dupStation = Datas
 .GroupBy(d => d.Station)
 .FirstOrDefault(g => g.Count() >1);
 if (dupStation != null)
 {
 MessageBox.Show($"工位号 {dupStation.Key} 重复，请确保一个工位只配置一种等级。");
 return;
 }

 MainStorage.Saves.StationLevelMap = Datas.ToList();
 MainStorage.SaveManager.Save();
 DialogResult = true;
 Close();
 }
 }
}
