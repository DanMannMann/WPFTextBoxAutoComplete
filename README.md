WPFTextBoxAutoComplete
======================

An attached behavior for WPF's TextBox control that provides auto-completion suggestions from a given collection. Forked to add a strong name so it can be used in a project where strong names are required.

## How to use this library:

1. Add a reference to the library in your view

	``` csharp
		xmlns:behaviors="clr-namespace:WPFTextBoxAutoComplete;assembly=WPFTextBoxAutoComplete"
	```
	
2. Create a textbox and bind the "AutoCompleteItemsSource" to a collection of ```IEnumerable<String>```

	``` csharp
		<TextBox 
			Width="250"
			HorizontalAlignment="Center"
			Text="{Binding TestText, UpdateSourceTrigger=PropertyChanged}" 
			behaviors:AutoCompleteBehavior.AutoCompleteItemsSource="{Binding TestItems}" 
		/>
	```
	
3. (Optional) Set the "AutoCompleteStringComparison" property, which is of type <a href='https://msdn.microsoft.com/en-us/library/system.stringcomparison(v=vs.110).aspx'>StringComparison</a>

	``` csharp
		<TextBox 
			Width="250"
			HorizontalAlignment="Center"
			Text="{Binding TestText, UpdateSourceTrigger=PropertyChanged}" 
			behaviors:AutoCompleteBehavior.AutoCompleteItemsSource="{Binding TestItems}" 
			behaviors:AutoCompleteBehavior.AutoCompleteStringComparison="InvariantCultureIgnoreCase"
		/>
	```
    
Now, every time the "TestText" property of your datacontext is changed, WPFTextBoxAutoComplete will provide you with auto-completion suggestions.  To accept a suggestion, just hit "enter".

		
