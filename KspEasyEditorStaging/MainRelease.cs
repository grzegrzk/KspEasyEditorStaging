#if !DEBUG

using KspEasyEditorStaging;
using UnityEngine;


[KSPAddon(KSPAddon.Startup.EditorAny, false)]
public class KspEasyEditorStagingMainRelease : MonoBehaviour {

	MainImpl impl = new MainImpl();

	public void Start() {
		EESLogger.logDebug("Start in Release mode");
		impl.Start();
	}

	public void Update() {
		impl.Update();
	}

	public void OnGUI() {
		impl.OnGUI();
	}

	public void OnDestroy() {
		impl.OnDestroy();
	}

	public void OnDisable() {
		impl.OnDisable();
	}
}

#endif