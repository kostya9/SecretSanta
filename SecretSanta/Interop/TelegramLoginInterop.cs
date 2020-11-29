using System;
using Microsoft.JSInterop;

namespace SecretSanta.Interop
{
    public class TelegramLoginInterop
    {
        private readonly Action<string, string> _action;

        public TelegramLoginInterop(Action<string, string> action)
        {
            _action = action;
        }
        
        [JSInvokable("LoginWithTelegram")]
        public void LoginWithTelegram(string id, string login)
        {
            _action.Invoke(id, login);
        }
    }
}