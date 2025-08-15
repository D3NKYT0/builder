using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace UpdateBuilder.ViewModels.Base
{
	public class ViewModelBase : INotifyPropertyChanged
	{
		private bool _isBusy;

		public bool IsBusy
		{
			get
			{
				return _isBusy;
			}
			set
			{
				_isBusy = value;
				RaisePropertyChanged(() => IsBusy);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
		{
			string propertyName = ExtractPropertyName(propertyExpression);
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public static string ExtractPropertyName<T>(Expression<Func<T>> propertyExpression)
		{
			if (propertyExpression == null)
			{
				throw new ArgumentNullException("propertyExpression");
			}
			MemberExpression memberExpression = propertyExpression.Body as MemberExpression;
			if (memberExpression == null)
			{
				throw new ArgumentException("memberExpression");
			}
			PropertyInfo propertyInfo = memberExpression.Member as PropertyInfo;
			if (propertyInfo == null)
			{
				throw new ArgumentException("property");
			}
			MethodInfo getMethod = propertyInfo.GetGetMethod(nonPublic: true);
			if (getMethod.IsStatic)
			{
				throw new ArgumentException("static method");
			}
			return memberExpression.Member.Name;
		}

		protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
		{
			if (object.Equals(storage, value))
			{
				return false;
			}
			storage = value;
			RaisePropertyChanged(propertyName);
			return true;
		}
	}
}
