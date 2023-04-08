namespace Meek.MVP
{
    public interface IPresenterEventHandler
    {
        void PresenterDidInit(IPresenter sender);

        void PresenterDidSetup(IPresenter sender, object model);

        void PresenterDidBind(IPresenter sender, object model);

        void PresenterDidDeinit(IPresenter sender, object model);
    }
}