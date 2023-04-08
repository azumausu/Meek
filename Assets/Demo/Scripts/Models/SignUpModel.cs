using System.Threading.Tasks;
using UniRx;

namespace Demo
{
    public class SignUpModel
    {
        private ReactiveProperty<string> _name = new();
        private ReactiveProperty<string> _email = new();
        private ReactiveProperty<string> _password = new();
        
        public IReadOnlyReactiveProperty<string> Name => _name;
        public IReadOnlyReactiveProperty<string> Email => _email;
        public IReadOnlyReactiveProperty<string> Password => _password;
        
        public void UpdateName(string value)
        {
            _name.Value = value;
        }
        
        public void UpdateEmail(string value)
        {
            _email.Value = value;
        }
        
        public void UpdatePassword(string value)
        {
            _password.Value = value;
        }

        public async Task SignUpAsync()
        {
            
        }
    }
}