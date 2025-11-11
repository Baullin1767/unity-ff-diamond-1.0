using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.CustomScrollRect
{
    public abstract class BaseItemView: MonoBehaviour
    {
        [SerializeField] protected TMP_Text title;
        [SerializeField] protected Button button;
        public abstract void Bind<T>(int dataIndex, T data);
    }
}