using System;
using System.Collections.Generic;
using Meek.MVP;

namespace Demo
{
    public class ProfilePresenter : Presenter<ProfileModel>
    {
        protected override IEnumerable<IDisposable> Bind(ProfileModel model)
        {
            yield break;
        }
    }
}
