using UnityEngine;
using Wrld;
using Wrld.Precaching;
using Wrld.Space;

public class CancelPrecachingMapData : MonoBehaviour
{
    private void OnEnable()
    {
        // Start precaching resources in a 2000 meter radius around this point
        PrecacheOperation precacheOperation = Api.Instance.PrecacheApi.Precache(
            LatLong.FromDegrees(37.7952, -122.4028), 
            2000.0, 
            (_result) =>
            {
                Debug.LogFormat("Precaching {0}", _result.Succeeded ? "complete" : "cancelled");
            });
        // cancel the precache operation
        precacheOperation.Cancel();
    }
}

