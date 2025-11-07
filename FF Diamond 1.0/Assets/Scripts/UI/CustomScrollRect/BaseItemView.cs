using System.Collections.Generic;
using UnityEngine;

namespace UI.CustomScrollRect
{
    public abstract class BaseItemView: MonoBehaviour
    {
        public abstract void Bind<T>(int dataIndex, T data);
    }
}