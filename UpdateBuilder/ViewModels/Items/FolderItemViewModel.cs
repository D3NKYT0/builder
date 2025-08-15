using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UpdateBuilder.Models;
using UpdateBuilder.Utils;

namespace UpdateBuilder.ViewModels.Items
{
	public class FolderItemViewModel : ItemViewModel
	{
		private bool _quickUpdate;

		private bool _checkHash;

		private ModifyType _modifyType;

		private ObservableCollection<FileItemViewModel> _files;

		private ObservableCollection<FolderItemViewModel> _folders;

		private ObservableCollection<ItemViewModel> _childrens;

		public bool Updating
		{
			get;
			set;
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

		public ObservableCollection<FileItemViewModel> Files
		{
			get
			{
				return _files;
			}
			set
			{
				SetProperty(ref _files, value, "Files");
			}
		}

		public ObservableCollection<FolderItemViewModel> Folders
		{
			get
			{
				return _folders;
			}
			set
			{
				SetProperty(ref _folders, value, "Folders");
			}
		}

		public ObservableCollection<ItemViewModel> Childrens
		{
			get
			{
				return _childrens;
			}
			set
			{
				SetProperty(ref _childrens, value, "Childrens");
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
				if (!Updating)
				{
					SetRecurceQuickUpdate(this, value);
				}
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
				SetRecurseCheckHash(this, value);
			}
		}

		private void SetRecurceQuickUpdate(FolderItemViewModel rootFolder, bool value)
		{
			foreach (FolderItemViewModel folder in rootFolder.Folders)
			{
				folder.QuickUpdate = value;
				SetRecurceQuickUpdate(folder, value);
			}
			foreach (FileItemViewModel file in rootFolder.Files)
			{
				file.QuickUpdate = value;
			}
		}

		private void SetRecurseCheckHash(FolderItemViewModel rootFolder, bool value)
		{
			foreach (FolderItemViewModel folder in rootFolder.Folders)
			{
				folder.CheckHash = value;
				SetRecurseCheckHash(folder, value);
			}
			foreach (FileItemViewModel file in rootFolder.Files)
			{
				file.CheckHash = value;
			}
		}

		public FolderItemViewModel(FolderModel model)
		{
			base.Name = model.Name;
			Files = new ObservableCollection<FileItemViewModel>(model.Files.Select((FileModel c) => new FileItemViewModel(c)));
			Folders = new ObservableCollection<FolderItemViewModel>(model.Folders.Select((FolderModel c) => new FolderItemViewModel(c)));
			Childrens = new ObservableCollection<ItemViewModel>();
			Childrens.AddRange(Folders);
			Childrens.AddRange(Files);
			_checkHash = GetCheckHashRecurse(this);
			RaisePropertyChanged("CheckHash");
			_quickUpdate = GetQuickUpdateRecurse(this);
			RaisePropertyChanged("QuickUpdate");
			ModifyType = GetModifyType(this);
		}

		public int GetCount()
		{
			return GetCountRecurce(this);
		}

		public long GetSize()
		{
			return GetSizeRecurce(this);
		}

		private ModifyType GetModifyType(FolderItemViewModel rootFolder)
		{
			List<ModifyType> modifyTypeRecurse = GetModifyTypeRecurse(rootFolder);
			IGrouping<ModifyType, ModifyType>[] array = (from c in modifyTypeRecurse
				group c by c).ToArray();
			return (array.Length != 1) ? ModifyType.Modified : array.First().Key;
		}

		private List<ModifyType> GetModifyTypeRecurse(FolderItemViewModel rootFolder)
		{
			List<ModifyType> list = rootFolder.Folders.SelectMany(GetModifyTypeRecurse).ToList();
			list.AddRange(rootFolder.Files.Select((FileItemViewModel c) => c.ModifyType));
			return list;
		}

		private bool GetCheckHashRecurse(FolderItemViewModel rootFolder)
		{
			return rootFolder.Folders.All(GetCheckHashRecurse) && rootFolder.Files.All((FileItemViewModel c) => c.CheckHash);
		}

		private bool GetQuickUpdateRecurse(FolderItemViewModel rootFolder)
		{
			return rootFolder.Folders.All(GetQuickUpdateRecurse) && rootFolder.Files.All((FileItemViewModel c) => c.QuickUpdate);
		}

		private int GetCountRecurce(FolderItemViewModel rootFolder)
		{
			int num = ((IEnumerable<FolderItemViewModel>)rootFolder.Folders).Sum((Func<FolderItemViewModel, int>)GetCountRecurce);
			return num + rootFolder.Files.Count;
		}

		private long GetSizeRecurce(FolderItemViewModel rootFolder)
		{
			long num = rootFolder.Folders.Sum((FolderItemViewModel folder) => GetSizeRecurce(folder));
			return num + rootFolder.Files.Sum((FileItemViewModel c) => c.Size);
		}

		private FolderModel GetFolderRecurce(FolderItemViewModel rootFolder)
		{
			FolderModel folderModel = new FolderModel
			{
				Name = rootFolder.Name
			};
			foreach (FolderItemViewModel folder in rootFolder.Folders)
			{
				folderModel.Folders.Add(GetFolderRecurce(folder));
			}
			foreach (FileItemViewModel file in rootFolder.Files)
			{
				folderModel.Files.Add(file.ToModel());
			}
			return folderModel;
		}

		private FolderModel GetFolderUnDeletedRecurce(FolderItemViewModel rootFolder)
		{
			FolderModel folderModel = new FolderModel
			{
				Name = rootFolder.Name
			};
			foreach (FolderItemViewModel item in rootFolder.Folders.Where((FolderItemViewModel c) => c.ModifyType != ModifyType.Deleted))
			{
				folderModel.Folders.Add(GetFolderUnDeletedRecurce(item));
			}
			foreach (FileItemViewModel item2 in rootFolder.Files.Where((FileItemViewModel c) => c.ModifyType != ModifyType.Deleted))
			{
				folderModel.Files.Add(item2.ToModel());
			}
			return folderModel;
		}

		public FolderModel ToModel()
		{
			return GetFolderRecurce(this);
		}

		public FolderModel ToUnDeletedModel()
		{
			return GetFolderUnDeletedRecurce(this);
		}
	}
}
