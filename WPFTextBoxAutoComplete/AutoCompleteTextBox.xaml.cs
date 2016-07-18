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

		public double MaxDropdownHeight
		{
			get { return (double)GetValue(MaxDropdownHeightProperty); }
			set { SetValue(MaxDropdownHeightProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MaxDropdownHeight.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MaxDropdownHeightProperty =
			DependencyProperty.Register("MaxDropdownHeight", typeof(double), typeof(AutoCompleteTextBox), new PropertyMetadata(double.MaxValue));


		// Dependency Property
		public static readonly DependencyProperty InternalFilterProperty =
			 DependencyProperty.Register("InternalFilter", typeof(bool),
			 typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(true));

		// .NET Property wrapper
		public bool InternalFilter
		{
			get { return (bool)GetValue(InternalFilterProperty); }
			set { SetValue(InternalFilterProperty, value); }
		}

		// Dependency Property
		public static readonly DependencyProperty SelectorTypeProperty =
             DependencyProperty.Register("SelectorType", typeof(Type),
             typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null));

        // .NET Property wrapper
        public Type SelectorType
        {
            get { return (Type)GetValue(SelectorTypeProperty); }
            set { SetValue(SelectorTypeProperty, value); }
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

		private static event EventHandler<Tuple<ObservableCollection<object>, ObservableCollection<object>>> SuggestionListChanged;
		private static event PropertyChangedCallback UsedSuggestionChanged;

		// Dependency Property
		public static readonly DependencyProperty SuggestionListProperty =
             DependencyProperty.Register("SuggestionList", typeof(ObservableCollection<object>),
             typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(new ObservableCollection<object>(), new PropertyChangedCallback((DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
			 {
				 SuggestionListChanged.Invoke(sender, new Tuple<ObservableCollection<object>, ObservableCollection<object>>(e.OldValue as ObservableCollection<object>, e.NewValue as ObservableCollection<object>));
			 })));

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

		// Dependency Property
		public static readonly DependencyProperty UsedSuggestionProperty =
			 DependencyProperty.Register("UsedSuggestion", typeof(object),
			 typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null, new PropertyChangedCallback((o,e) =>
			 {
				 UsedSuggestionChanged.Invoke(o, e);
			 })));

		public object UsedSuggestion
		{
			get { return GetValue(UsedSuggestionProperty); }
			set { SetValue(UsedSuggestionProperty, value); }
		}

		// Dependency Property
		public static readonly DependencyProperty SelectedItemProperty =
			 DependencyProperty.Register("SelectedItem", typeof(object),
			 typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null));

		public object SelectedItem
		{
			get { return GetValue(SelectedItemProperty); }
			set { SetValue(SelectedItemProperty, value); }
		}

		// Dependency Property
		public static readonly DependencyProperty QueryTextProperty =
			 DependencyProperty.Register("QueryText", typeof(string),
			 typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null));
		private string _previousText;

		public string QueryText
		{
			get { return (string)GetValue(QueryTextProperty); }
			set { SetValue(QueryTextProperty, value); }
		}

		public AutoCompleteTextBox()
        {
            InitializeComponent();
			SuggestionList.CollectionChanged += suggestionsChanged;
			SuggestionListChanged += AutoCompleteTextBox_SuggestionListChanged;
			UsedSuggestionChanged += AutoCompleteTextBox_UsedSuggestionChanged;
		}

		private void AutoCompleteTextBox_UsedSuggestionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d == this)
			{
				//inputText.Text = UsedSuggestion == null ? string.Empty : (UsedSuggestion as Suggestion).Item;
				
				if (UsedSuggestion != null)
				{
					inputText.CaretIndex = (UsedSuggestion as Suggestion).Item.Length;
					SelectedItem = (UsedSuggestion as Suggestion).ItemObject;
				}
				//Suggestions.Clear();
				//Suggestions = Suggestions;
			}
		}

		private void AutoCompleteTextBox_SuggestionListChanged(object sender, Tuple<ObservableCollection<object>, ObservableCollection<object>> e)
		{
			if (sender == this)
			{
				e.Item1.CollectionChanged -= suggestionsChanged;
				e.Item2.CollectionChanged += suggestionsChanged;
				UpdateSuggestions();
			}
		}

		private void suggestionsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			UpdateSuggestions();
		}

		private void UpdateSuggestions()
		{
			if (!InternalFilter)
			{
				var suggestions = new ObservableCollection<Suggestion>();
				var currentItem = GetCurrent();
				SuggestionList.ForEach(x => suggestions.Add(new Suggestion() { Item = Select(x), ItemObject = x, Selected = (currentItem == null ? string.Empty : currentItem.Item) == Select(x) }));
				SetValue(SuggestionsProperty, suggestions);
			}
		}

		public AutoCompleteTextBox(IEnumerable<string> suggestionList) : this()
        {
            SuggestionList.Clear();
            foreach(var x in suggestionList)
            {
                SuggestionList.Add(x);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
			QueryText = inputText.Text;
			if (InternalFilter && _previousText != inputText.Text && (UsedSuggestion as Suggestion)?.Item != inputText.Text)
			{
				_previousText = inputText.Text;
				var candidates = SuggestionList.Where(x => Select(x).ApproximatelyEquals((sender as TextBox).Text, FuzzyStringComparisonTolerance.Strong,
					FuzzyStringComparisonOptions.UseLongestCommonSubstring,
					FuzzyStringComparisonOptions.UseLongestCommonSubsequence))
					.OrderByDescending(x => Select(x).LongestCommonSubsequence((sender as TextBox).Text).Length + Select(x).LongestCommonSubstring((sender as TextBox).Text).Length)
					.ToList();
				var suggestions = new ObservableCollection<Suggestion>();
				var current = GetCurrent();
				var currentItem = current == null ? string.Empty : current.Item;

				candidates.ForEach(x => suggestions.Add(new Suggestion() { Item = Select(x), ItemObject = x, Selected = (current == null ? string.Empty : current.Item) == Select(x) }));
				UsedSuggestion = suggestions.SingleOrDefault(x => x.Selected);
				if ((UsedSuggestion as Suggestion)?.ItemObject != null)
				{
					SelectedItem = (UsedSuggestion as Suggestion).ItemObject;
				}
				SetValue(SuggestionsProperty, suggestions);
			}
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Key.Tab)
            {
                //Suggestions.Clear();
                //Suggestions = Suggestions;
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
					UsedSuggestion = current;
					//inputText.Text = current.Item;
					//UsedSuggestion = current.ItemObject;
					
				}
				//Suggestions.Clear();
				//Suggestions = Suggestions;
				TraversalRequest request = new TraversalRequest(FocusNavigationDirection.Next);
				inputText.MoveFocus(request);
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
			if (UsedSuggestion == null)
			{
				//inputText.Text = string.Empty;
			}
            //Suggestions = Suggestions;
			SuggGrid.Visibility = Visibility.Hidden;
		}

        private void AdornedControl_GotFocus(object sender, RoutedEventArgs e)
        {
			SuggGrid.Visibility = Visibility.Visible;
			inputText.Focus();
        }

        private void AdornedControl_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Keyboard.Focus(inputText);
        }

        private void AdornedControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Suggestions.Clear();
            //Suggestions = Suggestions;
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
