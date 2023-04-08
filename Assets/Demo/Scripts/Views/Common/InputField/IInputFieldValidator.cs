using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Demo
{
    public interface IInputFieldValidator
    {
        bool IsValid(string text);
    }
}