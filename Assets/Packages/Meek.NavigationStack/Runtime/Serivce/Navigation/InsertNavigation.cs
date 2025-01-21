using System;
using System.Threading.Tasks;

#nullable enable

namespace Meek.NavigationStack
{
    public class InsertNavigation
    {
        protected readonly StackNavigationService StackNavigationService;
        protected readonly InsertContext Context = new();
        protected object? Sender;

        public InsertNavigation(StackNavigationService stackNavigationService)
        {
            StackNavigationService = stackNavigationService;
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

        public virtual Task InsertScreenBeforeAsync<TBeforeScreen, TInsertionScreen>()
            where TBeforeScreen : IScreen
            where TInsertionScreen : IScreen
        {
            return InsertScreenBeforeAsync(typeof(TBeforeScreen), typeof(TInsertionScreen));
        }

        public virtual Task InsertScreenBeforeAsync(Type beforeScreenClassType, Type insertionScreenClassType)
        {
            return StackNavigationService.InsertScreenBeforeAsync(beforeScreenClassType, insertionScreenClassType,
                this.Context);
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