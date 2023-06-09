using System;
using Meek.UGUI;

namespace Meek.MVP
{
    public class MVPOption
    {
        public UGUIOption UGUIOption;
        private Type _presenterLoaderFactoryType;
        public Type PresenterLoaderFactoryType
        {
            get => _presenterLoaderFactoryType;
            set
            {
                value.AssertImplementation<IPresenterLoaderFactory>();
                _presenterLoaderFactoryType = value;
            }
        }
    }
}