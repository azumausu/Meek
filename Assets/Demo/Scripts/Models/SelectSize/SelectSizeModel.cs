using UniRx;

namespace Demo
{
    public class SelectSizeModel
    {
        private readonly ReactiveProperty<SizeType> _size = new();
        
        public IReadOnlyReactiveProperty<SizeType> Size => _size;

        public SelectSizeModel(int productId)
        {
            
        }

        public void UpdateSize(SizeType sizeType)
        {
            _size.Value = sizeType;
        }
    }
}