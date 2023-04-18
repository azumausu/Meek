using System;
using System.Collections;
using UnityEngine;

namespace Meek.NavigationStack
{
    public interface ICoroutineRunner : IDisposable
    {
        Coroutine StartCoroutine(IEnumerator routine);
    }
}