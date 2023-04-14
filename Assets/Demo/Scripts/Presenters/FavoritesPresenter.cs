using System;
using System.Collections.Generic;
using Meek.MVP;

namespace Demo
{
    public class FavoritesPresenter : Presenter<FavoritesModel>
    {
        protected override IEnumerable<IDisposable> Bind(FavoritesModel model)
        {
            yield break;
        }
    }
}