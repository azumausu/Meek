using Meek.UGUI;

namespace Meek.MVP
{
    public class MVPOption
    {
        public UGUIOption UGUIOption;
        public ServiceType<IPresenterLoaderFactory> PresenterLoaderFactory = new();
    }
}