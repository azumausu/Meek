using System;
using System.Collections.Generic;
using Meek.MVP;

namespace Demo
{
    public class SearchPresenter : Presenter<SearchModel>
    {
        protected override IEnumerable<IDisposable> Bind(SearchModel model)
        {
            yield break;
        }
    }
}
