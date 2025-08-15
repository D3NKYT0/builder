using UpdateBuilder.Models;

namespace UpdateBuilder.ViewModels.Items
{
	public class FileUpdateItemViewModel : ItemViewModel
	{
		private long _size;

		private string _hash;

		private ModifyType _modifyType;

		public long Size
		{
			get
			{
				return _size;
			}
			set
			{
				SetProperty(ref _size, value, "Size");
			}
		}

		public string Hash
		{
			get
			{
				return _hash;
			}
			set
			{
				SetProperty(ref _hash, value, "Hash");
			}
		}

		public ModifyType ModifyType
		{
			get
			{
				return _modifyType;
			}
			set
			{
				SetProperty(ref _modifyType, value, "ModifyType");
			}
		}

		public int Version
		{
			get;
			set;
		}

		public FileUpdateItemViewModel(FileUpdateModel model)
		{
			base.Name = model.Name;
			Size = model.Size;
			Hash = model.Hash;
			ModifyType = model.ModifyType;
			Version = model.Version;
		}

		public FileUpdateModel ToModel()
		{
			return new FileUpdateModel
			{
				Name = base.Name,
				Size = Size,
				Hash = Hash,
				ModifyType = ModifyType,
				Version = Version
			};
		}
	}
}
