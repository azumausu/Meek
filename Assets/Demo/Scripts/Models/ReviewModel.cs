using UniRx;

namespace Demo
{
    public class ReviewModel
    {
        private readonly ReactiveProperty<int> _productId = new();
        
        public IReadOnlyReactiveProperty<int> ProductId => _productId;
        
        public ReviewModel(int productId)
        {
            _productId.Value = productId;
        }
    }
}