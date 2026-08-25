using System.Collections.Generic;
using Spine.Unity;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Video;

public class BundleHandler
{
    public enum PLATFORM { Android, iOS, WebGL }
    public static BundleHandler MAIN
    {
        get
        {
            if (_INSTANCE == null) _INSTANCE = new();
            return _INSTANCE;
        }
    }
    private static string[] _PREFAB_TAILS = { ".prefab" }, _TEXT_TAILS = { ".txt" }, _IMAGE_TAILS = { ".png", ".jpg", ".jpeg" }, _FONT_TAILS = { ".asset" },
        _AUDIO_TAILS = { ".mp3", ".ogg" }, _VIDEO_TAILS = { ".mp4" }, _MATERIAL_TAILS = { ".mat" }, _SKELETON_TAILS = { ".asset" };
    private static BundleHandler _INSTANCE;
    public const string BASE_PATH = "Assets/AssetBundles/", CATEGORY = "category.txt", SPLIT = "_hash_", RESOURCES = "Assets/Resources/";
    public string BundleUrl;
    private Dictionary<string, BundleVersion> _AssetsMapD = new();
    private Dictionary<Object, BundleLoader> _LoadersBLs = new();
    private Dictionary<string, Shader> _FontShadersD = new();

    public void AddLoader(Object _loader) => _LoadersBLs.Add(_loader, _loader.GetComponent<BundleLoader>());
    public void RemoveLoader(Object _loader) => _LoadersBLs.Remove(_loader);
    public void ClearAssetsDictionary() => _AssetsMapD.Clear();
    public void AddToLocalMap(BundleVersion _aBV)
    {
        foreach (string assetName in _aBV.AssetNamesHS)
        {
            if (_AssetsMapD.ContainsKey(assetName)) _AssetsMapD[assetName] = _aBV;
            else _AssetsMapD.Add(assetName, _aBV);
        }
    }
    //-------------------------------------------------- |   ) )=3 --------------------------------------------------
    //                                      path must starts from Assets/Resources
    private static T _LoadAsset<T>(string _path, string[] _tails) where T : Object
    {
        if (!_path.StartsWith(RESOURCES)) _path = RESOURCES + _path;
        foreach (string tail in _tails)
        {
            if (_path.EndsWith(tail)) _path = _path.Replace(tail, "");
            string fullPath = (_path + tail).ToLower();
            if (MAIN._AssetsMapD.ContainsKey(fullPath)) return MAIN._AssetsMapD[fullPath].BundleAB.LoadAsset<T>(fullPath);
        }
        return Resources.Load<T>(_path.Replace(RESOURCES, ""));
    }
    private static ResourceRequest _LoadAssetAsync<T>(string _path, string[] _tails) where T : Object
    {
        if (!_path.StartsWith(RESOURCES)) _path = RESOURCES + _path;
        foreach (string tail in _tails)
        {
            if (_path.EndsWith(tail)) _path = _path.Replace(tail, "");
            string fullPath = (_path + tail).ToLower();
            if (MAIN._AssetsMapD.ContainsKey(fullPath))
                return MAIN._AssetsMapD[fullPath].BundleAB.LoadAssetAsync<T>(fullPath);
        }
        return Resources.LoadAsync<T>(_path.Replace(RESOURCES, ""));
    }
    private static T[] _LoadAssetWithSubAssets<T>(string _path, string[] _tails) where T : Object
    {
        if (!_path.StartsWith(RESOURCES)) _path = RESOURCES + _path;
        foreach (string tail in _tails)
        {
            if (_path.EndsWith(tail)) _path = _path.Replace(tail, "");
            string fullPath = (_path + tail).ToLower();
            if (MAIN._AssetsMapD.ContainsKey(fullPath)) return MAIN._AssetsMapD[fullPath].BundleAB.LoadAssetWithSubAssets<T>(fullPath);
        }
        return Resources.LoadAll<T>(_path.Replace(RESOURCES, ""));
    }
    private static void _PrepareLoadersIfNeeded(GameObject _parent)
    {
        BundleLoader[] itemBLs = _parent.GetComponentsInChildren<BundleLoader>(true);
        foreach (BundleLoader itemBL in itemBLs) itemBL.PrepareData();
    }
    #region Load Assets
    public static T Instantiate<T>(T _prefab, Transform _parentTf = null) where T : Object
    {
        T output = GameObject.Instantiate(_prefab, _parentTf);
        _PrepareLoadersIfNeeded(output.GameObject());
        return output;
    }
    // GameObject
    public static GameObject LoadGameObject(string _path)
    {
        GameObject output = _LoadAsset<GameObject>(_path, _PREFAB_TAILS);
        if (output != null) _PrepareLoadersIfNeeded(output);
        return output;
    }
    // TextAsset
    public static TextAsset LoadTextAsset(string _path) => _LoadAsset<TextAsset>(_path, _TEXT_TAILS);
    // Font
    public static TMP_FontAsset LoadFontAsset(string _path) => _LoadAsset<TMP_FontAsset>(_path, _FONT_TAILS);
    public static Shader GetFontShader(string _shaderName) => MAIN._FontShadersD.ContainsKey(_shaderName) ? MAIN._FontShadersD[_shaderName] : Shader.Find(_shaderName);
    // Sprite
    public static Sprite LoadSprite(string _path) => _LoadAsset<Sprite>(_path, _IMAGE_TAILS);
    // Texture
    public static Texture LoadTexture(string _path) => _LoadAsset<Texture>(_path, _IMAGE_TAILS);
    // Texture2D
    public static Texture2D LoadTexture2D(string _path) => _LoadAsset<Texture2D>(_path, _IMAGE_TAILS);
    // AudioClip
    public static AudioClip LoadAudioClip(string _path) => _LoadAsset<AudioClip>(_path, _AUDIO_TAILS);
    // VideoClip    
    public static VideoClip LoadVideoClip(string _path) => _LoadAsset<VideoClip>(_path, _VIDEO_TAILS);
    public static async Awaitable<VideoClip> LoadVideoClipAsync(string path)
    {
        ResourceRequest rr = _LoadAssetAsync<VideoClip>(path, _VIDEO_TAILS);
        await rr;
        return rr.asset as VideoClip;
    }
    // Material
    public static Material LoadMaterial(string _path) => _LoadAsset<Material>(_path, _MATERIAL_TAILS);
    // SkeletonDataAsset
    public static SkeletonDataAsset LoadSkeletonDataAsset(string _path) => _LoadAsset<SkeletonDataAsset>(_path, _SKELETON_TAILS);
    public static async Awaitable<SkeletonDataAsset> LoadSkeletonDataAssetAsync(string path)
    {
        ResourceRequest rr = _LoadAssetAsync<SkeletonDataAsset>(path, _SKELETON_TAILS);
        await rr;
        return rr.asset as SkeletonDataAsset;
    }
    // Sprite[]
    public static Sprite[] LoadMultipleSprites(string _path) => _LoadAssetWithSubAssets<Sprite>(_path, _IMAGE_TAILS);
    #endregion
    public static bool SetDataForASkeletonGraphic(SkeletonGraphic _targetSG, string _path, string _animName = "", bool _loop = true)
    {
        if (_targetSG == null) return false;
        SkeletonDataAsset skeDataSDA = LoadSkeletonDataAsset(_path);
        if (skeDataSDA == null) return false;
        _targetSG.skeletonDataAsset = skeDataSDA;
        _targetSG.Initialize(true);
        if (!_animName.Equals("")) _targetSG.AnimationState.SetAnimation(0, _animName, _loop);
        return true;
    }
}

public class BundleVersion
{
    public enum STATE { Cloud, Downloading, Downloaded }
    public STATE State = STATE.Cloud;
    public HashSet<string> AssetNamesHS = new();
    public AssetBundle BundleAB;
    public Hash128 HashH128;
    public string Name, Url;
}
