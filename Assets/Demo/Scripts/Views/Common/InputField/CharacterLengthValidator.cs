using UnityEngine;

namespace Demo
{
    public class CharacterLengthValidator : MonoBehaviour, IInputFieldValidator
    {
        [SerializeField] private int _characterLenght;
        
        public bool IsValid(string text)
        {
            return text.Length >= _characterLenght;
        }
    }
}