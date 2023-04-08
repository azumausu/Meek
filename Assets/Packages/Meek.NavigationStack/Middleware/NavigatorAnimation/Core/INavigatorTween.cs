using System;

namespace Meek.NavigationStack
{
    public interface INavigatorTween
    {
        #region Methods
        
        NavigatorAnimationType NavigatorAnimationType { get; }
        
        float Length { get; }
        
        string FromScreenName { get; }
        
        string ToScreenName { get; }
        
        void Evaluate(float t);

        void Play(Action onComplete = null);

        void Stop();

        #endregion
    }
}