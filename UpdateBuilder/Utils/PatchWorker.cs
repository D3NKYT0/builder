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
					Logger.Instance.Add("Verificando pasta raiz");
					if (!Directory.Exists(path))
					{
						Logger.Instance.Add("Pasta não encontrada");
						return null;
					}
					Logger.Instance.Add("Pasta encontrada, iniciando busca de arquivos e pastas");
					DirectoryInfo rootDir = new DirectoryInfo(path);
					FolderModel treeRecurse = GetTreeRecurse(rootDir, token);
					Logger.Instance.Add("Todos os arquivos carregados com sucesso");
					return treeRecurse;
				}
				catch (Exception ex)
				{
					Logger.Instance.Add("Erro durante a leitura dos arquivos [" + ex.Message + "]");
					return null;
				}
			});
		}

		private FolderModel GetTreeRecurse(DirectoryInfo rootDir, CancellationToken token, string path = "")
		{
			token.ThrowIfCancellationRequested();
			Logger.Instance.Add($"Explorando pasta: {rootDir.Name}");
			FolderModel folderModel = new FolderModel
			{
				Name = rootDir.Name,
				Path = Path.Combine(path, rootDir.Name)
			};
			Logger.Instance.Add("Verificando subpastas");
			foreach (DirectoryInfo item in rootDir.EnumerateDirectories())
			{
				folderModel.Folders.Add(GetTreeRecurse(item, token, folderModel.Path));
				Logger.Instance.Add($"Adicionada pasta: {item.Name} em {folderModel.Name}");
			}
			Logger.Instance.Add("Verificando arquivos");
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
				Logger.Instance.Add($"Adicionado arquivo: {item2.Name} em {folderModel.Name}");
			}
			Logger.Instance.Add($"Saindo da pasta: {rootDir.Name}");
			return folderModel;
		}

		public async Task<bool> BuildUpdateAsync(UpdateInfoModel updateInfoAll, UpdateInfoModel updateInfo, string outPath, CancellationToken token)
		{
			return await Task.Run(delegate
			{
				try
				{
					token.ThrowIfCancellationRequested();
					
					// Criar arquivo de log detalhado
					string logFilePath = Path.Combine(outPath, $"patch_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
					Logger.Instance.Add($"Criando arquivo de log detalhado: {logFilePath}");
					
					Logger.Instance.Add("Criando lista de patch");
					Logger.Instance.Add($"Caminho de saída: {outPath}");
					Logger.Instance.Add($"Pasta raiz: {updateInfoAll.Folder.Name}");
					
					// Verificar se o diretório de saída existe
					if (!Directory.Exists(outPath))
					{
						Logger.Instance.Add($"Criando diretório de saída: {outPath}");
						Directory.CreateDirectory(outPath);
					}
					
					// Teste simples de compactação
					Logger.Instance.Add("Testando biblioteca ZIP...");
					try
					{
						string testFile = Path.Combine(outPath, "test.txt");
						File.WriteAllText(testFile, "Teste de compactação");
						
						string testZip = Path.Combine(outPath, "test.zip");
						using (ZipFile zipFile = new ZipFile())
						{
							zipFile.CompressionLevel = CompressionLevel.BestCompression;
							zipFile.AddFile(testFile, "");
							zipFile.Save(testZip);
						}
						
						if (File.Exists(testZip))
						{
							Logger.Instance.Add("✓ Teste de compactação bem-sucedido");
							File.Delete(testFile);
							File.Delete(testZip);
						}
						else
						{
							Logger.Instance.Add("✗ Teste de compactação falhou");
						}
					}
					catch (Exception testEx)
					{
						Logger.Instance.Add($"✗ Erro no teste de compactação: {testEx.Message}");
					}
					
					BuildUpdateInfo(updateInfo, outPath);
					Logger.Instance.Add("Lista de patch criada");
					Logger.Instance.Add("Iniciando compactação");
					
					string targetPath = outPath + "\\" + updateInfoAll.Folder.Name;
					Logger.Instance.Add($"Caminho de destino para compactação: {targetPath}");
					
					PuckingRecurse(updateInfoAll.Folder, targetPath, token, logFilePath);
					Logger.Instance.Add("Compactação concluída");
					return true;
				}
				catch (Exception ex)
				{
					Logger.Instance.Add("Erro durante a criação do update [" + ex.Message + "]");
					Logger.Instance.Add("Stack trace: " + ex.StackTrace);
					return false;
				}
			});
		}

		private void PuckingRecurse(FolderModel rootFolder, string outPath, CancellationToken token, string logFilePath)
		{
			token.ThrowIfCancellationRequested();
			Logger.Instance.Add($"Processando pasta: {rootFolder.Name} em {outPath}");
			
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
				PuckingRecurse(folder, text, token, logFilePath);
			}
			
			// Contar arquivos por tipo
			var deletedFiles = rootFolder.Files.Where((FileModel c) => c.ModifyType == ModifyType.Deleted).ToList();
			var notModifiedFiles = rootFolder.Files.Where((FileModel c) => c.ModifyType == ModifyType.NotModified).ToList();
			var modifiedFiles = rootFolder.Files.Where((FileModel c) => c.ModifyType == ModifyType.Modified || c.ModifyType == ModifyType.New).ToList();
			
			// Se não há arquivos modificados mas é a primeira execução (todos são NotModified), 
			// vamos criar um patch inicial com todos os arquivos
			if (modifiedFiles.Count == 0 && notModifiedFiles.Count > 0)
			{
				Logger.Instance.Add("Primeira execução detectada - criando patch inicial com todos os arquivos");
				WriteDetailedLog(logFilePath, "=== PRIMEIRA EXECUÇÃO - CRIANDO PATCH INICIAL ===");
				modifiedFiles = notModifiedFiles.ToList();
				notModifiedFiles.Clear();
			}
			
			Logger.Instance.Add($"Arquivos na pasta {rootFolder.Name}: Deletados={deletedFiles.Count}, Não modificados={notModifiedFiles.Count}, Modificados/Novos={modifiedFiles.Count}");
			WriteDetailedLog(logFilePath, $"=== PROCESSANDO PASTA: {rootFolder.Name} ===");
			WriteDetailedLog(logFilePath, $"Total de arquivos: {rootFolder.Files.Count}");
			WriteDetailedLog(logFilePath, $"Arquivos deletados: {deletedFiles.Count}");
			WriteDetailedLog(logFilePath, $"Arquivos não modificados: {notModifiedFiles.Count}");
			WriteDetailedLog(logFilePath, $"Arquivos modificados/novos: {modifiedFiles.Count}");
			WriteDetailedLog(logFilePath, "");
			
			foreach (FileModel item in deletedFiles)
			{
				string text2 = Path.Combine(outPath, item.Name + ".zip");
				Logger.Instance.Add($"Removendo arquivo: {text2} | Hash: {item.Hash} | Tamanho: {item.Size} bytes | Tipo: {item.ModifyType}");
				WriteDetailedLog(logFilePath, $"DELETADO: {item.Name} | Caminho: {item.FullPath} | Hash: {item.Hash} | Tamanho: {item.Size} bytes | Tipo: {item.ModifyType}");
				if (File.Exists(text2))
				{
					File.Delete(text2);
				}
			}
			foreach (FileModel item2 in notModifiedFiles)
			{
				Logger.Instance.Add($"Arquivo não modificado: {item2.FullPath} | Hash: {item2.Hash} | Tamanho: {item2.Size} bytes | Tipo: {item2.ModifyType}");
				WriteDetailedLog(logFilePath, $"NÃO MODIFICADO: {item2.Name} | Caminho: {item2.FullPath} | Hash: {item2.Hash} | Tamanho: {item2.Size} bytes | Tipo: {item2.ModifyType}");
			}
			foreach (FileModel item3 in modifiedFiles)
			{
				try
				{
					string str = Path.Combine(outPath, item3.Name);
					Logger.Instance.Add($"Verificando arquivo: {item3.FullPath} | Hash: {item3.Hash} | Tamanho: {item3.Size} bytes | Tipo: {item3.ModifyType}");
					WriteDetailedLog(logFilePath, $"MODIFICADO/NOVO: {item3.Name} | Caminho: {item3.FullPath} | Hash: {item3.Hash} | Tamanho: {item3.Size} bytes | Tipo: {item3.ModifyType}");
					if (!File.Exists(item3.FullPath))
					{
						throw new Exception("Arquivo não encontrado");
					}
					Logger.Instance.Add($"Arquivo encontrado: {item3.FullPath}");
					Logger.Instance.Add($"Compactando arquivo: {item3.Name}");
					
					// Verificar se o diretório de saída existe
					string outputDir = Path.GetDirectoryName(str);
					if (!Directory.Exists(outputDir))
					{
						Logger.Instance.Add($"Criando diretório: {outputDir}");
						Directory.CreateDirectory(outputDir);
					}
					
					using (ZipFile zipFile = new ZipFile())
					{
						zipFile.CompressionLevel = CompressionLevel.BestCompression;
						zipFile.AddFile(item3.FullPath, "");
						Logger.Instance.Add($"Salvando arquivo ZIP: {str}.zip");
						zipFile.Save(str + ".zip");
					}
					Logger.Instance.Add("Arquivo compactado: " + item3.Name);
					WriteDetailedLog(logFilePath, $"✓ COMPACTADO COM SUCESSO: {item3.Name} -> {str}.zip");
				}
				catch (Exception ex)
				{
					Logger.Instance.Add("Falha ao compactar " + item3.Name + ", motivo: [" + ex.Message + "]");
					Logger.Instance.Add("Stack trace: " + ex.StackTrace);
					WriteDetailedLog(logFilePath, $"✗ ERRO AO COMPACTAR: {item3.Name} | Erro: {ex.Message}");
					WriteDetailedLog(logFilePath, $"Stack trace: {ex.StackTrace}");
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
				Logger.Instance.Add("Dados do patch anterior obtidos");
				FolderModel folderModel = SyncFolder(updateInfoModel.Folder, mainFolder, token);
				if (folderModel == null)
				{
					return mainFolder;
				}
				Logger.Instance.Add("Patch sincronizado");
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
				Logger.Instance.Add("Sincronização do patch anterior com o novo...");
				SyncFolderRecurse(patchInfoFolder, mainFolder, folderModel, reverse: false);
				token.ThrowIfCancellationRequested();
				Logger.Instance.Add("Sincronizado");
				Logger.Instance.Add("Sincronização do novo patch com o anterior...");
				SyncFolderRecurse(mainFolder, patchInfoFolder, folderModel, reverse: true);
				token.ThrowIfCancellationRequested();
				Logger.Instance.Add("Sincronizado");
				SyncFiles(patchInfoFolder, mainFolder, folderModel, reverse: false);
				token.ThrowIfCancellationRequested();
				SyncFiles(mainFolder, patchInfoFolder, folderModel, reverse: true);
				token.ThrowIfCancellationRequested();
				return folderModel;
			}
			Logger.Instance.Add("Pastas para sincronização não coincidem");
			return null;
		}

		private void SyncFolderRecurse(FolderModel masterFolders, FolderModel slaveFolders, FolderModel syncFolderModel, bool reverse)
		{
			foreach (FolderModel masterFolder in masterFolders.Folders)
			{
				Logger.Instance.Add("Sincronização de pasta " + masterFolder.Name);
				if (slaveFolders != null)
				{
					Logger.Instance.Add("Procurando pasta dependente " + masterFolder.Name);
					FolderModel folderModel = slaveFolders.Folders.FirstOrDefault((FolderModel c) => c.Name.Equals(masterFolder.Name));
					FolderModel folderModel2 = syncFolderModel.Folders.FirstOrDefault((FolderModel c) => c.Name.Equals(masterFolder.Name));
					if (folderModel2 == null)
					{
						Logger.Instance.Add("Criando pasta de sincronização " + masterFolder.Name);
						folderModel2 = CreateSyncFolder(masterFolder, folderModel);
						syncFolderModel.Folders.Add(folderModel2);
					}
					else
					{
						Logger.Instance.Add("Pasta de sincronização presente " + masterFolder.Name);
					}
					Logger.Instance.Add("Sincronização de arquivos para " + masterFolder.Name);
					SyncFiles(masterFolder, folderModel, folderModel2, reverse);
					SyncFolderRecurse(masterFolder, folderModel, folderModel2, reverse);
					continue;
				}
				Logger.Instance.Add("Pasta dependente não encontrada " + masterFolder.Name);
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
					Logger.Instance.Add($"Definindo tipo {modifyType} para {file.Name}");
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
				Logger.Instance.Add("Sincronizando arquivo " + masterFile.Name);
				FileModel slaveFile = sameSlaveFolder?.Files.FirstOrDefault((FileModel c) => c.Name.Equals(masterFile.Name));
				FileModel fileModel = syncFolder.Files.FirstOrDefault((FileModel c) => c.Name.Equals(masterFile.Name));
				if (fileModel == null)
				{
					FileModel item = CreateSyncFile(masterFile, slaveFile, reverse);
					syncFolder.Files.Add(item);
					Logger.Instance.Add("Criando arquivo de sincronização " + masterFile.Name);
				}
				else
				{
					Logger.Instance.Add("Arquivo de sincronização presente " + masterFile.Name);
				}
			}
		}

		private FolderModel CreateSyncFolder(FolderModel masterFolder, FolderModel slaveFolder)
		{
			if (slaveFolder != null)
			{
				Logger.Instance.Add("Pasta dependente encontrada " + masterFolder.Name);
				return new FolderModel
				{
					Name = slaveFolder.Name,
					ModifyType = slaveFolder.ModifyType,
					Path = slaveFolder.Path
				};
			}
			Logger.Instance.Add("Pasta dependente não encontrada " + masterFolder.Name);
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
				Logger.Instance.Add("Arquivo dependente encontrado " + slaveFile.Name);
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
			Logger.Instance.Add("Arquivo dependente não encontrado " + masterFile.Name);
			ModifyType modifyType = (!reverse) ? ModifyType.Deleted : ModifyType.New;
			Logger.Instance.Add($"Definindo tipo {modifyType} para {masterFile.Name}");
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

		private void WriteDetailedLog(string logFilePath, string message)
		{
			try
			{
				string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
				string logEntry = $"[{timestamp}] {message}";
				File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
			}
			catch (Exception ex)
			{
				Logger.Instance.Add($"Erro ao escrever log detalhado: {ex.Message}");
			}
		}
	}
}
