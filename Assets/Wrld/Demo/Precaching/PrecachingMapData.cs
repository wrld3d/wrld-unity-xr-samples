using UnityEngine;
using Wrld;
using Wrld.Space;

public class PrecachingMapData : MonoBehaviour
{
    private void OnEnable()
    {
        // cache a 2000 meter radius around this point
        Api.Instance.PrecacheApi.Precache(LatLong.FromDegrees(37.7952, -122.4028), 2000.0, (_result) =>
            {
                Debug.LogFormat("Precaching {0}", _result.Succeeded ? "complete" : "failed");
            });
    }
}

