using System.Collections.ObjectModel;
using System.Linq;
using UpdateBuilder.Models;

namespace UpdateBuilder.ViewModels.Items
{
	public class FileItemViewModel : ItemViewModel
	{
		private long _size;

		private string _hash;

		private bool _quickUpdate;

		private bool _checkHash;

		private ModifyType _modifyType;

		private ObservableCollection<FileUpdateItemViewModel> _fileUpdates;

		public string FullPath
		{
			get;
			set;
		}

		public string Path
		{
			get;
			set;
		}

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

		public bool QuickUpdate
		{
			get
			{
				return _quickUpdate;
			}
			set
			{
				SetProperty(ref _quickUpdate, value, "QuickUpdate");
			}
		}

		public bool CheckHash
		{
			get
			{
				return _checkHash;
			}
			set
			{
				SetProperty(ref _checkHash, value, "CheckHash");
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

		public ObservableCollection<FileUpdateItemViewModel> FileUpdates
		{
			get
			{
				return _fileUpdates;
			}
			set
			{
				SetProperty(ref _fileUpdates, value, "FileUpdates");
			}
		}

		public FileItemViewModel(FileModel model)
		{
			base.Name = model.Name;
			Size = model.Size;
			Hash = model.Hash;
			QuickUpdate = model.QuickUpdate;
			CheckHash = model.CheckHash;
			FullPath = model.FullPath;
			Path = model.Path;
			ModifyType = model.ModifyType;
			FileUpdates = new ObservableCollection<FileUpdateItemViewModel>(model.FileUpdates.Select((FileUpdateModel c) => new FileUpdateItemViewModel(c)));
		}

		public FileModel ToModel()
		{
			return new FileModel
			{
				Name = base.Name,
				Size = Size,
				Hash = Hash,
				QuickUpdate = QuickUpdate,
				CheckHash = CheckHash,
				FullPath = FullPath,
				Path = Path,
				ModifyType = ModifyType
			};
		}
	}
}
