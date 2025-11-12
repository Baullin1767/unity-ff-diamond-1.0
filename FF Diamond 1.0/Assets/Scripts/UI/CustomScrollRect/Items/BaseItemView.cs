using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.CustomScrollRect
{
    public abstract class BaseItemView: MonoBehaviour
    {
        [SerializeField] protected TMP_Text title;
        public abstract void Bind<T>(T data);
    }
}