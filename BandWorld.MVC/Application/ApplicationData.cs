using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace BandWorld.MVC.Application
{
	public enum MediaStorageState
	{
		Unknown,        // We don't know right now.
		Present,        // Media is present.
		Downloaded,     // Media has been downloaded, either on-demand or via a download page.
		Absent,         // Media is not present.
		External,       // The media is an external reference - do not download.
		BadLink         // The url is bad, or there was an network outage.
	};

	public enum ProgressMode
	{
		Start,
		Update,
		Stop,
		Hide,
		Show,
		DelayedShow
	}

	public enum ClientMode { StartMessage, WaitMessage, StopMessage, Canceled, Error };
	public delegate void ClientWaitCallback(ClientMode mode, string messageString);

	public class ApplicationData
	{
		// Settings managed by InitializeApplicationData
		public static string ApplicationName { get; set; }
		public static string ServiceUrl { get; set; }
		public static string ContentTildeUrl { get; set; }
		public static string DatabaseTildeUrl { get; set; }
		public static string ImagesTildeUrl { get; set; }
		public static string MediaTildeUrl { get; set; }
		public static string PicturesTildeUrl { get; set; }
		public static string SandboxTildeUrl { get; set; }
		public static string TempTildeUrl { get; set; }
		public static string ContentPath { get; set; }
		public static string DatabasePath { get; set; }
		public static string ImagesPath { get; set; }
		public static string MediaPath { get; set; }
		public static string PicturesPath { get; set; }
		public static string SandboxPath { get; set; }
		public static string TempPath { get; set; }

		// Settings managed or overridable by classes derived for platform.
		public static bool IsDevelopmentVersion { get; set; }
		public static bool IsMobileVersion { get; set; }
		public static string BasePlatformDirectory { get; set; }
		public static string MediaVersion { get; set; }
		public static UTF8Encoding Encoding { get; set; }
		public static string PlatformPathSeparator { get; set; }
		public static string MasterAdministratorUserName { get; set; }

		// These flags control how content in the mobile version is referenced.

		// Copy media content when it is referenced, saving it locally for future use.
		public static bool CopyRemoteMediaOnDemand { get; set; }
		// Limit copied media to the size specified by DownloadedMediaSizeLimit.
		public static bool LimitRemoteMedia { get; set; }
		// Limit copied media to the to this size threshhold.
		public static long DownloadedMediaSizeLimit { get; set; }
		// If the downloaded media size limit is exceeded, delete local copies of older media.
		public static bool AutoDeleteOlderData { get; set; }
		// Timeout for media transfers.
		public static int TransferTimeoutMsec { get; set; }
		// Retry limit for media transfers.
		public static int TransferRetryLimit { get; set; }

		// Cached current total media size.
		public static long CurrentTotalMediaSize { get; set; }
		// Include media files in tree install package.
		public static bool IncludeMediaFilesInPackage { get; set; }

		// Global pointer to the ApplicationData instance.
		public static ApplicationData Global;

		// Delagate definitions.
		public delegate string MapToFilePathDelegate(string tildePath); // Set before
		public delegate void DumpString(string text);

		// Delegate pointers managed by derived classes.
		// These must be set before InitializeApplicationData is called. 
		// ClearApplicationData and InitializeApplicationData must not change these!
		public static MapToFilePathDelegate MapToFilePath;

		public ApplicationData()
		{
			// Don't clear the members.  Let InitializeApplicationData make sure everything is
			// initialized, both here and in the derived classes.
		}

		// Only for the case where there is no derived class used.
		public ApplicationData(
			string applicationName,
			string masterAdministratorUserName,
			bool isDevelopmentVersion,
			string serviceUrl,
			string basePlatformDirectory,
			string contentTildeUrl,
			string mediaVersion)
		{
			InitializeApplicationData(
				applicationName,
				masterAdministratorUserName,
				isDevelopmentVersion,
				serviceUrl,
				basePlatformDirectory,
				contentTildeUrl,
				mediaVersion);
		}

		public void ClearApplicationData()
		{
			ApplicationName = String.Empty;
			ServiceUrl = null;
			ContentTildeUrl = String.Empty;
			DatabaseTildeUrl = String.Empty;
			ImagesTildeUrl = String.Empty;
			MediaTildeUrl = String.Empty;
			PicturesTildeUrl = String.Empty;
			SandboxTildeUrl = String.Empty;
			TempTildeUrl = String.Empty;
			ContentPath = String.Empty;
			DatabasePath = String.Empty;
			ImagesPath = String.Empty;
			MediaPath = String.Empty;
			PicturesPath = String.Empty;
			SandboxPath = String.Empty;
			TempPath = String.Empty;

			Global = null;

			IsDevelopmentVersion = false;
			IsMobileVersion = false;
			BasePlatformDirectory = null;
			Encoding = null;
			PlatformPathSeparator = String.Empty;
			MasterAdministratorUserName = String.Empty;

			CopyRemoteMediaOnDemand = false;
			LimitRemoteMedia = false;
			DownloadedMediaSizeLimit = 0L;
			AutoDeleteOlderData = false;
			TransferTimeoutMsec = 30000;
			TransferRetryLimit = 3;
			CurrentTotalMediaSize = 0;
			IncludeMediaFilesInPackage = false;
		}

		public virtual void InitializeApplicationData(
				string applicationName,
				string masterAdministratorUserName,
				bool isDevelopmentVersion,
				string serviceUrl,
				string basePlatformDirectory,
				string contentTildeUrl,
				string mediaVersion)
		{
			InitializeApplicationDataStatic(
				applicationName,
				masterAdministratorUserName,
				isDevelopmentVersion,
				serviceUrl,
				basePlatformDirectory,
				contentTildeUrl,
				mediaVersion);

			Global = this;
		}

		public static void InitializeApplicationDataStatic(
				string applicationName,
				string masterAdministratorUserName,
				bool isDevelopmentVersion,
				string serviceUrl,
				string basePlatformDirectory,
				string contentTildeUrl,
				string mediaVersion)
		{
			ApplicationName = applicationName;
			ServiceUrl = serviceUrl;
			ContentTildeUrl = contentTildeUrl;
			DatabaseTildeUrl = contentTildeUrl + "/" + "Database";
			ImagesTildeUrl = contentTildeUrl + "/" + "Images";
			MediaTildeUrl = contentTildeUrl + "/" + "Media";
			PicturesTildeUrl = contentTildeUrl + "/" + "Pictures";
			SandboxTildeUrl = contentTildeUrl + "/" + "Sandbox";
			TempTildeUrl = contentTildeUrl + "/" + "Temp";
			ContentPath = MapToFilePath(ContentTildeUrl);
			DatabasePath = MapToFilePath(DatabaseTildeUrl);
			ImagesPath = MapToFilePath(ImagesTildeUrl);
			MediaPath = MapToFilePath(MediaTildeUrl);
			PicturesPath = MapToFilePath(PicturesTildeUrl);
			SandboxPath = MapToFilePath(SandboxTildeUrl);
			TempPath = MapToFilePath(TempTildeUrl);

			// These still can be optionally overridden by the platform.
			IsDevelopmentVersion = isDevelopmentVersion;
			IsMobileVersion = false;
			BasePlatformDirectory = basePlatformDirectory;
			MediaVersion = mediaVersion;
			Encoding = new UTF8Encoding(true, true);
			PlatformPathSeparator = @"\";
			MasterAdministratorUserName = masterAdministratorUserName;

			// Media option defaults.
			CopyRemoteMediaOnDemand = false;
			LimitRemoteMedia = false;
			DownloadedMediaSizeLimit = 0L;
			AutoDeleteOlderData = false;
			TransferTimeoutMsec = 30000;
			TransferRetryLimit = 3;
			CurrentTotalMediaSize = 0;
			IncludeMediaFilesInPackage = false;
		}

		// Call before InitializeApplicationData is called, or set MapToFilePath directly.
		public static void SetUpMapToFile(string basePlatformDirectory)
		{
			BasePlatformDirectory = basePlatformDirectory;
			MapToFilePath = MapToFilePathGeneric;
		}

		public static string MapToFilePathGeneric(string url)
		{
			string filePath = url;

			if (String.IsNullOrEmpty(filePath))
				return filePath;

			if (filePath.StartsWith("~"))
				filePath = filePath.Substring(1);

			if (filePath.StartsWith("/"))
				filePath = filePath.Substring(1);

			filePath = BasePlatformDirectory + "/" + filePath;

			return filePath;
		}

		public static void LoadMediaOptions()
		{
			/*
            ServiceUrl = Settings.GetString("ServiceUrl", ServiceUrl);
            CopyRemoteMediaOnDemand = Settings.GetFlag("CopyRemoteMediaOnDemand", CopyRemoteMediaOnDemand);
            LimitRemoteMedia = Settings.GetFlag("LimitRemoteMedia", LimitRemoteMedia);
            DownloadedMediaSizeLimit = Settings.GetLong("DownloadedMediaSizeLimit", DownloadedMediaSizeLimit);
            AutoDeleteOlderData = Settings.GetFlag("AutoDeleteOlderData", AutoDeleteOlderData);
            TransferTimeoutMsec = Settings.GetInteger("TransferTimeoutMsec", TransferTimeoutMsec);
            TransferRetryLimit = Settings.GetInteger("TransferRetryLimit", TransferRetryLimit);
            CurrentTotalMediaSize = Settings.GetLong("CurrentTotalMediaSize", 0L);
            */

		}

		public static void SaveMediaOptions()
		{
			/*
            Settings.SetString("ServiceUrl", ServiceUrl);
            Settings.SetFlag("CopyRemoteMediaOnDemand", CopyRemoteMediaOnDemand);
            Settings.SetFlag("LimitRemoteMedia", LimitRemoteMedia);
            Settings.SetLong("DownloadedMediaSizeLimit", DownloadedMediaSizeLimit);
            Settings.SetFlag("AutoDeleteOlderData", AutoDeleteOlderData);
            Settings.SetInteger("TransferTimeoutMsec", TransferTimeoutMsec);
            Settings.SetInteger("TransferRetryLimit", TransferRetryLimit);
            */
		}

		public virtual void ServiceUrlChanged()
		{
		}

		public virtual bool DirectoryExistsCheck(string filePath)
		{
			return false;
		}

		public virtual bool FileExists(string filePath)
		{
			return false;
		}

		public virtual long FileSize(string filePath)
		{
			return -1L;
		}

		public virtual void FileDelete(string filePath)
		{
		}

		public virtual string FileReadAllText(string filePath)
		{
			return String.Empty;
		}

		public virtual void FileWriteAllText(string filePath, string contents)
		{
		}

		public string GetFilePath(string filePath)
		{
			if (String.IsNullOrEmpty(filePath))
				return String.Empty;
			int offset1 = filePath.LastIndexOf('\\');
			int offset2 = filePath.LastIndexOf('/');
			int offset;
			if (offset1 > offset2)
				offset = offset1;
			else
				offset = offset2;
			if (offset < 0)
				return String.Empty;
			string path = filePath.Substring(0, offset);
			return path;
		}

		public string GetFileName(string filePath)
		{
			string fileName = filePath;
			char[] delimiters = { '/', '\\' };
			string[] parts = filePath.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
			if ((parts != null) && (parts.Count() != 0))
				fileName = parts[parts.Count() - 1];
			int offset = fileName.IndexOf('?');
			if (offset > 0)
				fileName = fileName.Substring(0, offset);
			return fileName;
		}

		public virtual bool HandleMediaAccess(ref string url, ref MediaStorageState storageState, out bool changed)
		{
			bool returnValue = true;

			changed = false;

			if (String.IsNullOrEmpty(url))
				return false;

			MediaStorageState oldState = storageState;

			switch (storageState)
			{
				case MediaStorageState.Unknown:
				case MediaStorageState.Present:
				case MediaStorageState.Downloaded:
					if (url.StartsWith("~"))
					{
						string path = MapToFilePath(url);
						if (FileExists(path))
						{
							if (storageState == MediaStorageState.Unknown)
								storageState = MediaStorageState.Present;
						}
						else if (CopyRemoteMediaOnDemand)
						{
							if (IsConnectedToANetwork())
							{
								string remoteUrl = GetRemoteMediaUrlFromTildeUrl(url);

								if (GetRemoteMediaFile(remoteUrl, path))
									storageState = MediaStorageState.Downloaded;
								else
								{
									storageState = MediaStorageState.BadLink;
									returnValue = false;
								}
							}
							else
								returnValue = false;
						}
						else
							returnValue = false;
					}
					else
						storageState = MediaStorageState.External;
					break;
				case MediaStorageState.External:
					break;
				case MediaStorageState.BadLink:
					returnValue = false;
					break;
				default:
					break;
			}

			if (oldState != storageState)
				changed = true;

			return returnValue;
		}

		public bool GetRemoteMediaFilesFromFilePaths(
			List<string> mediaFiles, bool overwrite, out string errorMessage)
		{
			int count = mediaFiles.Count;
			int index = 0;
			bool returnValue = true;

			errorMessage = String.Empty;

			if (ApplicationData.Global.IsConnectedToANetwork())
			{
				ProgressOperation_Dispatch(ProgressMode.Start, count, "Downloading media files...");

				foreach (string filePath in mediaFiles)
				{
					string remoteUrl = ApplicationData.GetRemoteMediaUrlFromFilePath(filePath);
					string fileName = GetFileName(remoteUrl);

					if (overwrite || !FileExists(filePath))
					{
						ProgressOperation_Dispatch(ProgressMode.Update, index, "Getting: " + fileName);

						if (!ApplicationData.Global.GetRemoteMediaFile(remoteUrl, filePath))
						{
							errorMessage = errorMessage
								+ "Error getting remote file: " + remoteUrl + "\n";
							returnValue = false;
						}
					}

					index++;
				}

				ProgressOperation_Dispatch(ProgressMode.Stop, index, null);
				//// Let PostHandleRequest operation stop the progress.
				//ProgressOperation_Dispatch(ProgressMode.Update, index, "Finished server communication.");
			}
			else
			{
				errorMessage = "Sorry, can't download files because there is no network connection.";
				returnValue = false;
			}

			return returnValue;
		}

		public virtual bool IsConnectedToANetwork()
		{
			return false;
		}

		public virtual bool GetDiskSpaceInfo(out long totalSpace, out long usedSpace, out long freeSpace)
		{
			totalSpace = 0L;
			usedSpace = 0L;
			freeSpace = 0L;
			return false;
		}

		public static string GetRemoteMediaUrlFromTildeUrl(string tildeUrl)
		{
			string returnValue = tildeUrl;

			if (String.IsNullOrEmpty(tildeUrl))
				return returnValue;

			if (tildeUrl.StartsWith("~"))
				returnValue = ServiceUrl + tildeUrl.Substring(1);

			return returnValue;
		}

		public static string GetRemoteMediaUrlFromFilePath(string filePath)
		{
			string returnValue = String.Empty;

			if (String.IsNullOrEmpty(filePath))
				return returnValue;

			string serviceUrl = ServiceUrl;

			if (!serviceUrl.EndsWith("/"))
				serviceUrl = serviceUrl + "/";

			string basePlatformDirectory = BasePlatformDirectory;

			if (basePlatformDirectory.EndsWith("/") || basePlatformDirectory.EndsWith(@"\"))
				basePlatformDirectory = basePlatformDirectory.Substring(0, basePlatformDirectory.Length);

			if (filePath.StartsWith(basePlatformDirectory))
				returnValue = ServiceUrl + filePath.Substring(basePlatformDirectory.Length).Replace(@"\", "/");

			return returnValue;
		}

		public virtual bool MaybeGetRemoteMediaFile(string remoteUrl, string outputFilePath)
		{
			return false;
		}

		public virtual bool GetRemoteMediaFile(string remoteUrl, string outputFilePath)
		{
			return false;
		}

		public virtual bool GetFile(string url, Stream dataBuffer, string filePath)
		{
			return false;
		}

		public virtual bool CheckIncomingRemoteFile(string filePath, long length)
		{
			if (LimitRemoteMedia)
			{
				long newSize = CurrentTotalMediaSize + length;
				long freedSize = 0;

				if (newSize > DownloadedMediaSizeLimit)
				{
					if (!AutoDeleteOlderData)
						return false;

					long sizeToFree = newSize - DownloadedMediaSizeLimit;
					freedSize = FreeUpMediaSpace(sizeToFree);

					UpdateTotalMediaSize(length - freedSize);
				}
			}

			return true;
		}

		// Returns false if thread-qued.
		public virtual bool RefreshFiles(bool force)
		{
			return true;
		}

		// Returns false if thread-qued.
		public virtual bool RefreshFile(string filePath, bool force)
		{
			return true;
		}

		public List<string> BookkeptMediaFiles
		{
			get
			{
				throw new Exception("Need implementation.");
			}
			set
			{
			}
		}

		public virtual void BookkeepMediaFileRemove(string filePath, long size)
		{
			List<string> mediaFiles = BookkeptMediaFiles;

			if (mediaFiles.Remove(filePath))
				BookkeptMediaFiles = mediaFiles;

			UpdateTotalMediaSize(-size);
		}

		public virtual void BookkeepMediaFileAdd(string filePath, long size)
		{
			if (String.IsNullOrEmpty(filePath))
				return;

			List<string> mediaFiles = BookkeptMediaFiles;

			if (!mediaFiles.Contains(filePath))
				mediaFiles.Add(filePath);

			BookkeptMediaFiles = mediaFiles;

			UpdateTotalMediaSize(size);
		}

		public virtual long FreeUpMediaSpace(long sizeToFree)
		{
			long freedUpSpace = 0;
			List<string> mediaFiles = BookkeptMediaFiles;
			int index = 0;

			while (index < mediaFiles.Count)
			{
				string filePath = mediaFiles[index];

				long size = FileSize(filePath);

				try
				{
					FileDelete(filePath);
					mediaFiles.RemoveAt(0);
					freedUpSpace += size;

					if (freedUpSpace >= sizeToFree)
						break;
				}
				catch (Exception)
				{
					index++;
				}
			}

			if (freedUpSpace != 0)
				BookkeptMediaFiles = mediaFiles;

			return freedUpSpace;
		}

		public virtual bool UpdateTotalMediaSize(long delta)
		{
			CurrentTotalMediaSize = CurrentTotalMediaSize + delta;
			//Settings.SetLong("CurrentTotalMediaSize", CurrentTotalMediaSize);
			return true;
		}

		public virtual void RunAsThread(WaitCallback threadOp, WaitCallback continueOp)
		{
			ThreadPool.QueueUserWorkItem(o => SubThread(threadOp, continueOp));
		}

		public void SubThread(WaitCallback threadOp, WaitCallback continueOp)
		{
			threadOp(null);
			continueOp(null);
		}

		protected bool InProgress = false;
		protected bool InDelayedProgress = false;
		protected bool InPageTransition = false;
		protected static Timer ProgressDelayedTimer = null;
		public static int ProgressDelayMsec = 1000;

		public virtual void ProgressOperation_Dispatch(ProgressMode mode, int value, string message)
		{
		}

		public virtual void ProgressOperation(ProgressMode mode, int value, string message)
		{
		}

		protected void ProgressTimerStart(int msecDelay)
		{
			if (ProgressDelayedTimer == null)
			{
				InDelayedProgress = true;
				ProgressDelayedTimer = new Timer(ProgressTimerCallback, null, msecDelay, Timeout.Infinite);
			}
		}

		protected void ProgressTimerStop()
		{
			if (ProgressDelayedTimer != null)
			{
				ProgressDelayedTimer.Dispose();
				ProgressDelayedTimer = null;
			}
			InDelayedProgress = false;
		}

		private void ProgressTimerCallback(object state)
		{
			if (ProgressDelayedTimer != null)
			{
				ProgressDelayedTimer.Dispose();
				ProgressDelayedTimer = null;
			}
			else
			{
				InDelayedProgress = false;
				return;
			}

			if (InDelayedProgress)
			{
				ProgressOperation_Dispatch(ProgressMode.Show, 0, null);
				InDelayedProgress = false;
			}
		}

		public bool PreHandleRequest(string url)
		{
			if (InProgress)
			{
				ProgressOperation_Dispatch(ProgressMode.Hide, 0, null);
				InPageTransition = false;
			}
			else
			{
				InPageTransition = true;
				ProgressOperation_Dispatch(ProgressMode.DelayedShow, ProgressDelayMsec, null);
			}

			return true;
		}

		public bool PostHandleRequest(string url)
		{
			if (InPageTransition)
			{
				ProgressOperation_Dispatch(ProgressMode.Hide, 0, null);
				InPageTransition = false;
			}

			return true;
		}

		public string GetSandboxFileUrl(string userName, string fileName)
		{
			string url = SandboxTildeUrl + "/" + userName + "/" + fileName;
			return url;
		}

		public string GetSandboxFileName(string userName, string fileName)
		{
			string filePath = SandboxPath + PlatformPathSeparator + userName + PlatformPathSeparator + fileName;
			return filePath;
		}

		public string GetTempFileUrl(string extension)
		{
			string fileName = DateTime.UtcNow.Ticks.ToString() + extension;
			string fileUrl = TempPath + "/" + fileName;
			return fileUrl;
		}

		public string GetTempFileName(string extension)
		{
			string fileName = DateTime.UtcNow.Ticks.ToString() + extension;
			string filePath = TempPath + PlatformPathSeparator + fileName;
			return filePath;
		}

		public string GetTempFileUrl(string userName, string fileName)
		{
			string fileUrl = TempPath + "/" + userName + "/" + fileName;
			return fileUrl;
		}

		public string GetTempFileName(string userName, string fileName)
		{
			string filePath = TempPath + PlatformPathSeparator + userName + PlatformPathSeparator + fileName;
			return filePath;
		}

		public virtual List<string> FontNames()
		{
			return new List<string>();
		}

		public static MediaStorageState GetStorageStateFromString(string str)
		{
			MediaStorageState storageState;

			switch (str)
			{
				case "Unknown":
					storageState = MediaStorageState.Unknown;
					break;
				case "Present":
					storageState = MediaStorageState.Present;
					break;
				case "Downloaded":
					storageState = MediaStorageState.Downloaded;
					break;
				case "Absent":
					storageState = MediaStorageState.Absent;
					break;
				case "External":
					storageState = MediaStorageState.External;
					break;
				case "BadLink":
					storageState = MediaStorageState.BadLink;
					break;
				default:
					throw new Exception("ApplicationData.GetStorageStateFromString: Unknown storage state: "
						+ str);
			}

			return storageState;
		}

		public virtual string EvaluateJavascript(string command)
		{
			return null;
		}

		public virtual void DispatchToUI(WaitCallback thunk)
		{
		}
	}
}
