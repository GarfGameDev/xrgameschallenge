using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Analytics;
using Unity.Services.Core;
using Unity.Services.Core.Environments;

public class AnalyticsManager : MonoBehaviour
{
    string consentIdentifier;
    bool isOptInConsentRequired;

    // Start is called before the first frame update
    async void Start()
    {
        try
        {
            var options = new InitializationOptions();
            options.SetEnvironmentName("production");
            await UnityServices.InitializeAsync();
            List<string> consentIdentifiers = await Events.CheckForRequiredConsents();
            if (consentIdentifiers.Count > 0)
            {
                consentIdentifier = consentIdentifiers[0];
                isOptInConsentRequired = consentIdentifier == "pipl";
            }
        }
        catch (ConsentCheckException e)
        {
            // Something went wrong when checking the GeoIP, check the e.Reason and handle appropriately
        }
    }

    public void CheckUserConsent()
    {
        try
        {
            if (isOptInConsentRequired)
            {
                // If consent is provided for both use and export
                Events.ProvideOptInConsent(consentIdentifier, true);

                // If consent is not provided
                Events.ProvideOptInConsent(consentIdentifier, false);
            }
        }
        catch (ConsentCheckException e)
        {
            // Handle the exception by checking e.Reason
        }
    }


}
