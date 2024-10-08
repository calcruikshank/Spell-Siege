using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System.Collections.Generic;

using Unity.Services.CloudSave;


#if UNITY_EDITOR
using ParrelSync;
#endif

public static class Authentication {
    public static string PlayerId { get; private set; }

    public static async Task Login() {
        if (UnityServices.State == ServicesInitializationState.Uninitialized) {
            var options = new InitializationOptions();


#if UNITY_EDITOR
            // Remove this if you don't have ParrelSync installed. 
            // It's used to differentiate the clients, otherwise lobby will count them as the same
            if (ClonesManager.IsClone()) options.SetProfile(ClonesManager.GetArgument());
            else options.SetProfile("Primary");
#endif
            
            await UnityServices.InitializeAsync(options);
        }

        if (!AuthenticationService.Instance.IsSignedIn) {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            var data = new Dictionary<string, object> { { "MySaveKey", "HelloWorld" } };
            await CloudSaveService.Instance.Data.ForceSaveAsync(data);
            PlayerId = AuthenticationService.Instance.PlayerId;
        }
    }
}