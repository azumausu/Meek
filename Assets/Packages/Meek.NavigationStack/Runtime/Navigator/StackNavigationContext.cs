using System;
using JetBrains.Annotations;

namespace Meek.NavigationStack
{
    public class StackNavigationContext : NavigationContext
    {
        private const string InsertionBeforeScreen = nameof(InsertionBeforeScreen);
        private const string InsertionScreen = nameof(InsertionScreen);
        private const string RemoveScreen = nameof(RemoveScreen);
        private const string RemoveBeforeScreen = nameof(RemoveBeforeScreen);
        private const string RemoveAfterScreen = nameof(RemoveAfterScreen);

        public const string NextScreenParameter = nameof(NextScreenParameter);

        public StackNavigationSourceType NavigatingSourceType;

        public bool IsCrossFade;

        public bool SkipAnimation;

        public StackScreen GetInsertionScreen()
        {
            if (NavigatingSourceType != StackNavigationSourceType.Insert)
            {
                throw new InvalidOperationException();
            }

            return this.GetFeatureValue<StackScreen>(InsertionScreen);
        }

        public StackScreen GetInsertionBeforeScreen()
        {
            if (NavigatingSourceType != StackNavigationSourceType.Insert)
            {
                throw new InvalidOperationException();
            }

            return this.GetFeatureValue<StackScreen>(InsertionBeforeScreen);
        }

        public StackScreen GetRemoveScreen()
        {
            if (NavigatingSourceType != StackNavigationSourceType.Remove)
            {
                throw new InvalidOperationException();
            }

            return this.GetFeatureValue<StackScreen>(RemoveScreen);
        }

        [CanBeNull]
        public StackScreen GetRemoveBeforeScreen()
        {
            if (NavigatingSourceType != StackNavigationSourceType.Remove)
            {
                throw new InvalidOperationException();
            }

            return this.GetFeatureNullableValue<StackScreen>(RemoveBeforeScreen);
        }

        [CanBeNull]
        public StackScreen GetRemoveAfterScreen()
        {
            if (NavigatingSourceType != StackNavigationSourceType.Remove)
            {
                throw new InvalidOperationException();
            }

            return this.GetFeatureNullableValue<StackScreen>(RemoveAfterScreen);
        }

        public TParam GetNextScreenParameter<TParam>()
        {
            return this.GetFeatureValue<TParam>(NextScreenParameter);
        }

        public void SetInsertionBeforeScreen(StackScreen insertionBeforeScreen)
        {
            Features.Add(InsertionBeforeScreen, insertionBeforeScreen);
        }

        public void SetInsertionScreen(StackScreen insertionScreen)
        {
            Features.Add(InsertionScreen, insertionScreen);
        }

        public void SetRemoveScreen(StackScreen removeScreen)
        {
            Features.Add(RemoveScreen, removeScreen);
        }

        public void SetRemoveBeforeScreen(StackScreen removeBeforeScreen)
        {
            Features.Add(RemoveBeforeScreen, removeBeforeScreen);
        }

        public void SetRemoveAfterScreen(StackScreen removeAfterScreen)
        {
            Features.Add(RemoveAfterScreen, removeAfterScreen);
        }

        public void SetNextScreenParameter(object parameter)
        {
            Features.Add(NextScreenParameter, parameter);
        }
    }
}