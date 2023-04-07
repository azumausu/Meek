using System;
using Harmos.UI.UGUI;
using Meek.NavigationStack;
using UnityEngine;

namespace Sample
{
    public class NavigatorTest : MonoBehaviour
    {
        [SerializeField] private Main _main;
        
        public void Push(string screenName)
        {
            var stackNavigatorService = _main.App.GetService<StackNavigationService>();
            stackNavigatorService.PushAsync(GetScreenType(screenName)).Forget();
        }
        
        public void Pop()
        {
            var stackNavigatorService = _main.App.GetService<StackNavigationService>();
            stackNavigatorService.PopAsync().Forget();
        }
        
        public void InsertScreenBefore(string beforeScreenAndInsertionScreenName)
        {
            var stackNavigatorService = _main.App.GetService<StackNavigationService>();
            var screenName = beforeScreenAndInsertionScreenName.Split(',');
            stackNavigatorService.InsertScreenBeforeAsync(GetScreenType(screenName[0]), GetScreenType(screenName[1])).Forget();
        }
        
        public void Remove(string screenName)
        {
            var stackNavigatorService = _main.App.GetService<StackNavigationService>();
            stackNavigatorService.RemoveAsync(GetScreenType(screenName)).Forget();
        }
        
        private Type GetScreenType(string screenName)
        {
            return screenName switch
            {
                "BlueScreen" => typeof(BlueScreen),
                "RedDialogScreen" => typeof(RedDialogScreen),
                "YellowScreen" => typeof(YellowScreen),
            };
        }
    }
}