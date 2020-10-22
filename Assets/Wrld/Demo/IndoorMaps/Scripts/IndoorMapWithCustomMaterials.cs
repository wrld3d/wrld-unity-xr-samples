using Wrld.Space;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace Wrld.Demo.IndoorMaps
{
    public class IndoorMapWithCustomMaterials : MonoBehaviour
    {
        private LatLong m_indoorMapLocation = LatLong.FromDegrees(56.460143, -2.978325);
        private Slider m_floorSlider;

        private string IndoorMapmaterialDirectory;
        
        private void OnEnable()
        {
            m_floorSlider = FindObjectOfType<Slider>();

            IndoorMapmaterialDirectory = "WrldMaterials/Archetypes";

            Api.Instance.IndoorMapsApi.IndoorMapTextureFetcher = new CustomIndoorMapTextureFetcher();
            Api.Instance.IndoorMapsApi.IndoorMapMaterialFactory = new CustomIndoorMapMaterialFactory(IndoorMapmaterialDirectory);

            Api.Instance.IndoorMapsApi.OnIndoorMapEntered += IndoorMapsApi_OnIndoorMapEntered;
            Api.Instance.IndoorMapsApi.OnIndoorMapExited += IndoorMapsApi_OnIndoorMapExited;
            
            Api.Instance.CameraApi.MoveTo(m_indoorMapLocation, distanceFromInterest: 500, headingDegrees: 0, tiltDegrees: 45);
        }

        private void OnDisable()
        {
            Api.Instance.IndoorMapsApi.OnIndoorMapExited -= IndoorMapsApi_OnIndoorMapExited;
            Api.Instance.IndoorMapsApi.OnIndoorMapEntered -= IndoorMapsApi_OnIndoorMapEntered;
        }

        public void OnExpand()
        {
            Api.Instance.IndoorMapsApi.ExpandIndoor();
        }

        public void OnCollapse()
        {
            Api.Instance.IndoorMapsApi.CollapseIndoor();
        }

        public void MoveUp()
        {
            Api.Instance.IndoorMapsApi.MoveUpFloor();
        }

        public void MoveDown()
        {
            Api.Instance.IndoorMapsApi.MoveDownFloor();
        }

        public void ExitMap()
        {
            Api.Instance.IndoorMapsApi.ExitIndoorMap();
        }

        public void EnterMap()
        {
            if (Api.Instance.IndoorMapsApi.GetActiveIndoorMap() == null)
            {
                Api.Instance.CameraApi.MoveTo(m_indoorMapLocation, distanceFromInterest: 500);
                Api.Instance.IndoorMapsApi.EnterIndoorMap("westport_house");
            }
        }

        public void OnSliderValueChanged()
        {
            Api.Instance.IndoorMapsApi.SetIndoorFloorInterpolation(m_floorSlider.value);
        }

        public void OnBeginDrag()
        {
            Api.Instance.IndoorMapsApi.ExpandIndoor();
        }

        public void OnEndDrag()
        {
            float sliderValue = m_floorSlider.value;
            int roundedValue = Mathf.RoundToInt(sliderValue);
            var map = Api.Instance.IndoorMapsApi.GetActiveIndoorMap();

            if (map != null)
            {
                int endFloorId = map.FloorIds[roundedValue];
                Api.Instance.IndoorMapsApi.SetCurrentFloorId(endFloorId);
                Api.Instance.IndoorMapsApi.CollapseIndoor();
            }
        }

        private void IndoorMapsApi_OnIndoorMapExited()
        {
        }

        private void IndoorMapsApi_OnIndoorMapEntered()
        {
            var map = Api.Instance.IndoorMapsApi.GetActiveIndoorMap();
            m_floorSlider.minValue = 0.0f;
            m_floorSlider.maxValue = map.FloorCount - 1.0f;
            var currentFloorId = Api.Instance.IndoorMapsApi.GetCurrentFloorId();
            m_floorSlider.value = Array.IndexOf(map.FloorIds, currentFloorId);
        }
    }
}