using UpdateBuilder.ViewModels.Base;

namespace UpdateBuilder.ViewModels.Items
{
	public class ItemViewModel : ViewModelBase
	{
		private string _name;

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				SetProperty(ref _name, value, "Name");
			}
		}
	}
}
