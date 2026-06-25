using UnityEngine;

namespace ProjectEclipse.World
{
    [RequireComponent(typeof(RoomBounds2D))]
    public class MapArea2D : MonoBehaviour
    {
        [SerializeField] private string areaId = "area";
        [SerializeField] private string displayName = "Map Area";
        [SerializeField] private Vector2 size = new Vector2(24f, 11.5f);

        private RoomBounds2D roomBounds;

        public string AreaId { get { return areaId; } }
        public string DisplayName { get { return displayName; } }
        public RoomBounds2D RoomBounds { get { return GetRoomBounds(); } }

        public void Configure(string id, string areaName, Vector2 boundsSize)
        {
            areaId = string.IsNullOrEmpty(id) ? areaId : id;
            displayName = string.IsNullOrEmpty(areaName) ? displayName : areaName;
            size = new Vector2(Mathf.Max(1f, boundsSize.x), Mathf.Max(1f, boundsSize.y));
            SyncBounds();
        }

        private void Reset()
        {
            SyncBounds();
        }

        private void OnValidate()
        {
            SyncBounds();
        }

        private RoomBounds2D GetRoomBounds()
        {
            if (roomBounds == null)
            {
                roomBounds = GetComponent<RoomBounds2D>();
            }

            return roomBounds;
        }

        private void SyncBounds()
        {
            RoomBounds2D bounds = GetRoomBounds();
            if (bounds != null)
            {
                bounds.Configure(size);
            }
        }
    }
}
