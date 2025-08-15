using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;
using UpdateBuilder.Models;
using UpdateBuilder.Properties;
using UpdateBuilder.Utils;
using UpdateBuilder.ViewModels.Base;
using UpdateBuilder.ViewModels.Items;

namespace UpdateBuilder.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		private bool _isRoot;

		private bool _inBuilding;

		private string _pathPath;

		private string _outPath;

		private string _totalSize = 0L.BytesToString();

		private int _totalCount;

		private int _progressValue;

		private FolderModel _mainFolder;

		private ObservableCollection<FolderItemViewModel> _syncFolder = new ObservableCollection<FolderItemViewModel>();

		private readonly PatchWorker _patchWorker;

		private CancellationTokenSource _cts;

		public ICommand SetPatchPathCommand
		{
			get;
			set;
		}

		public ICommand SetOutPathCommand
		{
			get;
			set;
		}

		public ICommand GoToSiteCommand
		{
			get;
			set;
		}

		public ICommand ClearLogCommand
		{
			get;
			set;
		}

		public ICommand BuildUpdateCommand
		{
			get;
			set;
		}

		public ICommand SyncCommand
		{
			get;
			set;
		}

		public bool IsRoot
		{
			get
			{
				return _isRoot;
			}
			set
			{
				SetProperty(ref _isRoot, value, "IsRoot");
			}
		}

		public string PatchPath
		{
			get
			{
				return _pathPath;
			}
			set
			{
				if (SetProperty(ref _pathPath, value, "PatchPath"))
				{
					Settings.Default.PatchPath = value;
					Settings.Default.Save();
					LoadInfoAsync();
				}
			}
		}

		public string OutPath
		{
			get
			{
				return _outPath;
			}
			set
			{
				if (SetProperty(ref _outPath, value, "OutPath"))
				{
					Settings.Default.OutPath = value;
					Settings.Default.Save();
					SyncInfoAsync();
				}
			}
		}

		public ObservableCollection<FolderItemViewModel> SyncFolder
		{
			get
			{
				return _syncFolder;
			}
			set
			{
				SetProperty(ref _syncFolder, value, "SyncFolder");
			}
		}

		public string TotalSize
		{
			get
			{
				return _totalSize;
			}
			set
			{
				SetProperty(ref _totalSize, value, "TotalSize");
			}
		}

		public int TotalCount
		{
			get
			{
				return _totalCount;
			}
			set
			{
				SetProperty(ref _totalCount, value, "TotalCount");
			}
		}

		public int ProgressValue
		{
			get
			{
				return _progressValue;
			}
			set
			{
				SetProperty(ref _progressValue, value, "ProgressValue");
				RaisePropertyChanged(() => ProgressProcent);
			}
		}

		public string ProgressProcent
		{
			get
			{
				if (TotalCount == 0)
				{
					return "100%";
				}
				return ProgressValue * 100 / TotalCount + "%";
			}
		}

		public bool InBuilding
		{
			get
			{
				return _inBuilding;
			}
			set
			{
				SetProperty(ref _inBuilding, value, "InBuilding");
			}
		}

		public bool CanSync => !string.IsNullOrEmpty(PatchPath) && !string.IsNullOrEmpty(OutPath) && _mainFolder != null;

		public MainWindowViewModel()
		{
			_patchWorker = new PatchWorker();
			_patchWorker.ProgressChanged += delegate
			{
				ProgressValue++;
			};
			SetPatchPathCommand = new RelayCommand(delegate
			{
				PatchPath = GetPath();
			}, (object can) => !InBuilding);
			SetOutPathCommand = new RelayCommand(delegate
			{
				OutPath = GetPath();
			}, (object can) => !InBuilding);
			GoToSiteCommand = new RelayCommand(delegate
			{
				Process.Start("http:\\\\l2jcore.top");
			});
			ClearLogCommand = new RelayCommand(delegate
			{
				Logger.Instance.Clear();
			});
			SyncCommand = new RelayCommand(delegate
			{
				LoadInfoAsync();
			}, (object can) => !base.IsBusy && CanSync);
			BuildUpdateCommand = new RelayCommand(delegate
			{
				BuildUpdateAsync();
			}, (object can) => !base.IsBusy && !string.IsNullOrWhiteSpace(PatchPath) && !string.IsNullOrWhiteSpace(OutPath));
			Logger.Instance.Add("Ready to work");
		}

		private async void LoadInfoAsync()
		{
			base.IsBusy = true;
			_cts?.Cancel(throwOnFirstException: true);
			_cts = new CancellationTokenSource();
			CancellationToken token = _cts.Token;
			_mainFolder = null;
			TotalCount = 0;
			TotalSize = "0";
			ProgressValue = 0;
			Logger.Instance.Clear();
			_mainFolder = await _patchWorker.GetFolderInfoAsync(PatchPath, token);
			if (_mainFolder != null)
			{
				CreateSyncFolder(_mainFolder);
			}
			base.IsBusy = false;
			bool cancel = _cts.IsCancellationRequested;
			_cts = null;
			CommandManager.InvalidateRequerySuggested();
			if (!cancel)
			{
				SyncInfoAsync();
			}
		}

		private async void SyncInfoAsync()
		{
			if (!CanSync || base.IsBusy)
			{
				return;
			}
			base.IsBusy = true;
			_cts?.Cancel(throwOnFirstException: true);
			_cts = new CancellationTokenSource();
			CancellationToken token = _cts.Token;
			try
			{
				Logger.Instance.Add("Начинаем синхронизацию...");
				string patchInfoPath = Path.Combine(OutPath, "UpdateInfo.xml");
				if (File.Exists(patchInfoPath))
				{
					CreateSyncFolder(await _patchWorker.SyncUpdateInfoAsync(_mainFolder, patchInfoPath, token));
				}
				else
				{
					Logger.Instance.Add("Файлов предыдущего патча не найдено");
				}
			}
			catch (Exception e)
			{
				Logger.Instance.Add("Во время синхронизации произошла ошибка");
				Logger.Instance.Add(e.Message);
			}
			Logger.Instance.Add("Конец синхронизации");
			base.IsBusy = false;
			_cts = null;
			CommandManager.InvalidateRequerySuggested();
		}

		private void CreateSyncFolder(FolderModel syncF)
		{
			SyncFolder.Clear();
			FolderItemViewModel folderItemViewModel = new FolderItemViewModel(syncF);
			TotalCount = folderItemViewModel.GetCount();
			TotalSize = folderItemViewModel.GetSize().BytesToString();
			SyncFolder.Add(folderItemViewModel);
			ProgressValue = TotalCount;
		}

		private async void BuildUpdateAsync()
		{
			base.IsBusy = true;
			InBuilding = true;
			_cts?.Cancel(throwOnFirstException: true);
			_cts = new CancellationTokenSource();
			CancellationToken token = _cts.Token;
			Logger.Instance.Clear();
			ProgressValue = 0;
			FolderItemViewModel rootFolder = SyncFolder.FirstOrDefault();
			if (rootFolder != null)
			{
				UpdateInfoModel updateInfoAll = new UpdateInfoModel
				{
					Folder = rootFolder.ToModel()
				};
				UpdateInfoModel updateInfo = new UpdateInfoModel
				{
					Folder = rootFolder.ToUnDeletedModel()
				};
				bool result = await _patchWorker.BuildUpdateAsync(updateInfoAll, updateInfo, OutPath, token);
				if (!token.IsCancellationRequested && result)
				{
					Logger.Instance.Add("--------------------------------------------");
					Logger.Instance.Add("--------------ПАТЧ-ГОТОВ!------------");
					Logger.Instance.Add("--------------------------------------------");
					Process.Start("explorer", OutPath);
				}
			}
			else
			{
				Logger.Instance.Add("КОРНЕВОЙ ПАПКИ НЕТ!");
			}
			ProgressValue = TotalCount;
			InBuilding = false;
			base.IsBusy = false;
			_cts = null;
			CommandManager.InvalidateRequerySuggested();
		}

		private string GetPath()
		{
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
			return (folderBrowserDialog.ShowDialog() == DialogResult.OK) ? folderBrowserDialog.SelectedPath : string.Empty;
		}
	}
}
