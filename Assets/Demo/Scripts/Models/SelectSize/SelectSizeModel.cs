using UniRx;

namespace Demo
{
    public class SelectSizeModel
    {
        private readonly ReactiveProperty<Size> _size = new();
        
        public IReadOnlyReactiveProperty<Size> Size => _size;

        public void UpdateSize(Size size)
        {
            _size.Value = size;
        }
    }
}