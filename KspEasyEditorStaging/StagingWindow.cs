using System;
using KspNalCommon;
using UnityEngine;
using System.Collections.Generic;
using KSP.UI.Screens;
using UnityEngine.UI;
using System.Reflection;

namespace KspEasyEditorStaging {
	public class StageIconHelper : StageIcon {
		
		public void doNotCallThisThisIsJustTestIfStageIconPropertyIsAvailable() {
			base.iconImage = null;//this is here just to get clear compilation error once KSP changes name of this property
		}

		public static RawImage getStageTexture(StageIcon stageIcon) {

			if (stageIcon.GetType().GetField("iconImage", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public) == null) {
				PluginLogger.logDebug("field null!");
			}
			return (RawImage)stageIcon.GetType().GetField("iconImage", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(stageIcon);
		}

	}
	public class EntryInEasyStage{
		public int indexInStage { get; private set;}
		public EntryInEasyStage(int i) {
			this.indexInStage = i;
		}
		public List<Part> parts = new List<Part>();
		internal Rect rect = Globals.ZERO_RECT;
	}

	public class EasyStage {
		public int index;
		public List<EntryInEasyStage> entries = new List<EntryInEasyStage>();

		public EasyStage(int index) {
			this.index = index;
		}

		public void addEntryFromPart(Part p) {
			enlargeEntries(p.inStageIndex);
			entries[p.inStageIndex].parts.Add(p);
		}

		private void enlargeEntries(int goal) {
			if (entries.Count <= goal) {
				for (int i = entries.Count; i <= goal; ++i) {
					entries.Add(new EntryInEasyStage(i));
				}
			}
		}
	}

	public class EasyStages {
		public List<EasyStage> stages = new List<EasyStage>();

		public void addPart(Part p) {
			enlargeStages(p.inverseStage);
			stages[p.inverseStage].addEntryFromPart(p);
		}

		private void enlargeStages(int goal) {
			if (stages.Count <= goal) {
				for (int i = stages.Count; i <= goal; ++i) {
					stages.Add(new EasyStage(i));
				}
			}
		}
	}

	public class StagingWindow : BaseWindow {

		bool wasLocked = false;

		private EasyStages easyStages = new EasyStages();
		public StagingWindow() : base("Edit staging") {
		}

		override protected float getWindowHeightOnScreen(Rect pos) {
			return 500;
		}

		protected override float getWindowWidthOnScreen(Rect pos) {
			return 400;
		}

		override protected float getMinWindowInnerWidth(Rect pos) {
			return 400;
		}

		Vector2 scrollPos;
		bool stagingCamera_;

		bool stagingCamera {
			get {
				return stagingCamera_;
			}
			set {
				if (value != stagingCamera_) {
					stagingCamera_ = value;
					if (value) {
						//EditorLogic.fetch.editorCamera.
						//KSPBasics.instance.lockEditor();
						//EditorLogic.fetch.enabled = false;
						//EditorLogic.fetch.editorCamera.fieldOfView += 50;

						//EditorLogic.fetch.editorBounds.Expand(100);
						EditorBounds.Instance.cameraMaxDistance += 200;
						EditorBounds.Instance.cameraOffsetBounds.Expand(200);
						EditorLogic.fetch.editorBounds.Expand(200);
						VABCamera cameraVAB = Camera.main.GetComponent<VABCamera>();
						if (cameraVAB != null) {
							PluginLogger.logDebug("Found VAB camera");
							cameraVAB.maxDistance += 200;
						}

						EditorBounds.Instance.cameraOffsetBounds.Expand(200);
						EditorBounds.Instance.constructionBounds.Expand(200);

						//EditorLogic.fetch.editorCamera.transform.localPosition -= new Vector3(0, 0, 100); ;
						EditorDriver.fetch.vabCamera.maxDistance += 200;
						//EditorDriver.fetch.vabCamera.Distanc += 100;
						EditorDriver.fetch.vabCamera.PlaceCamera(Vector3.zero, 100);


					} else {
						//KSPBasics.instance.unlockEditor();
						//EditorLogic.fetch.enabled = true;
						//EditorLogic.fetch.editorCamera.fieldOfView -= 50;


						EditorBounds.Instance.cameraMaxDistance -= 100;
						EditorLogic.fetch.editorCamera.transform.localPosition += new Vector3(0, 0, 100);
					}
				}
			}
		}

		public override void update() {
			base.update();

			easyStages = new EasyStages();
			foreach (Part p in EditorLogic.SortedShipList) {
				if (p.hasStagingIcon && p.stagingOn) {
					easyStages.addPart(p);
				}
			}

			if (stagingCamera) {
				KSPBasics.instance.lockEditor();
				wasLocked = true;
				//PluginLogger.logDebug("-----");
				//PluginLogger.logDebug(EditorLogic.fetch.editorCamera.transform.rotation);//<- this one is changed when user rotates camera
				//PluginLogger.logDebug(EditorLogic.fetch.editorCamera.transform.localPosition);//<- .z value is changed to zoom
				//PluginLogger.logDebug(EditorLogic.fetch.editorCamera.transform.parent.localPosition);//<- .y value is changed to move up/down
				//PluginLogger.logDebug("-----");

			} else if (wasLocked) {
				KSPBasics.instance.unlockEditor();
				wasLocked = false;
			}
		}


		protected override void windowGUI(int WindowID) {
			using (var scrollViewScope = new GUILayout.ScrollViewScope(scrollPos)) {
				scrollPos = scrollViewScope.scrollPosition;
				using (new GUILayout.VerticalScope()) {
					
					foreach (EasyStage stage in easyStages.stages) {
						GUILayout.Label(stage.index + ":");
						foreach (var stageEntry in stage.entries) {
							//RUI.Icons.Simple.IconLoader

							//GameObject icon = GameObject.Instantiate(stageEntry.parts[0].partInfo.iconPrefab);

							if (stageEntry.parts.Count > 0) {
								using (new GUILayout.HorizontalScope()) {
									RawImage texture = StageIconHelper.getStageTexture(stageEntry.parts[0].stackIcon.StageIcon);
									//GUI.DrawTextureWithTexCoords(new Rect(10, 50 * stageEntry.indexInStage, 50, 50), texture.texture, texture.uvRect);
									GUILayout.Space(40);
									if (Event.current.type == EventType.Repaint) {
										stageEntry.rect = GUILayoutUtility.GetLastRect();
										stageEntry.rect.height = stageEntry.rect.width = 40;
									}
									GUI.DrawTextureWithTexCoords(stageEntry.rect, texture.texture, texture.uvRect);
									if (GUILayout.Button(stageEntry.parts[0].partInfo.name + "(" + stageEntry.parts.Count + ") " + stageEntry.parts[0].craftID)) {
										highlight(stageEntry);
									}
									if (GUILayout.Button("Move to 0")) {

										//EditorLogic.fetch.SetBackup();

										stageEntry.parts[0].inverseStage = 0;

										for (int j = StageManager.Instance.Stages.Count - 1; j >= 0; j--) {
											UnityEngine.Object.Destroy(StageManager.Instance.Stages[j].gameObject);
										}
										StageManager.Instance.Stages.Clear();

										//The only sane way to restet staging manager is to create two ship backups one after another and then simulate "undo" action.
										//The other options would be to save ship to disc and load it immediately, but this completely wipes out undo history
										//EditorLogic.fetch.SetBackup();
										//EditorLogic.fetch.SetBackup();
										//EditorLogic.fetch.GetType().GetMethod("RestoreState", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(EditorLogic.fetch, new object[] { -1 });

										//EditorLogic.fetch.SetBackup();
										//if (this.selectedPart != null) {
										//	this.displayAttachNodeIcons(false, false, false);
										//}
										//if (this.ship.Any<Part>()) {
										//	Object.Destroy(this.rootPart.gameObject);
										//}
										//this.undoLevel = num;
										//this.ship = ShipConstruction.RestoreBackup(EditorLogic.fetch.undoLevel - 1);
										////this.rootPart = this.ship.parts[0].localRoot;
										////ShipConstruction.SanitizeCraftIDs(this.ship.parts, false);
										////ShipConstruction.ShipConfig = this.ship.SaveShip();
										//GameEvents.onEditorRestoreState.Fire();
										////this.RefreshCrewAssignment(ShipConstruction.ShipConfig, this.GetPartExistsFilter());
										//GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
										//GameEvents.onEditorUndo.Fire(EditorLogic.fetch.ship);
										//this.fsm.RunEvent(this.on_undoRedo);


										//StageManager.CurrentStage = 0;
										//StageManager.Instance.SortIcons(true);
										//StageManager.SetSeparationIndices(EditorLogic.RootPart, 0);

										//stageEntry.parts[0].stackIcon.StageIcon.group
										//StageManager.SetSeparationIndices(EditorLogic.RootPart, 0);
										//StageManager.Instance.UpdateStageGroups(false);
										//StageManager.Instance.Stages.Clear();
										//StageManager.GenerateStagingSequence(EditorLogic.RootPart);
										//StageManager.Instance.SortIcons(false);

										//EditorLogic editor = EditorLogic.fetch;
										//ConfigNode shipCfg = editor.ship.SaveShip();

										//string filename = "saves/" + HighLogic.SaveFolder + "/Ships/VAB/StagingEditor.craft.hidden";
										//shipCfg.Save(filename);
										//EditorLogic.LoadShipFromFile(filename);
										//EditorLogic.fetch.SetBackup();

										//ProtoStageIcon i;

										//stageEntry.parts[0].stackIcon.Highlight(true);

										//KSP.UI.Screens.StageIcon stageIcon = UnityEngine.Object.Instantiate<KSP.UI.Screens.StageIcon>(KSP.UI.Screens.StageManager.Instance.stageIconPrefab);
										//stageIcon.Setup(stageEntry.parts[0].stackIcon);
										//stageIcon.transform.localPosition = new Vector3(100, 100, 0);
										//stageIcon.transform.setPa

										//PluginLogger.logDebug(Globals.join( new RUI.Icons.Selectable.IconLoader().iconDictionary.Keys, ","));
										//stageEntry.parts[0].stackIcon.StageIcon.ProtoIcon.Stage

										//stageEntry.parts[0].stagingIcon;
										//PluginLogger.logDebug("debug:" + stageEntry.parts[0].stackIcon.);
									}
								}
							}
						}


						//if (GUILayout.Button(p.partInfo.name + (p.symmetryCounterparts.Count > 0 ? "--" + p.symmetryCounterparts.Count : ""))) {
						//	p.highlight(new Color(1, 0, 0));
						//	drawLine(p.transform.position);
						//	foreach (Part p2 in p.symmetryCounterparts) {
						//		p2.highlight(new Color(0, 1, 0));
						//	}
						//}
					}
					//foreach (Part p in EditorLogic.SortedShipList) {
					//	if (p.hasStagingIcon && p.stagingOn) {
					//		if (GUILayout.Button(p.partInfo.name + (p.symmetryCounterparts.Count > 0 ? "--" + p.symmetryCounterparts.Count : ""))) {
					//			p.highlight(new Color(1, 0, 0));
					//			drawLine(p.transform.position);
					//			foreach (Part p2 in p.symmetryCounterparts) {
					//				p2.highlight(new Color(0, 1, 0));
					//			}
					//		}
					//	}
					//}
				}
			}
			stagingCamera = GUILayout.Toggle(stagingCamera, "Staging camera");
			if (GUILayout.Button("Debug")) {
				PluginLogger.logDebug(EditorLogic.fetch.editorCamera.transform.position);
				PluginLogger.logDebug(EditorLogic.fetch.editorCamera.transform.forward);
				destroyHighlightObjects();
			}
			if (GUILayout.Button("apply")) {
				EditorLogic.fetch.SetBackup();
				EditorLogic.fetch.SetBackup();
				EditorLogic.fetch.GetType().GetMethod("RestoreState", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(EditorLogic.fetch, new object[] { -1 });
				destroyHighlightObjects();
			}
			           
			GUI.DragWindow();
		}

		public override void onGUI() {
			base.onGUI();


			foreach (Part p in highlightParts) {
				Bounds partBounds;
				if (p.GetRendererBounds().Length > 0) {
					partBounds = p.GetRendererBounds()[0];
					for (int i = 0; i < p.GetRendererBounds().Length; ++i) {
						partBounds.Encapsulate(p.GetRendererBounds()[i]);

						//List<Vector3> points = createPointsForBounds(p.GetRendererBounds()[i]);
						//foreach (Vector3 p3 in points) {
						//	Vector2 boundsScreenPosition = Camera.main.WorldToScreenPoint(p3);
						//	GUI.Box(new Rect(boundsScreenPosition.x, Screen.height - boundsScreenPosition.y, 10, 10), "");
						//}

					}
				} else {
					partBounds = p.GetPartRendererBound();
				}
				List<Vector3> partBounds3dList = createPointsForBounds(partBounds);


				//List<Vector2> partBounds2dList = new List<Vector2>();
				//Vector2 centerOnScreen = Vector2.zero;
				//foreach (Vector3 p3 in partBounds3dList) {
				//	Vector2 boundsScreenPosition = Camera.main.WorldToScreenPoint(p3);
				//	partBounds2dList.Add(boundsScreenPosition);
				//	centerOnScreen += boundsScreenPosition;


				//	//GUI.Box(new Rect(boundsScreenPosition.x, Screen.height - boundsScreenPosition.y, 10, 10), "");
				//}
				//centerOnScreen /= partBounds3dList.Count;
				//Vector2 radius = Vector2.zero;
				//foreach (Vector2 p2 in partBounds2dList) {
				//	Vector2 r = (centerOnScreen - p2);
				//	r.x = Mathf.Abs(r.x);
				//	r.y = Mathf.Abs(r.y);
				//	if (r.x > radius.x) {
				//		radius.x = r.x;
				//	}
				//	if (r.y > radius.y) {
				//		radius.y = r.y;
				//	}
				//}

				Vector2 screenMin = Camera.main.WorldToScreenPoint(partBounds3dList[0]);
				Vector2 screenMax = Vector2.zero;
				foreach (Vector3 p3 in partBounds3dList) {
					Vector2 boundsScreenPosition = Camera.main.WorldToScreenPoint(p3);
					screenMin.x = Mathf.Min(screenMin.x, boundsScreenPosition.x);
					screenMin.y = Mathf.Min(screenMin.y, boundsScreenPosition.y);


					screenMax.x = Mathf.Max(screenMax.x, boundsScreenPosition.x);
					screenMax.y = Mathf.Max(screenMax.y, boundsScreenPosition.y);

					//GUI.Box(new Rect(boundsScreenPosition.x, Screen.height - boundsScreenPosition.y, 10, 10), "");
				}
				Vector2 centerOnScreen = (screenMin + screenMax)/2;
				Vector2 radius = (screenMax - screenMin) / 2;

				radius.x = Mathf.Max(radius.x, 10);
				radius.y = Mathf.Max(radius.y, 10);
				if (selectionGuiStyle == null) {
					selectionGuiStyle = new GUIStyle(GUI.skin.box);
					selectionGuiStyle.normal.background = UiUtils.createSingleColorTexture(new Color(1, 0, 0));
					selectionGuiStyle.normal.textColor = new Color(0, 1, 0);
					selectionGuiStyle.stretchWidth = true;
					//selectionGuiStyle.onNormal.background = UiUtils.createSingleColorTexture(new Color(1, 0, 0));
					//selectionGuiStyle.active.background = UiUtils.createSingleColorTexture(new Color(1, 0, 0));
					//selectionGuiStyle.onActive.background = UiUtils.createSingleColorTexture(new Color(1, 0, 0));

					selectionGuiStyle.border = new RectOffset(0, 0, 0, 0);

				}
				float width = 1;
				GUI.Box(new Rect(centerOnScreen.x - radius.x, Screen.height - centerOnScreen.y - radius.y, width, radius.y * 2), "", selectionGuiStyle);//left
				GUI.Box(new Rect(centerOnScreen.x - radius.x, Screen.height - centerOnScreen.y - radius.y, radius.x * 2, width), "", selectionGuiStyle);//top
				GUI.Box(new Rect(centerOnScreen.x - radius.x, Screen.height - centerOnScreen.y + radius.y, radius.x * 2, width), "", selectionGuiStyle);//bottom
				GUI.Box(new Rect(centerOnScreen.x + radius.x - width, Screen.height - centerOnScreen.y - radius.y, width, radius.y * 2), "", selectionGuiStyle);//right

				//GUI.Box(new Rect(centerOnScreen.x - radius.x, Screen.height - centerOnScreen.y - radius.y, radius.x * 2, radius.y * 2), "", selectionGuiStyle);
				//GUI.Box(new Rect(centerOnScreen.x - radius.x, Screen.height - centerOnScreen.y - radius.y, radius.x * 2, radius.y * 2), "", selectionGuiStyle);
				//GUI.Box(new Rect(centerOnScreen.x - radius.x, Screen.height - centerOnScreen.y - radius.y, radius.x * 2, radius.y * 2), "", selectionGuiStyle);


				//PluginLogger.logDebug(partBounds + "," + p.transform.position);
				//Vector2 boundsScreenPosition = Camera.main.WorldToScreenPoint(partBounds.center);
				//GUI.Box(new Rect(boundsScreenPosition.x, Screen.height -  boundsScreenPosition.y, 10, 10), "");
			}
		}

		List<Vector3> createPointsForBounds(Bounds bounds) {
			return new List<Vector3>(new Vector3[] {
					bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, -bounds.extents.z),
					bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, bounds.extents.z),
					bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, -bounds.extents.z),
					bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, bounds.extents.z),
					bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, -bounds.extents.z),
					bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, bounds.extents.z),
					bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, -bounds.extents.z),
					bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z),
				});
		}

		GUIStyle selectionGuiStyle;

		private void highlight(EntryInEasyStage entry) {
			foreach (Part p in EditorLogic.SortedShipList) {
				p.SetHighlight(false, false);
			}
			destroyHighlightObjects();
			foreach (Part p in entry.parts) {
				p.highlight(new Color(0, 1, 0));
				highlightObjects.Add(createLineObject(p));
				Vector2 partScreenPosition = Camera.main.WorldToScreenPoint(p.transform.position);
				float percent = 0.2f;

				PluginLogger.logDebug(Screen.height + ", " + partScreenPosition);
				if (partScreenPosition.y < Screen.height * percent || partScreenPosition.y > Screen.height * (1 - 2 * percent)) {					
					EditorDriver.fetch.vabCamera.scrollHeight = p.transform.position.y;
				}


				highlightParts.Add(p);
			}
		}

		List<GameObject> highlightObjects = new List<GameObject>();
		List<Part> highlightParts = new List<Part>();

		public override void hideWindow() {
			base.hideWindow();
			destroyHighlightObjects();
		}
		private void destroyHighlightObjects() {
			foreach (GameObject g in highlightObjects) {
				g.DestroyGameObject();
			}
			highlightObjects.Clear();
			highlightParts.Clear();
		}

		public GameObject createLineObject(Part p) {
			Bounds bounds = calcShipBounds();
			Vector3 pos = p.transform.position;
			Vector2d shipCenter2d = new Vector2d(bounds.center.x, bounds.center.z);
			Vector2d arrowEnd = new Vector2d(pos.x, pos.z);
			Vector2d arrowDir = (shipCenter2d - arrowEnd).normalized;
			if (arrowDir.sqrMagnitude < Double.Epsilon) {
				arrowDir = (shipCenter2d - new Vector2d(1, 1)).normalized;
			}
			Vector2d arrowStart = arrowEnd - arrowDir * 5;

			GameObject lineObj = new GameObject("Line");
			LineRenderer line = lineObj.AddComponent<LineRenderer>();
			//line.transform.parent = transform; // ...child to our part...
			line.useWorldSpace = true; // ...and moving along with it (rather 
										// than staying in fixed world coordinates)
			line.transform.localPosition = Vector3.zero;
			line.transform.localEulerAngles = Vector3.zero;

			// Make it render a red to yellow triangle, 1 meter wide and 2 meters long
			line.material = new Material(Shader.Find("Particles/Additive"));
			line.SetColors(Color.red, Color.red);
			line.SetWidth(1, 0);

			line.SetVertexCount(2);
			line.SetPosition(0, new Vector3d(arrowStart.x, pos.y, arrowStart.y));
			line.SetPosition(1, pos);
			return lineObj;
		}

		private Bounds calcShipBounds() {
			Bounds result = new Bounds(EditorLogic.fetch.ship.Parts[0].transform.position, Vector3.zero);
			foreach (var current in EditorLogic.fetch.ship.Parts) {
				if (current.collider && !current.Modules.Contains("LaunchClamp")) {
					result.Encapsulate(current.collider.bounds);
				}
			}
			return result;
		}

		override protected bool isPopupWindow() {
			return false;
		}

	}
}

