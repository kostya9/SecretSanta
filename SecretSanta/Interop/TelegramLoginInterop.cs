using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace SecretSanta.Interop
{
    public class TelegramLoginInterop
    {
        private readonly Func<string, string, Task> _action;

        public TelegramLoginInterop(Func<string, string, Task> action)
        {
            _action = action;
        }
        
        [JSInvokable("LoginWithTelegram")]
        public async Task LoginWithTelegram(string id, string login)
        {
            await _action.Invoke(id, login);
        }
    }
}