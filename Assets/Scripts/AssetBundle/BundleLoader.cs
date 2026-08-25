using System;
using System.IO;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class BundleLoader : MonoBehaviour
{
    public enum TYPE_ASSET { NONE, IMAGE, SKELETON_GRAPHIC, TEXT };
    [SerializeField, HideInInspector] public TYPE_ASSET Type = TYPE_ASSET.IMAGE;
    [HideInInspector] public TextMeshProUGUI ThisTMPUGUI;
    [NonSerialized] public Image ThisImg;
    [NonSerialized] public SkeletonGraphic ThisSG;
    [SerializeField, HideInInspector] public string BundleLabel, AssetName, AnimName, FontName, FontMaterialName;
    [SerializeField, HideInInspector] public bool IsSetNativeSize, IsTextUpper;

    public void RefreshUI()
    {
        switch (Type)
        {
            case TYPE_ASSET.TEXT:
                {
                    if (ThisTMPUGUI == null) ThisTMPUGUI = GetComponent<TextMeshProUGUI>();
                    if (ThisTMPUGUI == null || !ThisTMPUGUI.enabled) return;
                    // if (!AssetName.Equals(""))
                    //     ThisTMPUGUI.text = IsTextUpper ? Globals.Config.getTextConfig(AssetName).ToUpper() : Globals.Config.getTextConfig(AssetName);
                    if (BundleLabel.Equals("")) return;
                    TMP_FontAsset aFA = BundleHandler.LoadFontAsset(FontName);
                    if (aFA == null) return;
                    ThisTMPUGUI.font = aFA;
                    ThisTMPUGUI.fontSharedMaterial = !FontMaterialName.Equals("") ? BundleHandler.LoadMaterial(FontMaterialName) : aFA.material;
                    ThisTMPUGUI.fontSharedMaterial.shader = BundleHandler.GetFontShader(aFA.material.shader.name);
                    break;
                }
            case TYPE_ASSET.IMAGE:
                {
                    if (ThisImg == null) ThisImg = GetComponent<Image>();
                    if (ThisImg == null || !ThisImg.enabled) return;
                    ThisImg.sprite = BundleHandler.LoadSprite(AssetName);
                    if (IsSetNativeSize) ThisImg.SetNativeSize();
                    break;
                }
            case TYPE_ASSET.SKELETON_GRAPHIC:
                {
                    if (ThisSG == null) ThisSG = GetComponent<SkeletonGraphic>();
                    if (ThisSG == null || !ThisSG.enabled) return;
                    BundleHandler.SetDataForASkeletonGraphic(ThisSG, AssetName, AnimName, ThisSG.startingLoop);
                    break;
                }
        }
    }
    public void PrepareData()
    {
        switch (Type)
        {
            case TYPE_ASSET.SKELETON_GRAPHIC:
                {
                    if (ThisSG == null) ThisSG = GetComponent<SkeletonGraphic>();
                    if (ThisSG == null || !ThisSG.enabled) return;
                    BundleHandler.SetDataForASkeletonGraphic(ThisSG, AssetName, "", ThisSG.startingLoop);
                    break;
                }
        }
    }

    private void OnDisable() => BundleHandler.MAIN.RemoveLoader(this);
    private void Start()
    {
        if (Type == TYPE_ASSET.SKELETON_GRAPHIC)
        {
            if (ThisSG.skeletonDataAsset == null) RefreshUI();
            if (ThisSG.skeletonDataAsset == null) return;
            ThisSG.allowMultipleCanvasRenderers = false;
            if (ThisSG.skeletonDataAsset.atlasAssets.Length > 1
                || ThisSG.skeletonDataAsset.atlasAssets[0].MaterialCount > 1
                || ThisSG.skeletonDataAsset.blendModeMaterials.additiveMaterials.Count > 0
                || ThisSG.skeletonDataAsset.blendModeMaterials.multiplyMaterials.Count > 0
                || ThisSG.skeletonDataAsset.blendModeMaterials.screenMaterials.Count > 0
                || ThisSG.canvasRenderers.Count > 0)
            {   // if these options were turned on before then now keep using them
                ThisSG.allowMultipleCanvasRenderers = true;
                ThisSG.canvasRenderer.Clear();
                ThisSG.TrimRenderers();
                ThisSG.UpdateMesh();
            }
        }
        else RefreshUI();
    }
    private void Awake()
    {
        ThisTMPUGUI = GetComponent<TextMeshProUGUI>();
        ThisSG = GetComponent<SkeletonGraphic>();
        ThisImg = GetComponent<Image>();
        BundleHandler.MAIN.AddLoader(this);
    }
}
//-------------------------------------------------- |   ) )=3 --------------------------------------------------
#if UNITY_EDITOR 
[CustomEditor(typeof(BundleLoader))]
public class LoaderEditor : Editor
{
    private SerializedProperty _AssetName, _BundleLabel, _AnimName, _IsSetNativeSize, _FontName, _FontMaterialName, _Type, _IsTextUpper;
    private string[] _AnimNames;
    private SkeletonData _LastSD;

    public override void OnInspectorGUI()
    {
        BundleLoader thisBL = (BundleLoader)target;
        serializedObject.Update();
        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField("Asset Name", _AssetName.stringValue);
            if ((BundleLoader.TYPE_ASSET)_Type.enumValueIndex == BundleLoader.TYPE_ASSET.SKELETON_GRAPHIC)
                EditorGUILayout.LabelField("Anim Name", _AnimName.stringValue);
            else if ((BundleLoader.TYPE_ASSET)_Type.enumValueIndex == BundleLoader.TYPE_ASSET.TEXT)
            {
                EditorGUILayout.LabelField("Upper", _IsTextUpper.boolValue + "");
                EditorGUILayout.LabelField("Font", _FontName.stringValue);
                EditorGUILayout.LabelField("Material", _FontMaterialName.stringValue);
            }
            serializedObject.ApplyModifiedProperties();
            return; // test in editor play mode will cause error, only work with this in editor idle mode
        }
        base.OnInspectorGUI();
        _Type.enumValueIndex = (int)BundleLoader.TYPE_ASSET.NONE;
        thisBL.ThisImg = thisBL.GetComponent<Image>();
        if (thisBL.ThisImg != null) _Type.enumValueIndex = (int)BundleLoader.TYPE_ASSET.IMAGE;
        else
        {
            thisBL.ThisSG = thisBL.GetComponent<SkeletonGraphic>();
            if (thisBL.ThisSG != null) _Type.enumValueIndex = (int)BundleLoader.TYPE_ASSET.SKELETON_GRAPHIC;
            else
            {
                thisBL.ThisTMPUGUI = thisBL.GetComponent<TextMeshProUGUI>();
                if (thisBL.ThisTMPUGUI != null) _Type.enumValueIndex = (int)BundleLoader.TYPE_ASSET.TEXT;
            }
        }
        EditorGUILayout.LabelField("Type: " + ((BundleLoader.TYPE_ASSET)_Type.enumValueIndex), EditorStyles.boldLabel);
        switch ((BundleLoader.TYPE_ASSET)_Type.enumValueIndex)
        {
            case BundleLoader.TYPE_ASSET.NONE:
                {
                    EditorGUILayout.HelpBox("YOU MUST ADD A COMPONENT FIRST!", MessageType.Error);
                    break;
                }
            case BundleLoader.TYPE_ASSET.TEXT:
                {
                    if (!thisBL.ThisTMPUGUI.enabled)
                    {
                        EditorGUILayout.HelpBox("You must have an active TextMeshPro!", MessageType.Error);
                        return;
                    }
                    if (thisBL.ThisTMPUGUI.font == null)
                    {
                        EditorGUILayout.HelpBox("No Font asset found!", MessageType.Error);
                        EditorGUILayout.LabelField("Label", _BundleLabel.stringValue);
                        EditorGUILayout.LabelField("Key", _AssetName.stringValue);
                        EditorGUILayout.LabelField("Upper", _IsTextUpper.boolValue + "");
                        EditorGUILayout.LabelField("Font", _FontName.stringValue);
                        EditorGUILayout.LabelField("Material", _FontMaterialName.stringValue);
                        if (_BundleLabel.stringValue.Equals("")) EditorGUILayout.HelpBox("This Font is not in any bundle!", MessageType.Error);
                        return;
                    }
                    _FontName.stringValue = AssetDatabase.GetAssetPath(thisBL.ThisTMPUGUI.font);
                    string path = _FontName.stringValue;
                    do
                    {
                        _BundleLabel.stringValue = AssetImporter.GetAtPath(path).assetBundleName;
                        if (_BundleLabel.stringValue.Equals("")) path = Path.GetDirectoryName(path);
                        else path = "";
                    } while (!path.Equals(""));
                    bool hasNoLabel = _BundleLabel.stringValue.Equals("");
                    string materialBundleLabel = "";
                    _FontMaterialName.stringValue = "";
                    if (!hasNoLabel)
                    {
                        if (thisBL.ThisTMPUGUI.fontSharedMaterial == thisBL.ThisTMPUGUI.font.material) materialBundleLabel = _BundleLabel.stringValue;
                        else
                        {
                            _FontMaterialName.stringValue = AssetDatabase.GetAssetPath(thisBL.ThisTMPUGUI.fontSharedMaterial);
                            path = _FontMaterialName.stringValue;
                            do
                            {
                                materialBundleLabel = AssetImporter.GetAtPath(path).assetBundleName;
                                if (materialBundleLabel.Equals("")) path = Path.GetDirectoryName(path);
                                else path = "";
                            } while (!path.Equals(""));
                        }
                    }
                    _AssetName.stringValue = EditorGUILayout.TextField("Key", _AssetName.stringValue);
                    _IsTextUpper.boolValue = EditorGUILayout.Toggle("Upper", _IsTextUpper.boolValue);
                    EditorGUILayout.LabelField("Font", _FontName.stringValue);
                    EditorGUILayout.LabelField("Material", _FontMaterialName.stringValue);
                    EditorGUILayout.LabelField("Label", _BundleLabel.stringValue);
                    if (hasNoLabel) EditorGUILayout.HelpBox("This Font is not in any bundle!", MessageType.Error);
                    else
                    {
                        if (!materialBundleLabel.Equals(_BundleLabel.stringValue))
                            EditorGUILayout.HelpBox("This Material is not in the same bundle of the Font!", MessageType.Error);
                    }
                    break;
                }
            case BundleLoader.TYPE_ASSET.IMAGE:
                {
                    if (!thisBL.ThisImg.enabled)
                    {
                        EditorGUILayout.HelpBox("You must have an active Image!", MessageType.Error);
                        return;
                    }
                    if (thisBL.ThisImg.sprite == null)
                    {
                        EditorGUILayout.HelpBox("No Image asset found!", MessageType.Error);
                        EditorGUILayout.LabelField("Label", _BundleLabel.stringValue);
                        if (_BundleLabel.stringValue.Equals("")) EditorGUILayout.HelpBox("This asset is not in any bundle!", MessageType.Error);
                        EditorGUILayout.LabelField("Asset Name", _AssetName.stringValue);
                        EditorGUILayout.LabelField("Set Native Size", _IsSetNativeSize.boolValue ? "True" : "False");
                        return;
                    }
                    _AssetName.stringValue = AssetDatabase.GetAssetPath(thisBL.ThisImg.sprite);
                    string path = _AssetName.stringValue;
                    do
                    {
                        _BundleLabel.stringValue = AssetImporter.GetAtPath(path).assetBundleName;
                        if (_BundleLabel.stringValue.Equals("")) path = Path.GetDirectoryName(path);
                        else path = "";
                    } while (!path.Equals(""));
                    EditorGUILayout.LabelField("Label", _BundleLabel.stringValue);
                    if (_BundleLabel.stringValue.Equals("")) EditorGUILayout.HelpBox("This asset is not in any bundle!", MessageType.Error);
                    EditorGUILayout.LabelField("Asset Name", _AssetName.stringValue);
                    _IsSetNativeSize.boolValue = EditorGUILayout.Toggle("Set Native Size", _IsSetNativeSize.boolValue);
                    break;
                }
            case BundleLoader.TYPE_ASSET.SKELETON_GRAPHIC:
                {
                    if (!thisBL.ThisSG.enabled)
                    {
                        EditorGUILayout.HelpBox("You must have an active SkeletonGraphic!", MessageType.Error);
                        return;
                    }
                    if (thisBL.ThisSG.SkeletonDataAsset == null)
                    {
                        EditorGUILayout.HelpBox("No SkeletonData asset found!", MessageType.Error);
                        EditorGUILayout.LabelField("Label", _BundleLabel.stringValue);
                        if (_BundleLabel.stringValue.Equals("")) EditorGUILayout.HelpBox("This asset is not in any bundle!", MessageType.Error);
                        EditorGUILayout.LabelField("Asset Name", _AssetName.stringValue);
                        EditorGUILayout.LabelField("Anim Name", _AnimName.stringValue);
                        return;
                    }
                    SkeletonData thisSD = thisBL.ThisSG.SkeletonData;
                    string assetName = AssetDatabase.GetAssetPath(thisBL.ThisSG.skeletonDataAsset);
                    if (string.IsNullOrEmpty(assetName)) return;
                    _AssetName.stringValue = assetName;
                    string path = Path.GetDirectoryName(_AssetName.stringValue);
                    do
                    {
                        _BundleLabel.stringValue = AssetImporter.GetAtPath(path).assetBundleName;
                        if (_BundleLabel.stringValue.Equals("")) path = Path.GetDirectoryName(path);
                        else path = "";
                    }
                    while (!path.Equals(""));
                    EditorGUILayout.LabelField("Label", _BundleLabel.stringValue);
                    if (_BundleLabel.stringValue.Equals("")) EditorGUILayout.HelpBox("This asset is not in any bundle!", MessageType.Error);
                    EditorGUILayout.LabelField("Asset Name", _AssetName.stringValue);
                    if (_LastSD != thisSD)
                    {
                        _LastSD = thisSD;
                        _AnimNames = new string[thisSD.Animations.Count];
                        int id = 0;
                        ExposedList<Spine.Animation> thisAs = thisSD.Animations;
                        foreach (Spine.Animation anim in thisAs) _AnimNames[id++] = anim.Name;
                        _AnimName.stringValue = thisBL.ThisSG.startingAnimation;
                    }
                    _AnimName.stringValue = _AnimNames[EditorGUILayout.Popup("Animation", Mathf.Max(0, Array.IndexOf(_AnimNames, _AnimName.stringValue)), _AnimNames)];
                    break;
                }
        }
        serializedObject.ApplyModifiedProperties();
    }
    private void OnEnable()
    {
        _AssetName = serializedObject.FindProperty("AssetName");
        _BundleLabel = serializedObject.FindProperty("BundleLabel");
        _AnimName = serializedObject.FindProperty("AnimName");
        _FontName = serializedObject.FindProperty("FontName");
        _FontMaterialName = serializedObject.FindProperty("FontMaterialName");
        _IsSetNativeSize = serializedObject.FindProperty("IsSetNativeSize");
        _IsTextUpper = serializedObject.FindProperty("IsTextUpper");
        _Type = serializedObject.FindProperty("Type");
    }
}
#endif
