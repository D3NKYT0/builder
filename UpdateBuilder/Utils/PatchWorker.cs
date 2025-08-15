using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using UpdateBuilder.Models;
using Ionic.Zlib;
using Ionic.Zip;

namespace UpdateBuilder.Utils
{
	public class PatchWorker
	{
		private readonly Crc32 _hashCalc = new Crc32();

		public event EventHandler ProgressChanged;

		public async Task<FolderModel> GetFolderInfoAsync(string path, CancellationToken token)
		{
			return await Task.Run(delegate
			{
				try
				{
					Logger.Instance.Add("Проверяем корневую папку");
					if (!Directory.Exists(path))
					{
						Logger.Instance.Add("Папка отсутствует");
						return null;
					}
					Logger.Instance.Add("Все на месте, начинаем поиск файлов и папок");
					DirectoryInfo rootDir = new DirectoryInfo(path);
					FolderModel treeRecurse = GetTreeRecurse(rootDir, token);
					Logger.Instance.Add("Все успешно загружено");
					return treeRecurse;
				}
				catch (Exception ex)
				{
					Logger.Instance.Add("Произошла ошибка во время чтения файлов [" + ex.Message + "]");
					return null;
				}
			});
		}

		private FolderModel GetTreeRecurse(DirectoryInfo rootDir, CancellationToken token, string path = "")
		{
			token.ThrowIfCancellationRequested();
			Logger.Instance.Add($"Погружаемся в {rootDir}");
			FolderModel folderModel = new FolderModel
			{
				Name = rootDir.Name,
				Path = Path.Combine(path, rootDir.Name)
			};
			Logger.Instance.Add("Проверяем исть ли подпапки");
			foreach (DirectoryInfo item in rootDir.EnumerateDirectories())
			{
				folderModel.Folders.Add(GetTreeRecurse(item, token, folderModel.Path));
				Logger.Instance.Add($"Добавили {item} в {folderModel.Name}");
			}
			Logger.Instance.Add("Проверяем исть ли файлы");
			foreach (FileInfo item2 in rootDir.EnumerateFiles())
			{
				token.ThrowIfCancellationRequested();
				string hash = _hashCalc.Get(item2.FullName);
				folderModel.Files.Add(new FileModel
				{
					Name = item2.Name,
					Hash = hash,
					Size = item2.Length,
					FullPath = item2.FullName,
					Path = folderModel.Path
				});
				Logger.Instance.Add($"Добавили {item2} в {folderModel.Name}");
			}
			Logger.Instance.Add($"Поднимаемся из {rootDir}");
			return folderModel;
		}

		public async Task<bool> BuildUpdateAsync(UpdateInfoModel updateInfoAll, UpdateInfoModel updateInfo, string outPath, CancellationToken token)
		{
			return await Task.Run(delegate
			{
				try
				{
					token.ThrowIfCancellationRequested();
					Logger.Instance.Add("Создаем патч лист");
					BuildUpdateInfo(updateInfo, outPath);
					Logger.Instance.Add("Патч лист создан");
					Logger.Instance.Add("Начинаем паковать");
					PuckingRecurse(updateInfoAll.Folder, outPath + "\\" + updateInfoAll.Folder.Name, token); //тут очен долго ...
					Logger.Instance.Add("Все запаковано");
					return true;
				}
				catch (Exception ex)
				{
					Logger.Instance.Add("Произошла ошибка во время создания апдейта [" + ex.Message + "]");
					return false;
				}
			});
		}

		private void PuckingRecurse(FolderModel rootFolder, string outPath, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			foreach (FolderModel folder in rootFolder.Folders)
			{
				string text = Path.Combine(outPath, folder.Name);
				if (Directory.Exists(text) && folder.ModifyType == ModifyType.Deleted)
				{
					Directory.Delete(text, recursive: true);
				}
				else
				{
					Directory.CreateDirectory(text);
				}
				PuckingRecurse(folder, text, token);
			}
			foreach (FileModel item in rootFolder.Files.Where((FileModel c) => c.ModifyType == ModifyType.Deleted))
			{
				string text2 = Path.Combine(outPath, item.Name + ".zip");
				Logger.Instance.Add("Удаляем " + text2);
				if (File.Exists(text2))
				{
					File.Delete(text2);
				}
			}
			foreach (FileModel item2 in rootFolder.Files.Where((FileModel c) => c.ModifyType == ModifyType.NotModified))
			{
				Logger.Instance.Add("Ничего не делаем с " + item2.FullPath);
			}
			foreach (FileModel item3 in rootFolder.Files.Where((FileModel c) => c.ModifyType == ModifyType.Modified || c.ModifyType == ModifyType.New))
			{
				try
				{
					string str = Path.Combine(outPath, item3.Name);
					Logger.Instance.Add("Проверяем " + item3.FullPath);
					if (!File.Exists(item3.FullPath))
					{
						throw new Exception("Файл отсутствует");
					}
					Logger.Instance.Add(item3.FullPath + " на месте");
					Logger.Instance.Add("Пакуем " + item3.Name);
					using (ZipFile zipFile = new ZipFile
					{
						//CompressionLevel = CompressionLevel.BestCompression
						CompressionLevel = CompressionLevel.BestCompression
					})
					{
						zipFile.ProvisionalAlternateEncoding = Encoding.UTF8;
						zipFile.AddFile(item3.FullPath, "");
						zipFile.Save(str + ".zip");
					}
					Logger.Instance.Add("Запаковано " + item3.Name);
				}
				catch (Exception ex)
				{
					Logger.Instance.Add("Не удалось запаковать " + item3.Name + ", причина [" + ex.Message + "]");
				}
				OnProgressChanged();
			}
		}

		private static void BuildUpdateInfo(UpdateInfoModel updateInfo, string outPath)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(UpdateInfoModel));
			XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = true,
				Encoding = Encoding.UTF8
			};
			using XmlWriter xmlWriter = XmlWriter.Create(outPath + "\\UpdateInfo.xml", settings);
			xmlSerializer.Serialize(xmlWriter, updateInfo);
		}

		protected virtual void OnProgressChanged()
		{
			this.ProgressChanged?.Invoke(this, EventArgs.Empty);
		}

		public async Task<FolderModel> SyncUpdateInfoAsync(FolderModel mainFolder, string patchInfoPath, CancellationToken token)
		{
			return await Task.Run(delegate
			{
				UpdateInfoModel updateInfoModel = DeserializeUpdateInfo(patchInfoPath);
				Logger.Instance.Add("Данные о прошлом патче получены");
				FolderModel folderModel = SyncFolder(updateInfoModel.Folder, mainFolder, token);
				if (folderModel == null)
				{
					return mainFolder;
				}
				Logger.Instance.Add("Патч синхронизирован");
				return folderModel;
			}, token);
		}

		private FolderModel SyncFolder(FolderModel patchInfoFolder, FolderModel mainFolder, CancellationToken token)
		{
			if (patchInfoFolder.Name.Equals(mainFolder.Name))
			{
				FolderModel folderModel = new FolderModel
				{
					Name = mainFolder.Name,
					ModifyType = mainFolder.ModifyType,
					Path = mainFolder.Path
				};
				Logger.Instance.Add("Синхронизация собранного ранее патча с новым...");
				SyncFolderRecurse(patchInfoFolder, mainFolder, folderModel, reverse: false);
				token.ThrowIfCancellationRequested();
				Logger.Instance.Add("Синхронизировано");
				Logger.Instance.Add("Синхронизация нового патча с собраным ранее...");
				SyncFolderRecurse(mainFolder, patchInfoFolder, folderModel, reverse: true);
				token.ThrowIfCancellationRequested();
				Logger.Instance.Add("Синхронизировано");
				SyncFiles(patchInfoFolder, mainFolder, folderModel, reverse: false);
				token.ThrowIfCancellationRequested();
				SyncFiles(mainFolder, patchInfoFolder, folderModel, reverse: true);
				token.ThrowIfCancellationRequested();
				return folderModel;
			}
			Logger.Instance.Add("Папки для синхронизации не совпадают");
			return null;
		}

		private void SyncFolderRecurse(FolderModel masterFolders, FolderModel slaveFolders, FolderModel syncFolderModel, bool reverse)
		{
			foreach (FolderModel masterFolder in masterFolders.Folders)
			{
				Logger.Instance.Add("Синхронизация папки " + masterFolder.Name);
				if (slaveFolders != null)
				{
					Logger.Instance.Add("Поиск зависимой папки " + masterFolder.Name);
					FolderModel folderModel = slaveFolders.Folders.FirstOrDefault((FolderModel c) => c.Name.Equals(masterFolder.Name));
					FolderModel folderModel2 = syncFolderModel.Folders.FirstOrDefault((FolderModel c) => c.Name.Equals(masterFolder.Name));
					if (folderModel2 == null)
					{
						Logger.Instance.Add("Создаем папку синхронизации " + masterFolder.Name);
						folderModel2 = CreateSyncFolder(masterFolder, folderModel);
						syncFolderModel.Folders.Add(folderModel2);
					}
					else
					{
						Logger.Instance.Add("Папка синхронизации присутствует " + masterFolder.Name);
					}
					Logger.Instance.Add("Синхронизации файлов для " + masterFolder.Name);
					SyncFiles(masterFolder, folderModel, folderModel2, reverse);
					SyncFolderRecurse(masterFolder, folderModel, folderModel2, reverse);
					continue;
				}
				Logger.Instance.Add("Зависимой папки нет " + masterFolder.Name);
				FolderModel folderModel3 = syncFolderModel.Folders.FirstOrDefault((FolderModel c) => c.Name.Equals(masterFolder.Name));
				if (folderModel3 == null)
				{
					folderModel3 = new FolderModel
					{
						Name = masterFolder.Name,
						ModifyType = masterFolder.ModifyType,
						Path = masterFolder.Path
					};
					syncFolderModel.Folders.Add(folderModel3);
				}
				foreach (FileModel file in masterFolder.Files)
				{
					ModifyType modifyType = (!reverse) ? ModifyType.Deleted : ModifyType.New;
					Logger.Instance.Add($"Устанавливаем тип {modifyType} для {file.Name}");
					folderModel3.Files.Add(new FileModel
					{
						Name = file.Name,
						ModifyType = modifyType,
						Path = file.Path,
						Hash = file.Hash,
						Size = file.Size
					});
				}
				SyncFolderRecurse(masterFolder, null, folderModel3, reverse);
			}
		}

		private void SyncFiles(FolderModel masterFolder, FolderModel sameSlaveFolder, FolderModel syncFolder, bool reverse)
		{
			foreach (FileModel masterFile in masterFolder.Files)
			{
				Logger.Instance.Add("Синхронизируем файл " + masterFile.Name);
				FileModel slaveFile = sameSlaveFolder?.Files.FirstOrDefault((FileModel c) => c.Name.Equals(masterFile.Name));
				FileModel fileModel = syncFolder.Files.FirstOrDefault((FileModel c) => c.Name.Equals(masterFile.Name));
				if (fileModel == null)
				{
					FileModel item = CreateSyncFile(masterFile, slaveFile, reverse);
					syncFolder.Files.Add(item);
					Logger.Instance.Add("Создаем файл синхронизации " + masterFile.Name);
				}
				else
				{
					Logger.Instance.Add("Файл синхронизации присутствует " + masterFile.Name);
				}
			}
		}

		private FolderModel CreateSyncFolder(FolderModel masterFolder, FolderModel slaveFolder)
		{
			if (slaveFolder != null)
			{
				Logger.Instance.Add("Зависимая папка найдена " + masterFolder.Name);
				return new FolderModel
				{
					Name = slaveFolder.Name,
					ModifyType = slaveFolder.ModifyType,
					Path = slaveFolder.Path
				};
			}
			Logger.Instance.Add("Зависимой папки нет " + masterFolder.Name);
			return new FolderModel
			{
				Name = masterFolder.Name,
				ModifyType = masterFolder.ModifyType,
				Path = masterFolder.Path
			};
		}

		private FileModel CreateSyncFile(FileModel masterFile, FileModel slaveFile, bool reverse)
		{
			if (slaveFile != null)
			{
				Logger.Instance.Add("Зависимый файл найден " + slaveFile.Name);
				FileModel fileModel = new FileModel
				{
					Name = slaveFile.Name,
					Path = slaveFile.Path,
					CheckHash = masterFile.CheckHash,
					QuickUpdate = masterFile.QuickUpdate,
					Hash = slaveFile.Hash,
					Size = slaveFile.Size,
					FullPath = slaveFile.FullPath
				};
				if (slaveFile.Hash == masterFile.Hash)
				{
					fileModel.ModifyType = ModifyType.NotModified;
				}
				if (slaveFile.Hash != masterFile.Hash)
				{
					fileModel.ModifyType = ModifyType.Modified;
				}
				return fileModel;
			}
			Logger.Instance.Add("Зависимого файла нет " + masterFile.Name);
			ModifyType modifyType = (!reverse) ? ModifyType.Deleted : ModifyType.New;
			Logger.Instance.Add($"Устанавливаем тип {modifyType} для {masterFile.Name}");
			return new FileModel
			{
				Name = masterFile.Name,
				ModifyType = modifyType,
				Path = masterFile.Path,
				FullPath = masterFile.FullPath,
				Hash = masterFile.Hash,
				Size = masterFile.Size,
				CheckHash = true,
				QuickUpdate = true
			};
		}

		private UpdateInfoModel DeserializeUpdateInfo(string patchInfoPath)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(UpdateInfoModel));
			using StreamReader textReader = new StreamReader(File.OpenRead(patchInfoPath));
			return (UpdateInfoModel)xmlSerializer.Deserialize(textReader);
		}
	}
}
