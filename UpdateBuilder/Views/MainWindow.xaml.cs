using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Controls;


namespace UpdateBuilder.Views
{
	public partial class MainWindow : Window, IComponentConnector
	{
		/*
		internal Button MinButton;

		internal Button CloseButton;

		internal Grid HeaderPanel;

		internal CheckBox ShowSizeCheckbox;

		internal CheckBox ShowHashCheckbox;

		internal Grid FilesPanel;

		//internal TextBlock PatchPath;

		//internal TextBlock OutPath;

		internal Grid MainPanel;

		internal Grid ProgressPanel;

		//private bool _contentLoaded;*/


		public MainWindow()
		{
			InitializeComponent();
		}

		private void MinButton_Click(object sender, RoutedEventArgs e)
		{
			SystemCommands.MinimizeWindow(this);
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			SystemCommands.CloseWindow(this);
		}

		private void Button_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] array = (string[])e.Data.GetData(DataFormats.FileDrop);
				if (Directory.Exists(array[0]))
				{
					PatchPath.Text = array[0];
				}
			}
		}

		private void ItemsControl_Loaded(object sender, RoutedEventArgs e)
		{
			ItemsControl root = (ItemsControl)sender;
			ScrollViewer scrollViewer = FindScrollViewer(root);
			if (scrollViewer == null)
			{
				return;
			}
			scrollViewer.ScrollChanged += delegate(object o, ScrollChangedEventArgs args)
			{
				if (args.ExtentHeightChange > 0.0)
				{
					scrollViewer.ScrollToBottom();
				}
			};
		}

		private static ScrollViewer FindScrollViewer(DependencyObject root)
		{
			Queue<DependencyObject> queue = new Queue<DependencyObject>(new DependencyObject[1]
			{
				root
			});
			do
			{
				DependencyObject dependencyObject = queue.Dequeue();
				if (dependencyObject is ScrollViewer)
				{
					return (ScrollViewer)dependencyObject;
				}
				for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
				{
					queue.Enqueue(VisualTreeHelper.GetChild(dependencyObject, i));
				}
			}
			while (queue.Count > 0);
			return null;
		}

		private void ButtonOut_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] array = (string[])e.Data.GetData(DataFormats.FileDrop);
				if (Directory.Exists(array[0]))
				{
					OutPath.Text = array[0];
				}
			}
		}

		/*
		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		public void InitializeComponent()
		{
			if (!_contentLoaded)
			{
				_contentLoaded = true;
				Uri resourceLocator = new Uri("/UpdateBuilder;component/views/mainwindow.xaml", UriKind.Relative);
				Application.LoadComponent(this, resourceLocator);
			}
		}*/
		/*
		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		internal Delegate _CreateDelegate(Type delegateType, string handler)
		{
			return Delegate.CreateDelegate(delegateType, this, handler);
		}*/
		/*
		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void IComponentConnector.Connect(int connectionId, object target)
		{
			switch (connectionId)
			{
			case 1:
				MinButton = (Button)target;
				MinButton.Click += MinButton_Click;
				break;
			case 2:
				CloseButton = (Button)target;
				CloseButton.Click += CloseButton_Click;
				break;
			case 3:
				HeaderPanel = (Grid)target;
				break;
			case 4:
				ShowSizeCheckbox = (CheckBox)target;
				break;
			case 5:
				ShowHashCheckbox = (CheckBox)target;
				break;
			case 6:
				FilesPanel = (Grid)target;
				break;
			case 7:
				PatchPath = (TextBlock)target;
				break;
			case 8:
				((Button)target).Drop += Button_Drop;
				break;
			case 9:
				OutPath = (TextBlock)target;
				break;
			case 10:
				((Button)target).Drop += ButtonOut_Drop;
				break;
			case 11:
				MainPanel = (Grid)target;
				break;
			case 12:
				((ItemsControl)target).Loaded += ItemsControl_Loaded;
				break;
			case 13:
				ProgressPanel = (Grid)target;
				break;
			default:
				_contentLoaded = true;
				break;
			}
		}*/
	}
}
