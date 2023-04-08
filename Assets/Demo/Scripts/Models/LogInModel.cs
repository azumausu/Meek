using System.Threading.Tasks;
using UniRx;

namespace Demo
{
    public class LogInModel
    {
        private ReactiveProperty<string> _email = new();
        private ReactiveProperty<string> _password = new();
        
        public IReadOnlyReactiveProperty<string> Email => _email;
        public IReadOnlyReactiveProperty<string> Password => _password;
        
        public void UpdateEmail(string value)
        {
            _email.Value = value;
        }
        
        public void UpdatePassword(string value)
        {
            _password.Value = value;
        }

        public async Task LogInAsync()
        {
            
        }
    }
}