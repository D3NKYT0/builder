using System.Windows;
using System.Windows.Controls;

namespace UpdateBuilder.Controls
{
	public class BusyIndicator : Control
	{
		public static readonly DependencyProperty IsBusyProperty;

		public static readonly DependencyProperty TitleProperty;

		public bool IsBusy
		{
			get
			{
				return (bool)GetValue(IsBusyProperty);
			}
			set
			{
				SetValue(IsBusyProperty, value);
			}
		}

		public string Title
		{
			get
			{
				return (string)GetValue(TitleProperty);
			}
			set
			{
				SetValue(TitleProperty, value);
			}
		}

		static BusyIndicator()
		{
			IsBusyProperty = DependencyProperty.Register("IsBusy", typeof(bool), typeof(BusyIndicator));
			TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(BusyIndicator), new PropertyMetadata("Загрузка"));
			UIElement.FocusableProperty.OverrideMetadata(typeof(BusyIndicator), new FrameworkPropertyMetadata(false));
		}
	}
}
