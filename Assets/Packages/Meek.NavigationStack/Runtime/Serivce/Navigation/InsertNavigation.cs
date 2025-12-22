using System;
using System.Threading.Tasks;

#nullable enable

namespace Meek.NavigationStack
{
    public class InsertNavigation
    {
        protected readonly NavigationSharedSemaphore SharedSemaphore;
        protected readonly StackNavigationService StackNavigationService;
        protected readonly InsertContext Context = new();
        protected object? Sender;

        public InsertNavigation(StackNavigationService stackNavigationService, NavigationSharedSemaphore sharedSemaphore)
        {
            StackNavigationService = stackNavigationService;
            SharedSemaphore = sharedSemaphore;
        }

        [Obsolete("Please use InsertScreenBeforeForget<TBeforeScreen, TInsertionScreen>")]
        public virtual void InsertScreenBefore<TBeforeScreen, TInsertionScreen>()
            where TBeforeScreen : IScreen
            where TInsertionScreen : IScreen
        {
            InsertScreenBeforeAsync<TBeforeScreen, TInsertionScreen>().Forget();
        }

        public virtual void InsertScreenBeforeForget<TBeforeScreen, TInsertionScreen>()
            where TBeforeScreen : IScreen
            where TInsertionScreen : IScreen
        {
            InsertScreenBeforeAsync<TBeforeScreen, TInsertionScreen>().Forget();
        }

        public virtual async Task<IScreen> InsertScreenBeforeAsync<TBeforeScreen, TInsertionScreen>()
            where TBeforeScreen : IScreen
            where TInsertionScreen : IScreen
        {
            var screen = await InsertScreenBeforeAsync(typeof(TBeforeScreen), typeof(TInsertionScreen));
            return (TInsertionScreen)screen;
        }

        public virtual async Task<IScreen> InsertScreenBeforeAsync(Type beforeScreenClassType, Type insertionScreenClassType)
        {
            await SharedSemaphore.NavigationSemaphore.WaitAsync();
            try
            {
                return await StackNavigationService.InsertScreenBeforeAsync(beforeScreenClassType, insertionScreenClassType, this.Context);
            }
            finally
            {
                SharedSemaphore.NavigationSemaphore.Release();
            }
        }

        public InsertNavigation NextScreenParameter(object nextScreenParameter)
        {
            Context.NextScreenParameter = nextScreenParameter;
            return this;
        }

        public InsertNavigation IsCrossFade(bool isCrossFade)
        {
            Context.IsCrossFade = isCrossFade;
            return this;
        }

        public InsertNavigation SkipAnimation(bool skipAnimation)
        {
            Context.SkipAnimation = skipAnimation;
            return this;
        }

        public InsertNavigation SetSender(object sender)
        {
            Sender = sender;
            return this;
        }
    }
}