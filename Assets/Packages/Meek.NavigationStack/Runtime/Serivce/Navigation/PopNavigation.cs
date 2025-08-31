using System.Threading.Tasks;

#nullable enable

namespace Meek.NavigationStack
{
    public class PopNavigation
    {
        protected readonly NavigationSharedSemaphore SharedSemaphore;
        protected readonly StackNavigationService StackNavigationService;
        protected readonly PopContext Context = new();
        protected object? Sender;

        public PopNavigation(StackNavigationService stackNavigationService, NavigationSharedSemaphore sharedSemaphore)
        {
            StackNavigationService = stackNavigationService;
            SharedSemaphore = sharedSemaphore;
        }

        [System.Obsolete("Please use PopForget")]
        public virtual void Pop()
        {
            PopAsync().Forget();
        }

        public virtual void PopForget()
        {
            PopAsync().Forget();
        }

        public virtual async Task PopAsync()
        {
            await SharedSemaphore.NavigationSemaphore.WaitAsync();
            try
            {
                await StackNavigationService.PopAsync(Context);
            }
            finally
            {
                SharedSemaphore.NavigationSemaphore.Release();
            }
        }

        public virtual PopNavigation IsCrossFade(bool isCrossFade)
        {
            Context.IsCrossFade = isCrossFade;
            return this;
        }

        public virtual PopNavigation SkipAnimation(bool skipAnimation)
        {
            Context.SkipAnimation = skipAnimation;
            return this;
        }

        /// <summary>
        /// Pop processing is not performed if the Screen is not the specified Screen.
        /// </summary>
        public PopNavigation OnlyWhen(IScreen screen)
        {
            Context.OnlyWhenScreen = screen;
            return this;
        }

        public PopNavigation SetSender(object sender)
        {
            Sender = sender;
            return this;
        }
    }
}