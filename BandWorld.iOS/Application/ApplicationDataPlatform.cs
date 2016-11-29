using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using BandWorld.MVC.Application;
using BandWorld.MVC.Controllers;
using JTRazorPortable;

namespace BandWorld.iOS
{
	public class ApplicationDataPlatform : ApplicationData
	{
		public static string StartUpMessage = "";
		public static string TextMessage = "";

		public ApplicationDataPlatform(
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

		public override void InitializeApplicationData(
			string applicationName,
			string masterAdministratorUserName,
			bool isDevelopmentVersion,
			string serviceUrl,
			string basePlatformDirectory,
			string contentTildeUrl,
			string mediaVersion)
		{
			base.InitializeApplicationData(
				applicationName,
				masterAdministratorUserName,
				isDevelopmentVersion,
				serviceUrl,
				basePlatformDirectory,
				contentTildeUrl,
				mediaVersion);
		}

		public override void ServiceUrlChanged()
		{
		}

		// Client events.
		public static void ServiceCallback(ClientMode theMode, string messageString)
		{
			if (!String.IsNullOrEmpty(StartUpMessage))
				messageString = StartUpMessage;
			else if (!String.IsNullOrEmpty(TextMessage))
				messageString = TextMessage;

			switch (theMode)
			{
				case ClientMode.StartMessage:
					Global.ProgressOperation_Dispatch(ProgressMode.Show, 0, messageString);
					break;
				case ClientMode.WaitMessage:
					break;
				case ClientMode.StopMessage:
					Global.ProgressOperation_Dispatch(ProgressMode.Hide, 0, messageString);
					break;
				case ClientMode.Error:
				case ClientMode.Canceled:
					Global.ProgressOperation_Dispatch(ProgressMode.Hide, 0, messageString);
					break;
				default:
					throw new Exception("ServiceCallback: Unknown mode: " + theMode.ToString());
			}
		}

		public static void SetMessage_Dispatch(string message)
		{
#if ANDROID
            MainActivity.Global.RunOnUiThread(() => SetMessage(message));
#endif
		}

#if ANDROID
        static bool HaveMessage = false;
#endif

		public static void SetMessage(string message)
		{
#if ANDROID
            TextView progressText = MainActivity.progressText;

            if (progressText != null)
            {
                if (!String.IsNullOrEmpty(message))
                {
                    progressText.Text = message;
                    progressText.Visibility = ViewStates.Visible;
                    HaveMessage = true;
                }
                else if (HaveMessage)
                {
                    progressText.Text = String.Empty;
                    progressText.Visibility = ViewStates.Gone;
                    HaveMessage = false;
                }
            }
#endif
		}

		public override void ProgressOperation_Dispatch(ProgressMode mode, int value, string message)
		{
#if ANDROID
            MainActivity.Global.RunOnUiThread(() => ProgressOperation(mode, value, message));
#endif
		}

		public override void ProgressOperation(ProgressMode mode, int value, string message)
		{
#if ANDROID
            ProgressBar progressSpinner = MainActivity.progressSpinner;
            ProgressBar progressBar = MainActivity.progressBar;
            TextView progressText = MainActivity.progressText;

            ProgressTimerStop();

            if (progressBar == null)
                return;

            switch (mode)
            {
                case ProgressMode.Start:
                    InProgress = true;
                    progressBar.Visibility = ViewStates.Visible;
                    progressBar.Max = value;
                    if (!String.IsNullOrEmpty(message))
                    {
                        progressText.Text = message;
                        progressText.Visibility = ViewStates.Visible;
                        HaveMessage = true;
                    }
                    break;
                case ProgressMode.Update:
                    if (InProgress)
                    {
                        progressBar.Progress = value;
                        if (HaveMessage)
                        {
                            if (!String.IsNullOrEmpty(message))
                                progressText.Text = message;
                        }
                    }
                    break;
                case ProgressMode.Stop:
                    progressBar.Visibility = ViewStates.Gone;
                    progressText.Text = String.Empty;
                    progressText.Visibility = ViewStates.Gone;
                    InProgress = false;
                    HaveMessage = false;
                    break;
                case ProgressMode.Hide:
                    progressSpinner.Visibility = ViewStates.Gone;
                    break;
                case ProgressMode.Show:
                    progressSpinner.Visibility = ViewStates.Visible;
                    break;
                case ProgressMode.DelayedShow:
                    ProgressTimerStart(value);
                    break;
                default:
                    throw new Exception("ProgressOperation: Need mode support for: " + mode.ToString());
            }
#endif
		}

		public override List<string> FontNames()
		{
			List<string> fontFamilyNames = new List<string>();
			fontFamilyNames.Add("sans-serif");
			return fontFamilyNames;
		}

		public override bool IsConnectedToANetwork()
		{
#if ANDROID
            ConnectivityManager connectivityManager =
                (ConnectivityManager)MainActivity.Global.ApplicationContext.GetSystemService(
                    Android.App.Activity.ConnectivityService);
            NetworkInfo activeConnection = connectivityManager.ActiveNetworkInfo;
            bool isOnline = (activeConnection != null) && activeConnection.IsConnected;
            return isOnline;
#elif __IOS__
			return true;
#endif
		}

		public override bool GetDiskSpaceInfo(out long totalSpace, out long usedSpace, out long freeSpace)
		{
#if ANDROID
            Android.OS.StatFs stats = new Android.OS.StatFs("/data");
            int totalBlocks = stats.BlockCount;
            int availableBlocks = stats.AvailableBlocks;
            int blockSizeInBytes = stats.BlockSize;
            totalSpace = (long)totalBlocks * blockSizeInBytes;
            freeSpace = (long)availableBlocks * blockSizeInBytes;
            usedSpace = totalSpace - freeSpace;
#elif __IOS__
			totalSpace = (long)0;
			freeSpace = (long)0;
			usedSpace = totalSpace - freeSpace;
#endif
			return true;
		}

		public override bool FileExists(string filePath)
		{
			return File.Exists(filePath);
		}

		public override long FileSize(string filePath)
		{
			if (!File.Exists(filePath))
				return 0L;

			FileInfo fileInfo = new FileInfo(filePath);
			return fileInfo.Length;
		}

		public override void FileDelete(string filePath)
		{
			File.Delete(filePath);
		}

		public override string FileReadAllText(string filePath)
		{
			return File.ReadAllText(filePath);
		}

		public override bool GetRemoteMediaFile(string remoteUrl, string outputFilePath)
		{
			if (String.IsNullOrEmpty(remoteUrl) || String.IsNullOrEmpty(outputFilePath))
				return false;

			Stream dataBuffer = new FileStream(outputFilePath, FileMode.OpenOrCreate);

			for (int retry = 0; retry < TransferRetryLimit; retry++)
			{
				try
				{
					if (GetFile(remoteUrl, dataBuffer, outputFilePath))
						return true;
				}
				catch (Exception)
				{
				}
			}

			return true;
		}

		//private static System.Net.CookieContainer SiteCookieContainer;

		public override bool GetFile(string url, Stream dataBuffer, string filePath)
		{
			Stream responseStream;
			Stream pageStream;
			bool returnValue = false;

			// Set GET to site.
			HttpWebRequest SiteRequest = (HttpWebRequest)WebRequest.Create(url);
			HttpWebResponse SiteResponse = null;

			SiteRequest.Method = "GET";
			//SiteRequest.AllowAutoRedirect = true;
			//SiteRequest.CookieContainer = SiteCookieContainer;
			SiteRequest.Referer = url;
			SiteRequest.Timeout = TransferTimeoutMsec;

			try
			{
				SiteResponse = (HttpWebResponse)SiteRequest.GetResponse();
				returnValue = true;
			}
			catch (Exception)
			{
				return false;
			}

			returnValue = false;

			try
			{
				// Read page data.
				responseStream = SiteResponse.GetResponseStream();
				long incomingSize = responseStream.Length;
				if (CheckIncomingRemoteFile(filePath, incomingSize))
				{
					pageStream = dataBuffer;
					StreamTransfer(responseStream, pageStream);
					responseStream.Close();
					dataBuffer.Close();
					BookkeepMediaFileAdd(filePath, incomingSize);
					returnValue = true;
				}
			}
			catch (Exception exc)
			{
				System.Console.WriteLine("Error reading \"" + url + "\": " + exc.Message + ".");
			}

			return returnValue;
		}

		public bool StreamTransfer(Stream inStream, Stream outStream)
		{
			const int bufferSize = 0x1000;
			byte[] buffer = new byte[bufferSize];
			int read;

			while ((read = inStream.Read(buffer, 0, bufferSize)) > 0)
				outStream.Write(buffer, 0, read);

			return true;
		}


		// Returns false if thread-qued.
		public override bool RefreshFiles(bool force)
		{
			// Do this as a thread so we don't block.
			RunAsThread(
				threadOp =>
				{
					ResourceManager.EnsureResources(
						typeof(TestController).Assembly,
						BasePlatformDirectory,
						force,
						true);
				},
				continueOp =>
				{
					DispatchToUI(callback => MVCManager.Global.RefreshNoQuery());
				}
			);
			return false;
		}

		// Returns false if thread-qued.
		public override bool RefreshFile(string filePath, bool force)
		{
			// Do this as a thread so we don't block.
			RunAsThread(
				threadOp =>
				{
					string partialPath = filePath.Substring(BasePlatformDirectory.Length);
					string file = GetFileName(partialPath);
					string directoryPath = GetFilePath(partialPath);
					if (directoryPath.StartsWith("/"))
						directoryPath = directoryPath.Substring(1);
					string resourceDirectory = directoryPath.Replace('/', '.');
					string resource = resourceDirectory + "/" + file;
					ResourceManager.EnsureResource(
						typeof(TestController).Assembly,
						filePath,
						resource,
						true);
				},
				continueOp =>
				{
					DispatchToUI(callback => MVCManager.Global.RefreshNoQuery());
				}
			);
			return false;
		}

		public override string EvaluateJavascript(string command)
		{
			return MVCManager.Global.HybridView.EvaluateJavascript(command);
		}

		public override void DispatchToUI(WaitCallback thunk)
		{
			WebViewController.Global.InvokeOnMainThread(() => thunk(null));
		}
	}
}