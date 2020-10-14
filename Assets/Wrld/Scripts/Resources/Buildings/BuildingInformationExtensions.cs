using UnityEngine;

namespace Wrld.Resources.Buildings
{
    public static class BuildingInformationExtensions
    {
        public static string ToJson(this BuildingInformation buildingInformation)
        {
            return JsonUtility.ToJson(buildingInformation.ToBuildingInformationDto());
        }
    }
}
