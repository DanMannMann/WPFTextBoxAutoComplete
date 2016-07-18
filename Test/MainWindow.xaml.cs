using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Test
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		public MainWindow()
		{
			InitializeComponent();
			CategoryCombo.KeyDown += CategoryCombo_KeyDown;
			CategoryCombo2.SuggestionList = new ObservableCollection<object>();
			CategoryCombo2.SuggestionList.Add("2Item 1");
			CategoryCombo2.SuggestionList.Add("2Item 2");
			CategoryCombo2.SuggestionList.Add("2Item 3");
			CategoryCombo2.SuggestionList.Add("2Item 4");
			CategoryCombo2.SuggestionList.Add("2Item 5");
			CategoryCombo2.SuggestionList.Add("2Item 6");
		}

		private void CategoryCombo_KeyDown(object sender, KeyEventArgs e)
		{
			if (CategoryCombo.SuggestionList.Count < 12)
			{
				CategoryCombo.SuggestionList.Add("Item 1");
				CategoryCombo.SuggestionList.Add("Item 2");
				CategoryCombo.SuggestionList.Add("Item 3");
			}
		}
	}
}
