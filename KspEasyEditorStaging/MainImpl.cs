using UnityEngine;
using System.Collections.Generic;
using System.IO;
using KSP.UI.Screens;
using KspNalCommon;

namespace KspEasyEditorStaging {

	public class KspEasyEditorStagingProperties : CommonPluginProperties {
		public bool canGetIsDebug() {
			return true;
		}

		public int getInitialWindowId() {
			return 3430924;
		}

		public string getPluginDirectory() {
			return "EasyEditorStaging";
		}

		public string getPluginLogName() {
			return "EasyEditorStaging";
		}

		public bool isDebug() {
			return true;
		}

		public GUISkin kspSkin() {
			return HighLogic.Skin;
		}
	}


	/**
	 * This class does not inherit from MonoBehaviour to make it easy to use Kramax reloader in debug builds and do not use it in normal builds.
	 */
	public class MainImpl {

		private List<BaseWindow> windows = new List<BaseWindow>();
		private List<ApplicationLauncherButton> appLauncherButtons = new List<ApplicationLauncherButton>();

		public void Start() {
			PluginCommons.init(new KspEasyEditorStagingProperties());

			PluginLogger.logDebug("plugin start");


			StagingWindow stagingWindow = addWindow(new StagingWindow());

			ApplicationLauncherButton button = null;

			Texture2D texture;
			texture = Texture2D.whiteTexture;


			button = ApplicationLauncher.Instance.AddModApplication(
				delegate () {
					onTrue();
					//button.SetFalse(false);
					//foreach (Part p in EditorLogic.SortedShipList) {
					//	PluginLogger.logDebug("DEBUG1: " + p.partInfo.name + ":" + p.GetInstanceID() + ", " + p.inStageIndex + "," + p.inverseStage + "," + p.stagingOn);

					//	//p.highlight(new Color(1, 0, 0));
					//	p.SetHighlight(false, false);

					//}
					//EditorLogic.RootPart.highlight(new Color(0, 0, 1));
					//EditorLogic.RootPart.SetHighlight(false, false);
					//PluginLogger.logDebug("DEBUG: " + EditorLogic.RootPart.partName);
					stagingWindow.displayWindow();
				}, delegate () {
					stagingWindow.hideWindow();
					onFalse();
				}, null, null, null, null,
				ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH, texture);
			appLauncherButtons.Add(button);

		}

		private void onTrue() {
			
		}


		private void onFalse() {
			
		}

		private T addWindow<T>(T newWindow) where T : BaseWindow {
			windows.Add(newWindow);
			return newWindow;
		}


		private void CleanUp() {
			PluginLogger.logDebug("CleanUp in " + EditorDriver.editorFacility);

			foreach (ApplicationLauncherButton button in appLauncherButtons) {
				ApplicationLauncher.Instance.RemoveModApplication(button);
			}

			foreach (BaseWindow window in windows) {
				window.hideWindow();
			}
		}

		public void Update() {
			foreach (BaseWindow window in windows) {
				window.update();
			}
		}

		public void OnGUI() {
			//if (Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout) {
			//	COLogger.Log(Event.current);
			//}
			foreach (BaseWindow window in windows) {
				window.onGUI();
			}
		}


		public void OnDestroy() {
			PluginLogger.logDebug("OnDestroy in " + EditorDriver.editorFacility);
			CleanUp();
		}

		public void OnDisable() {
			PluginLogger.logDebug("OnDisable in " + EditorDriver.editorFacility);
		}
	}
}

