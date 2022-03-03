using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.Core;
using UnityEngine;

/*
* Note that you need to have a published script in order to use the Cloud Code SDK.
* You can do that from the Unity Dashboard - https://dashboard.unity3d.com/
*/
public class CloudCodeExample : MonoBehaviour
{
    /*
     * The Cloud Code parameters passed to the script.
     * The values are made accessible from the script code.
     */
    class CloudCodeRequest
    {
        public string name;
    }

    /*
     * The response from the script, used for deserialization.
     * In this example, the script would return a JSON in the format
     * {"welcomeMessage": "Hello, CloudCodeRequest.name. Welcome to Cloud Code!"}
     */
    class CloudCodeResponse
    {
        public string welcomeMessage;
    }

    /*
     * Initialize all Unity Services and Sign In an anonymous player.
     * You can perform this operation in a more centralized spot in your project
     */
    public async void Awake()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    /*
     * Populate a CloudCodeRequest object and invoke the script.
     * Deserialize the response into a CloudCodeResponse object
     */
    public async void OnClick()
    {
        var request = new CloudCodeRequest { name = "Player1" };
        var response = await CloudCode.CallEndpointAsync<CloudCodeResponse>("HelloWorld", request);
        Debug.Log(response.welcomeMessage);
    }

}