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
using FuzzyString;
using System.ComponentModel;

namespace WPFTextBoxAutoComplete
{
    /// <summary>
    /// Interaction logic for AutoCompleteTextBox.xaml
    /// </summary>
    public partial class AutoCompleteTextBox : AdornedControl.AdornedControl
    {
        private IStringSelector GetSelector()
        {
            if (SelectorType == null || !typeof(IStringSelector).IsAssignableFrom(SelectorType))
            {
                return new PassThruSelector();
            }
            else
            {
                return (IStringSelector)Activator.CreateInstance(SelectorType);
            }
        }

        private string Select(object input)
        {
            return GetSelector().Select(input);
        }

        // Dependency Property
        public static readonly DependencyProperty SelectorTypeProperty =
             DependencyProperty.Register("SelectorType", typeof(Type),
             typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null));

        // .NET Property wrapper
        public Type SelectorType
        {
            get { return (Type)GetValue(SelectorTypeProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Dependency Property
        public static readonly DependencyProperty TextProperty =
             DependencyProperty.Register("Text", typeof(string),
             typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(string.Empty));

        // .NET Property wrapper
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Dependency Property
        public static readonly DependencyProperty SuggestionListProperty =
             DependencyProperty.Register("SuggestionList", typeof(ObservableCollection<object>),
             typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(new ObservableCollection<object>()));

        // .NET Property wrapper
        public ObservableCollection<object> SuggestionList
        {
            get { return (ObservableCollection<object>)GetValue(SuggestionListProperty); }
            set { SetValue(SuggestionListProperty, value); }
        }

        private IEnumerable<string> StringSuggestionList
        {
            get
            {
                return SuggestionList.Select(x => Select(x));
            }
        }

        // Dependency Property
        public static readonly DependencyProperty SuggestionsProperty =
             DependencyProperty.Register("Suggestions", typeof(ObservableCollection<Suggestion>),
             typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(new ObservableCollection<Suggestion>()));

        // .NET Property wrapper
        public ObservableCollection<Suggestion> Suggestions
        {
            get { return (ObservableCollection<Suggestion>)GetValue(SuggestionsProperty); }
            set { SetValue(SuggestionsProperty, value); }
        }

        public object UsedSuggestion { get; private set; }

        public AutoCompleteTextBox()
        {
            InitializeComponent();
        }

        public AutoCompleteTextBox(IEnumerable<string> suggestionList)
        {
            InitializeComponent();
            SuggestionList.Clear();
            foreach(var x in suggestionList)
            {
                SuggestionList.Add(x);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var candidates = SuggestionList.Where(x => Select(x).ApproximatelyEquals((sender as TextBox).Text, FuzzyStringComparisonTolerance.Strong,
                FuzzyStringComparisonOptions.UseLongestCommonSubstring,
                FuzzyStringComparisonOptions.UseLongestCommonSubsequence))
                .OrderByDescending(x => Select(x).LongestCommonSubsequence((sender as TextBox).Text).Length + Select(x).LongestCommonSubstring((sender as TextBox).Text).Length)
                .ToList();
            var suggestions = new ObservableCollection<Suggestion>();
            var current = GetCurrent();
            var currentItem = current == null ? string.Empty : current.Item;
            candidates.ForEach(x => suggestions.Add(new Suggestion() { Item = Select(x), Selected = currentItem == Select(x) }));
            SetValue(SuggestionsProperty, suggestions); 
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Key.Tab)
            {
                Suggestions.Clear();
                Suggestions = Suggestions;
                return;
            }

            if (Suggestions.Count == 0)
            {
                return;
            }

            var current = GetCurrent();

            if (e.Key == Key.Enter)
            {
                if (current != null)
                {
                    inputText.Text = current.Item;
                    UsedSuggestion = current.ItemObject;
                    inputText.CaretIndex = inputText.Text.Length;
                }
                Suggestions.Clear();
                Suggestions = Suggestions;
                return;
            }

            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                var index = Suggestions.IndexOf(current);
                Suggestions.ForEach(x => x.Selected = false);
                switch (e.Key)
                {
                    case Key.Down:
                        index++;
                        if (index >= Suggestions.Count)
                        {
                            index = 0;
                        }
                        break;

                    case Key.Up:
                        index--;
                        if (index < 0)
                        {
                            index = Suggestions.Count - 1;
                        }
                        break;
                }
                if (index >= 0)
                {
                    Suggestions[index].Selected = true;
                }
            }
        }

        private Suggestion GetCurrent()
        {
            var current = Suggestions.FirstOrDefault(x => x.Selected);
            return current;
        }

        private void AdornedControl_LostFocus(object sender, RoutedEventArgs e)
        {
            Suggestions.Clear();
            Suggestions = Suggestions;
        }

        private void AdornedControl_GotFocus(object sender, RoutedEventArgs e)
        {
            inputText.Focus();
        }

        private void AdornedControl_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Keyboard.Focus(inputText);
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                var index = Suggestions.IndexOf(Suggestions.FirstOrDefault(x => x.Item == (sender as TextBlock).Text));
                Suggestions.ForEach(x => x.Selected = false);
                Suggestions[index].Selected = true;
            }
            else if (e.ClickCount == 2)
            {
                inputText.Text = (sender as TextBlock).Text;
                inputText.CaretIndex = inputText.Text.Length;
                Suggestions.Clear();
                Suggestions = Suggestions;
            }
        }

        private void AdornedControl_Loaded(object sender, RoutedEventArgs e)
        {
            Suggestions.Clear();
            Suggestions = Suggestions;
        }
    }

    public class Suggestion : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string _item;
        private bool _selected;
        private object _itemObject;

        public string Item
        {
            get
            {
                return _item;
            }
            set
            {
                _item = value;
                RaisePropertyChanged("Item");
            }
        }

        public object ItemObject
        {
            get
            {
                return _itemObject;
            }
            set
            {
                _itemObject = value;
                RaisePropertyChanged("ItemObject");
            }
        }

        public bool Selected
        {
            get
            {
                return _selected;
            }
            set
            {
                _selected = value;
                RaisePropertyChanged("Selected");
            }
        }

        private void RaisePropertyChanged(params string[] p)
        {
            if (PropertyChanged != null)
            {
                foreach(var prop in p)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(prop));
                }
            }
        }
    }

    public static class Extenions
    {
        public static void ForEach<T>(this ObservableCollection<T> input, Action<T> action)
        {
            foreach(var i in input)
            {
                action.Invoke(i);
            }
        }
    }
}
