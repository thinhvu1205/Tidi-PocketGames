using UnityEngine;

public class ItemOnOff : MonoBehaviour
{
    [SerializeField] private Transform m_BgOnTf;

    public bool IsTurnOn() => m_BgOnTf.gameObject.activeSelf;
    public void TurnOn() => m_BgOnTf.gameObject.SetActive(true);
    public void TurnOff() => m_BgOnTf.gameObject.SetActive(false);
}
