using UnityEngine;

public class GameListener : MonoBehaviour
{
    public bool IsReady() { return gameObject != null && isActiveAndEnabled; }
    public virtual void HandleData(string _apiName, string _data) { }
    public virtual void HandleError(string _apiName, string _data) { }

    protected virtual void OnDestroy() => NetworkManager.INSTANCE.RemoveListener(this);
    protected virtual void Awake() => NetworkManager.INSTANCE.AddListener(this);
}
